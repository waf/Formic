using Formic.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;

namespace Formic.Utility
{
    public static class EditorGenerator
    {
        public static IHtmlContent EditorProperty(IHtmlHelper helper, object record, PropertySchema property)
        {
            return (property.Property is INavigation) ?
                EditorNavigationProperty(helper, record, property) :
                EditorDataProperty(helper, record, property);
        }

        private static IHtmlContent EditorNavigationProperty(IHtmlHelper helper, object record, PropertySchema propertySchema, object htmlAttributes = null)
        {
            INavigation property = propertySchema.Property as INavigation;
            object propertyValue = record == null ? null : property.GetGetter().GetClrValue(record);

            var lookupType = property.ForeignKey.PrincipalEntityType;
            IQueryable<object> lookupData = Reflection.GetDbSetForType(new FormicDbContext(), lookupType);

            var primaryKeyGetter = EFUtils.GetPrimaryKeyProperty(lookupType).GetGetter();
            var items = (from result in lookupData
                         let value = primaryKeyGetter.GetClrValue(result).ToString()
                         select new SelectListItem
                         {
                             Text = result.ToString(),
                             Value = value,
                             Selected = value.Equals(propertyValue)
                         })
                         .ToList();
            return helper.DropDownList(property.ForeignKey.Properties.First().Name, items, "", htmlAttributes);
        }

        private static IHtmlContent EditorDataProperty(IHtmlHelper helper, object record, PropertySchema propertySchema, object htmlAttributes = null)
        {
            IProperty property = propertySchema.Property as IProperty;
            if(property.GetContainingForeignKeys().Any())
            {
                // editing and creating this property should be handled by the INavigation entity property, rather than the raw key
                return new StringHtmlContent(string.Empty);
            }
            object propertyValue = record == null ? null : property.GetGetter().GetClrValue(record);
            return GeneratorUtils.LoadView(helper, "Edit", propertyValue, property) ??
                helper.TextBox(property.Name, propertyValue);
        }
    }
}
