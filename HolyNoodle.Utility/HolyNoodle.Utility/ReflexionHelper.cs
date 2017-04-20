using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility
{
    public static class ReflexionHelper
    {
        public static bool IsValueType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return type.IsValueType || type == typeof(string);
        }

        public static Type GetGenericTypeDefintion(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            var generics = type.GetGenericArguments();
            return generics.Count() > 0 ? generics[0] : null;
        }

        public static bool IsEnumarable(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return (type.GetInterface("IEnumerable") != null);
        }
    }
}
