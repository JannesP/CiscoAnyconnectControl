using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CiscoAnyconnectControl.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// from: https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/
        /// </summary>
        /// <typeparam name="TResult">the return type of the task</typeparam>
        /// <param name="task">the task to run</param>
        /// <param name="timeout">the timeout</param>
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

        /// <summary>
        /// from: https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/
        /// </summary>
        /// <param name="task">the task to run</param>
        /// <param name="timeout">the timeout</param>
        /// <exception cref="TimeoutException">Is thrown if the task timed out.</exception>
        /// <returns>the result from the given task</returns>
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
