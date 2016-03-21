using Microsoft.AspNet.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.ViewFeatures.Internal;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Entity;
using Formic.Controllers;
using Microsoft.AspNet.Routing;

namespace Formic.Utility
{
    public static class ViewHelper
    {
        public static IHtmlContent EditorProperty(this IHtmlHelper helper, object record, IPropertyBase property)
        {
            object propertyValue = property.GetGetter().GetClrValue(record);
            return helper.TextBox(property.Name, propertyValue);
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
            queryParameters["table"] = relation.ForeignEntityType;

            return helper.ActionLink("View", "ListRecords", "Home", queryParameters, null);
        }
        public static IHtmlContent DisplayProperty(this IHtmlHelper helper, object record, IProperty property)
        {
            const string ViewPrefix = "DisplayTemplates/";

            object propertyValue = property.GetGetter().GetClrValue(record);
            Func<string, IHtmlContent> Partial = str => helper.Partial(ViewPrefix + str, propertyValue);

            // use user-specified template
            var uiHint = property.DeclaringEntityType.ClrType
                .GetProperty(property.Name)
                .GetCustomAttribute(typeof(UIHintAttribute)) as UIHintAttribute;
            if(uiHint != null)
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

        private static string CreatePartialName(IPropertyBase property)
        {
            return property.DeclaringEntityType.Name + "." + property.Name;
        }

        private static string CreatePartialName(Type propertyType)
        {
            return propertyType.Name;
        }
    }
}
