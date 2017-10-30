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


        /// <summary>
        /// from: https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task">the task to run</param>
        /// <param name="timeout">the timeout in ms</param>
        /// <exception cref="TimeoutException">Is thrown if the task timed out.</exception>
        /// <returns>the result from the given task</returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await task;  // Very important in order to propagate exceptions
                    return;
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}
