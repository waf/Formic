using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formic.Models
{
    public class RecordSet
    {
        public PropertySchema[] Properties { get; set; }
        public object[] Data { get; set; }
        public string EntityName { get; set; }
    }
}
