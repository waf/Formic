using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Formic.Models;

namespace Formic.Utility
{
    public static class ViewHelper
    {
        public static IHtmlContent EditorProperty(this IHtmlHelper helper, object record, PropertySchema property, object htmlAttributes = null)
        {
            return EditorGenerator.EditorProperty(helper, record, property);
        }
        public static IHtmlContent DisplayProperty(this IHtmlHelper helper, object record, PropertySchema property)
        {
            return DisplayGenerator.DisplayProperty(helper, record, property);
        }
    }
}
