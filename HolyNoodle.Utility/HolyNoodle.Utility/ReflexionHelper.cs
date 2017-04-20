using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public static MethodInfo GetMethod(this Type type, string name, bool generic)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            return type.GetMethods()
                .FirstOrDefault(method => method.Name == name & method.IsGenericMethod == generic);
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
