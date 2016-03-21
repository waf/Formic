using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Formic.Utility
{
    public class Utility
    {
        public static T Error<T>(string msg)
        {
            throw new Exception(msg);
        }

        public static object Convert(string value, Type t)
        {
            return TypeDescriptor.GetConverter(t).ConvertFromInvariantString(value);
        }
    }
}
