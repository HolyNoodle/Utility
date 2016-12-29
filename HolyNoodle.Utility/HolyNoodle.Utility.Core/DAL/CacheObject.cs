using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.DAL
{
    public class CacheObject : Attribute
    {
        public List<Type> Types { get; internal set; }
        public bool DoNotCache { get; set; }

        public CacheObject(params Type[] cacheImpactedTypes)
        {
            Types = cacheImpactedTypes.ToList();
            DoNotCache = false;
        }

        public bool IsImpacted(IDbProcedure procedure)
        {
            return Types.Any(t => t.FullName == procedure.GetType().FullName);
        }
    }
}
