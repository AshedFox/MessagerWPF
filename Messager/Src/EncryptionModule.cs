using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Messager
{
    public static class EncryptionModule
    {
        public static string EcryptPassword(string password)
        {
            return Encoding.UTF8.GetString(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
