using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.CiscoCliHelper
{
    class CliProcess : Process
    {
        public CliProcess(string path)
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = "-s",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                ErrorDialog = true
            };
        }
    }
}
