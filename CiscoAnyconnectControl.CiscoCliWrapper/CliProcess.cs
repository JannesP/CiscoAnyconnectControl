using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CiscoAnyconnectControl.Utility;

namespace CiscoAnyconnectControl.CiscoCliHelper
{
    class CliProcess : Process
    {
        private readonly SemaphoreSlim _semaphoreWriteInput = new SemaphoreSlim(1, 1);
        public CliProcess(string path)
        {
            this.StartInfo = new ProcessStartInfo
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

        public enum Command
        {
            Connect, Disconnect, Stats, State
        }

        public async Task SendCommand(Command command, string param = null)
        {
            string cmd = command.ToString().ToLower();
            if (param != null)
            {
                cmd += $" {param}";
            }
            try
            {
                await this._semaphoreWriteInput.WaitAsync();
                try
                {
                    await this.StandardInput.WriteLineAsync(cmd);
                }
                catch (Exception ex)
                {
                    Utility.Util.TraceException("Error writing to cli stdin.", ex);
                }
            }
            finally
            {
                this._semaphoreWriteInput.Release();
            }
        }

        public async Task SendCompleteConnect(string host, string username, string password, string group)
        {
            string connectCommand = string.Format("{1}{0}{2}{0}{3}{0}{4}"
                , Environment.NewLine
                , $"connect {host}"
                , group
                , username
                , password
            );
            try
            {
                await this._semaphoreWriteInput.WaitAsync();
                try
                {
                    await this.StandardInput.WriteLineAsync(connectCommand);
                }
                catch (Exception ex)
                {
                    Utility.Util.TraceException("Error writing to cli stdin.", ex);
                }
            }
            finally
            {
                this._semaphoreWriteInput.Release();
            }
        }
    }
}
