using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UMS.Serialization
{
    public static class Extensions
    {
        public static string ToJson(this object obj)
        {
            return JsonSerializer.ToJson(obj);
        }
    }
}
