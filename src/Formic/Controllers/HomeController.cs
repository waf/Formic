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

namespace Formic.Controllers
{
    /*
        / - list tables
        /Blog - list records
        /Blog/123 - view
        /Blog/123/edit - edit

    */
    [Route("")]
    public class HomeController : Controller
    {
        DbContext db = new FormicdbContext();

        [HttpGet]
        [Route("")]
        public IActionResult ListTables()
        {
            var tables = db.Model
                .GetEntityTypes()
                .Select(type => type.Name)
                .ToList();

            return View(tables);
        }

        [HttpGet]
        [Route("{table}/{id}/edit")]
        public IActionResult EditPage(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            var record = GetByPrimaryKey(entity, id);

            return record != null ?
                View(CreateViewModelForEntity(entity, record)) :
                new HttpNotFoundResult() as IActionResult;
        }

        [HttpPost]
        [Route("{table}/{id}/edit")]
        public IActionResult EditRecord(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            var record = GetByPrimaryKey(entity, id);

            foreach (var prop in entity.GetProperties())
            {
                if(this.Request.Form.ContainsKey(prop.Name))
                {
                    var value = this.Request.Form[prop.Name].Single();
                    var parsedValue = Convert(value, prop.ClrType);
                    prop.GetSetter().SetClrValue(record, parsedValue);
                }
            }
            db.SaveChanges();

            return RedirectToAction("ListRecords");
        }

        [HttpPost]
        [HttpDelete]
        [Route("{table}/{id}/delete")]
        public IActionResult DeleteRecord(string table, string id)
        {
            IEntityType entity = db.Model.FindEntityType(table);

            var t = Activator.CreateInstance(entity.ClrType);
            var pk = entity.FindPrimaryKey().Properties.First();
            pk.GetSetter().SetClrValue(t, Convert(id, pk.ClrType));
            db.Attach(t);
            db.Remove(t);
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

        [HttpGet]
        [Route("{table}/")]
        public IActionResult ListRecords(string table)
        {
            IEntityType entity = db.Model.FindEntityType(table);
            //MetadataProvider.GetMetadataForProperties(entity.ClrType);
            // TODO: cache reflection
            IQueryable<object> results = Reflection.GetDbSetForType(db, entity);

            if (Request.Query.Any())
            {
                results = Expressions.FilterByEntityParameters(results, entity, Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()));
            }

            //ExpressionMetadataProvider.FromStringExpression()
            //TODO: paging, async
            var meta = MetadataProvider.GetMetadataForType(entity.ClrType);
            var model = CreateViewModelForEntity(entity, results.ToArray());
            return View(model);
        }

        private RecordSet CreateViewModelForEntity(IEntityType type, params Object[] entities)
        {
            return new RecordSet
            {
                Properties = type.GetProperties().ToArray(),
                NavigationProperties = type.GetNavigations().ToArray(),
                Data = entities
            };
        }
    }
}
