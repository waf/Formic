using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formic.Models;
using Formic.Utility;
using static Formic.Utility.Utility;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace Formic.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        readonly DbContext db = new FormicdbContext();

        [HttpGet("")]
        public IActionResult ListTables()
        {
            var tables = db.Model
                .GetEntityTypes()
                .Select(type => type.Name)
                .ToList();

            return View(tables);
        }

        [HttpGet("{table}/{id}/edit")]
        public IActionResult EditPage(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return HttpNotFound();

            var record = GetByPrimaryKey(entity, id);

            return record != null ?
                View(CreateViewModelForEntity(entity, record)) :
                new HttpNotFoundResult() as IActionResult;
        }

        [HttpPost("{table}/{id}/edit")]
        [HttpPut("{table}/{id}/")]
        public IActionResult EditRecord(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return HttpNotFound();

            var record = GetByPrimaryKey(entity, id);

            //TODO: proper async
            TryUpdateModelAsync(record, entity.ClrType, "").Wait();

            db.SaveChanges();

            return RedirectToAction("ListRecords");
        }

        [HttpDelete("{table}/{id}/")]
        [HttpPost("{table}/{id}/delete")]
        public IActionResult DeleteRecord(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return HttpNotFound();

            // create a new entity object, set the primary key, and delete it.
            // this is so we can issue just a DELETE, rather than a SELECT then DELETE.
            var t = Activator.CreateInstance(entity.ClrType);
            var pk = entity.FindPrimaryKey().Properties.First();
            pk.GetSetter().SetClrValue(t, Convert(id, pk.ClrType));
            db.Attach(t);
            db.Remove(t);
            db.SaveChanges();

            return RedirectToAction("ListRecords");
        }

        [HttpGet("{table}/create")]
        public IActionResult CreatePage(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return HttpNotFound();
            return View(CreateViewModelForEntity(entity));
        }

        [HttpPost("{table}/")]
        [HttpPost("{table}/create")]
        public IActionResult CreateRecord(string table)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return HttpNotFound();

            var record = Activator.CreateInstance(entity.ClrType);
            //TODO: proper async
            // update model using modelbinder
            TryUpdateModelAsync(record, entity.ClrType, "").Wait();
            // empty out the pk, EF or the DB will generate a new one.
            var pk = entity.FindPrimaryKey().Properties.First();
            var emptyPk = pk.ClrType.GetTypeInfo().IsValueType ? Activator.CreateInstance(pk.ClrType) : null;
            pk.GetSetter().SetClrValue(record, emptyPk);
                
            db.Add(record);
            db.SaveChanges();
            return RedirectToAction("ListRecords");
        }

        private object GetByPrimaryKey(IEntityType entity, string id)
        {
            var primaryKey = entity.FindPrimaryKey();
            if (primaryKey == null)
            {
                throw new Exception("Could not find primary key for " + entity.Name);
            }
            // todo: composite support?
            var pkQuery = new Dictionary<string, string[]>
            {
                {primaryKey.Properties.First().Name, new[] {id} }
            };

            IQueryable<object> results = Reflection.GetDbSetForType(db, entity);
            return Expressions.FilterByEntityParameters(results, entity, pkQuery).SingleOrDefault();
        }

        [HttpGet("{table}/")]
        public IActionResult ListRecords(string table)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return HttpNotFound();
            // TODO: cache reflection
            IQueryable<object> results = Reflection.GetDbSetForType(db, entity);

            if (Request.Query.Any())
            {
                results = Expressions.FilterByEntityParameters(results, entity, Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
            }

            //TODO: paging, async
            var model = CreateViewModelForEntity(entity, results.ToArray());
            return View(model);
        }

        private RecordSet CreateViewModelForEntity(IEntityType type, params object[] entities)
        {
            var mvcMetadata = MetadataProvider.GetMetadataForProperties(type.ClrType);
            var efMetadata = type.GetPropertiesAndNavigations();

            var metadata = mvcMetadata.Join(efMetadata,
                mvc => mvc.PropertyName,
                ef => ef.Name,
                (mvc, ef) => new { mvc, ef })
                .OrderBy(propertyMetadata => propertyMetadata.mvc.Order)
                .Select(md => new PropertySchema
                {
                    Description = md.mvc.GetDisplayName(),
                    Property = md.ef
                }).ToArray();

            return new RecordSet
            {
                Properties = metadata,
                Data = entities
            };
        }
    }
}
