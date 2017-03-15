using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formic.Utility
{
    public static class EFUtils
    {

        // todo: composite support?
        public static IProperty GetPrimaryKeyProperty(IEntityType entity)
        {
            var primaryKey = entity.FindPrimaryKey();
            return primaryKey?.Properties?.FirstOrDefault();
        }
    }
}
