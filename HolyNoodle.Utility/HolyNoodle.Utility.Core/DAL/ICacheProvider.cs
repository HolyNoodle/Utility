using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.DAL
{
    public interface ICacheProvider : IDisposable
    {
        void Cache(IDbProcedure key, object value);
        T Get<T>(IDbProcedure key);
    }
}
