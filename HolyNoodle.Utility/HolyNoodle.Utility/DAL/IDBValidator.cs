using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.DAL
{
    public interface IDbValidator
    {
        Tuple<bool, Dictionary<string, string>> Validate(IDb context);
    }
}
