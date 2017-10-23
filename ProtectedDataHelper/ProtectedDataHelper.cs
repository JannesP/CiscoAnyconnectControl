using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.Util
{
    public class ProtectedDataHelper
    {
        private static byte[] Salt => new byte[]{ 236, 74, 66, 6, 97, 113, 25, 69, 211, 75 };


        public static byte[] Protect(byte[] data)
        {
            return ProtectedData.Protect(data, Salt, DataProtectionScope.LocalMachine);
        }

        public static byte[] Unprotect(byte[] data)
        {
            return ProtectedData.Unprotect(data, Salt, DataProtectionScope.LocalMachine);
        }

    }
}
