using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProtectedDataHelper
{
    class ProtectedDataHelper
    {
        private static byte[] Salt => new byte[]{ 236, 74, 66, 6, 97, 113, 25, 69, 211, 75 };


        public static byte[] Protect(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, Salt, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not encrypted. An error occurred.");
                return null;
            }
        }

        public static byte[] Unprotect(byte[] data)
        {
            try
            {
                return ProtectedData.Unprotect(data, Salt, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Data was not decrypted. An error occurred.");
                return null;
            }
        }

    }
}
