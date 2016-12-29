using HolyNoodle.Utility.Dal;
using System.Security.Cryptography;
using System.Text;

namespace HolyNoodle.Utility.Crypto
{
    public class PasswordCrypter : ICrypter
    {
        public object Crypt(object password)
        {
            using (var algorithm = SHA256.Create())
            {
                byte[] data = Encoding.ASCII.GetBytes(password.ToString());
                data = algorithm.ComputeHash(data);
                return Encoding.ASCII.GetString(data);
            }
        }

        public object Decrypt(object cryptedData)
        {
            return cryptedData;
        }
    }
}
