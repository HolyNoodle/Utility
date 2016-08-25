using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.Dal
{
    public interface ICrypter
    {
        object Crypt(object data);
        object Decrypt(object cryptedData);
    }
}
