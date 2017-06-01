using Formic.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Formic.Utility
{
    public class GeneratorUtils
    {
        public static IHtmlContent LoadView(IHtmlHelper helper, string viewPrefix, object propertyValue, IPropertyBase property)
        {
            IHtmlContent Partial(string partialFileName) =>
                helper.Partial(Path.Combine(viewPrefix + "Templates", partialFileName), propertyValue, new ViewDataDictionary(helper.ViewData)
                {
                    { "label", property.Name }
                });

            // use user-specified template
            var attribute = property.DeclaringEntityType.ClrType
                .GetProperty(property.Name)
                .GetCustomAttribute(typeof(UIHintAttribute));
            if (attribute is UIHintAttribute uiHint && uiHint.PresentationLayer.ToUpperInvariant() == viewPrefix.ToUpperInvariant())
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
            return null;
        }

        private static string CreatePartialName(UIHintAttribute attr)
        {
            return attr.UIHint;
        }

        private static string CreatePartialName(IPropertyBase property)
        {
            return property.DeclaringType.Name + "." + property.Name;
        }

        private static string CreatePartialName(Type propertyType)
        {
            return propertyType.Name;
        }
    }
}
