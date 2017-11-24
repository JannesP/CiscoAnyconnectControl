using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.Utility
{
    public static class Util
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBasePseudoUrl = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                const string filePrefix3 = @"file:///";
                if (codeBasePseudoUrl.StartsWith(filePrefix3))
                {
                    string sPath = codeBasePseudoUrl.Substring(filePrefix3.Length);
                    string bsPath = sPath.Replace('/', '\\');
                    Console.WriteLine("bsPath: " + bsPath);
                    string fp = Path.GetDirectoryName(bsPath);
                    Console.WriteLine("fp: " + fp);
                    return fp;
                }
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        public static void TraceException(string line1, Exception ex)
        {
            Trace.TraceError($"{line1}: {ex.GetType()}\nMessage: {ex.Message}\nStack Trace:\n{ex.StackTrace}");
        }
    }
}
