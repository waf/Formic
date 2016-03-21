using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Formic.Utility
{
    public class Reflection
    {
        public static PropertyInfo GetClrPropertyForEntity(IEntityType entity, string property)
        {
            return entity.ClrType.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }


        public static IQueryable<object> GetDbSetForType(DbContext db, IEntityType entity)
        {
            return (IQueryable<object>)db.GetType()
                .GetMethod("Set")
                .MakeGenericMethod(entity.ClrType)
                .Invoke(db, null);
        }
    }
}
