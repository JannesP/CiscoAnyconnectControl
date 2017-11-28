using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CiscoAnyconnectControl.Utility
{
    public static class Util
    {
        public static string AssemblyDirectory
        {
            get { return Path.GetDirectoryName(FullAssemblyPath); }
        }

        public static string FullAssemblyPath
        {
            get
            {
                string codeBasePseudoUrl = Assembly.GetEntryAssembly().CodeBase;
                const string filePrefix3 = @"file:///";
                if (codeBasePseudoUrl.StartsWith(filePrefix3))
                {
                    string sPath = codeBasePseudoUrl.Substring(filePrefix3.Length);
                    string bsPath = sPath.Replace('/', '\\');
                    Console.WriteLine("bsPath: " + bsPath);
                    return bsPath;
                }
                return Assembly.GetExecutingAssembly().Location;
            }
        }

        public static string AssemblyName
        {
            get { return Path.GetFileName(FullAssemblyPath); }
        }

        /// <summary>
        /// Checks for any known Cisco interfaces that are still running and asks the user if he wants to close them.
        /// </summary>
        /// <returns>true if there are no other cisco interfaces running</returns>
        public static bool CheckForCiscoProcesses(bool close = false)
        {
            bool IsCiscoProcFunc(Process p)
            {
                switch (p.ProcessName)
                {
                    case "vpncli":
                    case "vpnui":
                        return true;
                    default:
                        return false;
                }
            }

            List<Process> exe = Process.GetProcesses().Where(IsCiscoProcFunc).ToList();
            if (close)
            {
                foreach (Process proc in exe)
                {
                    try
                    {
                        proc.CloseMainWindow();
                        proc.WaitForExit(50);
                        proc.Kill();
                    }
                    catch (Exception ex)
                    {
                        Util.TraceException("Error closing cisco process:", ex);
                        return false;
                    }
                }
                exe.Clear();
            }
            return exe.Count == 0;
        }

        public static void TraceException(string line1, Exception ex)
        {
            Trace.TraceError($"{line1}: {ex.GetType()}\nMessage: {ex.Message}\nStack Trace:\n{ex.StackTrace}");
        }
    }
}
