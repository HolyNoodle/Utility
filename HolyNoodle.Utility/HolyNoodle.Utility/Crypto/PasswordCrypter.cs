using HolyNoodle.Utility.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.Crypto
{
    public class PasswordCrypter : ICrypter
    {
        public object Crypt(object password)
        {
            byte[] data = Encoding.ASCII.GetBytes(password.ToString());
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return Encoding.ASCII.GetString(data);
        }

        public object Decrypt(object cryptedData)
        {
            return cryptedData;
        }
    }
}
