using Microsoft.EntityFrameworkCore.Metadata;

namespace Formic.Models
{
    public class PropertySchema
    {
        public string Description { get; set; }
        public bool IsPrimaryKey { get; internal set; }
        public IPropertyBase Property { get; set; }
    }
}
