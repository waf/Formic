using Formic.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formic.Utility
{
    public static class DisplayGenerator
    {
        public static IHtmlContent DisplayProperty(IHtmlHelper helper, object record, PropertySchema property)
        {
            return (property.Property is INavigation) ?
                DisplayNavigationProperty(helper, record, property) :
                DisplayDataProperty(helper, record, property);
        }

        private static IHtmlContent DisplayNavigationProperty(IHtmlHelper helper, object record, PropertySchema propertySchema)
        {
            INavigation property = propertySchema.Property as INavigation;
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

        private static IHtmlContent DisplayDataProperty(IHtmlHelper helper, object record, PropertySchema propertySchema)
        {
            IProperty property = propertySchema.Property as IProperty;
            object propertyValue = property.GetGetter().GetClrValue(record);

            return GeneratorUtils.LoadView(helper, "Display", propertyValue, property) ??
                new StringHtmlContent(propertyValue.ToString());
        }
    }
}
