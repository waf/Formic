using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Formic.Models;
using Formic.Utility;
using static Formic.Utility.Utility;
using System.Reflection;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Formic.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        public readonly DbContext db = new FormicDbContext();

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{table}/{id}/edit")]
        public async Task<IActionResult> EditPage(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if (entity == null) return NotFound();

            var record = await GetByPrimaryKey(entity, id);

            return record != null ?
                View(CreateViewModelForEntity(entity, record)) :
                NotFound() as IActionResult;
        }

        [HttpPost("{table}/{id}/edit")]
        public async Task<IActionResult> EditRecord(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return NotFound();

            var record = await GetByPrimaryKey(entity, id);

            await TryUpdateModelAsync(record, entity.ClrType, "");
            if(!ModelState.IsValid)
            {
                return View("EditPage", CreateViewModelForEntity(entity, record));
            }

            await db.SaveChangesAsync();

            return RedirectToAction("ListRecords");
        }

        [HttpPost("{table}/{id}/delete")]
        public async Task<IActionResult> DeleteRecord(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return NotFound();

            // create a new entity object, set the primary key, and delete it.
            // this is so we can issue just a DELETE, rather than a SELECT then DELETE.
            var t = Activator.CreateInstance(entity.ClrType);
            var pk = entity.FindPrimaryKey().Properties.First();
            pk.GetSetter().SetClrValue(t, Convert(id, pk.ClrType));
            db.Attach(t);
            db.Remove(t);
            await db.SaveChangesAsync();

            return RedirectToAction("ListRecords");
        }

        [HttpGet("{table}/create")]
        public IActionResult CreatePage(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return NotFound();
            return View(CreateViewModelForEntity(entity));
        }

        [HttpPost("{table}/create")]
        public async Task<IActionResult> CreateRecord(string table)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return NotFound();

            var record = Activator.CreateInstance(entity.ClrType);

            // update model using modelbinder
            await TryUpdateModelAsync(record, entity.ClrType, "");
            if(!ModelState.IsValid)
            {
                return View("CreatePage", CreateViewModelForEntity(entity, record));
            }
            // empty out the pk, EF or the DB will generate a new one.
            //var pk = entity.FindPrimaryKey().Properties.First();
            //var emptyPk = pk.ClrType.GetTypeInfo().IsValueType ? Activator.CreateInstance(pk.ClrType) : null;
            //pk.GetSetter().SetClrValue(record, emptyPk);

            db.Add(record);
            await db.SaveChangesAsync();
            return RedirectToAction("ListRecords");
        }

        private Task<object> GetByPrimaryKey(IEntityType entity, string id)
        {
            var primaryKey = EFUtils.GetPrimaryKeyProperty(entity);
            if (primaryKey == null)
            {
                throw new Exception("Could not find primary key for " + entity.Name);
            }
            var pkQuery = new Dictionary<string, string[]>
            {
                {primaryKey.Name, new[] {id} }
            };

            IQueryable<object> results = Reflection.GetDbSetForType(db, entity);
            return Expressions
                .FilterByEntityParameters(results, entity, pkQuery)
                .SingleOrDefaultAsync();
        }

        [HttpGet("{table}/")]
        public async Task<IActionResult> ListRecords(string table)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            if(entity == null) return NotFound();
            // TODO: cache reflection
            IQueryable<object> query = Reflection.GetDbSetForType(db, entity);

            if (Request.Query.Any())
            {
                var queryParams = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
                query = Expressions.FilterByEntityParameters(query, entity, queryParams);
            }

            //TODO: paging
            var results = await query.ToArrayAsync();
            var model = CreateViewModelForEntity(entity, results);
            return View(model);
        }

        private RecordSet CreateViewModelForEntity(IEntityType type, params object[] entities)
        {
            var mvcMetadata = MetadataProvider.GetMetadataForProperties(type.ClrType);
            var efMetadata = type.GetPropertiesAndNavigations();

            var primaryKeys = type.FindPrimaryKey().Properties;
            var metadata = mvcMetadata
                .Join(efMetadata,
                      mvc => mvc.PropertyName,
                      ef => ef.Name,
                      (mvc, ef) => new { mvc, ef })
                .OrderBy(propertyMetadata => propertyMetadata.mvc.Order)
                .Select(md => new PropertySchema
                {
                    Description = md.mvc.GetDisplayName(),
                    Property = md.ef,
                    IsPrimaryKey = primaryKeys.Contains(md.ef)
                }).ToArray();

            return new RecordSet
            {
                EntityName = type.Name,
                Properties = metadata,
                Data = entities
            };
        }
    }
}
