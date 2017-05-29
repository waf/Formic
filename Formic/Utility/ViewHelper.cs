using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Formic.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace Formic.Utility
{
    public static class ViewHelper
    {
        public static IHtmlContent EditorProperty(this IHtmlHelper helper, object record, IPropertyBase property, object htmlAttributes = null)
        {
            return (property is INavigation) ?
                helper.EditorProperty(record, property as INavigation) :
                helper.EditorProperty(record, property as IProperty);
        }
        public static IHtmlContent EditorProperty(this IHtmlHelper helper, object record, INavigation property, object htmlAttributes = null)
        {
            object propertyValue = record == null ? null : property.GetGetter().GetClrValue(record);

            var lookupType = property.ForeignKey.PrincipalEntityType;
            IQueryable<object> lookupData = Reflection.GetDbSetForType(HomeController.db, lookupType);

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
        public static IHtmlContent EditorProperty(this IHtmlHelper helper, object record, IProperty property, object htmlAttributes = null)
        {
            if(property.GetContainingForeignKeys().Any())
            {
                // editing and creating this property should be handled by the INavigation entity property, rather than the raw key
                return new StringHtmlContent(string.Empty);
            }
            object propertyValue = record == null ? null : property.GetGetter().GetClrValue(record);
            return helper.TextBox(property.Name, propertyValue, htmlAttributes);
        }
        public static IHtmlContent DisplayProperty(this IHtmlHelper helper, object record, IPropertyBase property)
        {
            return (property is INavigation) ?
                helper.DisplayProperty(record, property as INavigation) :
                helper.DisplayProperty(record, property as IProperty);
        }
        public static IHtmlContent DisplayProperty(this IHtmlHelper helper, object record, INavigation property)
        {
            var relation = property.IsCollection() ?
                new // one-to-many relation
                {
                    ForeignEntityType = property.ForeignKey.DeclaringEntityType,
                    ForeignEntityProperties = property.ForeignKey.Properties,
                    PrincipalProperties = property.ForeignKey.PrincipalKey.Properties,
                } :
                new // many-to-one or one-to-one relation
                {
                    ForeignEntityType = property.ForeignKey.PrincipalEntityType,
                    ForeignEntityProperties = property.ForeignKey.PrincipalKey.Properties,
                    PrincipalProperties = property.ForeignKey.Properties,
                };

            var queryParameters = relation.ForeignEntityProperties
                .Zip(relation.PrincipalProperties, (foreign, principal) => new { Parameter = foreign.Name, Value = principal.GetGetter().GetClrValue(record) })
                .ToDictionary(kvp => kvp.Parameter, kvp => kvp.Value);
            queryParameters["table"] = relation.ForeignEntityType.Name;

            return helper.ActionLink("View", "ListRecords", "Home", queryParameters, null);
        }
        public static IHtmlContent DisplayProperty(this IHtmlHelper helper, object record, IProperty property)
        {
            const string ViewPrefix = "DisplayTemplates/";
            object propertyValue = property.GetGetter().GetClrValue(record);

            IHtmlContent Partial(string partialFileName) =>
                helper.Partial(ViewPrefix + partialFileName, propertyValue, new ViewDataDictionary(helper.ViewData)
                {
                    { "label", property.Name }
                });

            // use user-specified template
            var attribute = property.DeclaringEntityType.ClrType
                .GetProperty(property.Name)
                .GetCustomAttribute(typeof(UIHintAttribute));
            if (attribute is UIHintAttribute uiHint)
            {
                return Partial(CreatePartialName(uiHint));
            }

            //TODO: this is gross exception-based control-flow
            // template by fully qualified name
            try { return Partial(CreatePartialName(property)); }
            catch (InvalidOperationException) { }

            // template by type
            try { return Partial(CreatePartialName(property.ClrType)); }
            catch (InvalidOperationException) { }

            // fallback, just display the value as a string, with no template.
            return new StringHtmlContent(propertyValue.ToString());
        }

        private static string CreatePartialName(UIHintAttribute attr)
        {
            return attr.UIHint;
        }

        private static string CreatePartialName(IProperty property)
        {
            return property.DeclaringEntityType.Name + "." + property.Name;
        }

        private static string CreatePartialName(Type propertyType)
        {
            return propertyType.Name;
        }
    }
}
