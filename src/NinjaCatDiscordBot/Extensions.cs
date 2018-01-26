/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: Extensions.cs
* 
* Copyright (c) 2016-2017 John Davis
*
* Permission is hereby granted, free of charge, to any person obtaining a
* copy of this software and associated documentation files (the "Software"),
* to deal in the Software without restriction, including without limitation
* the rights to use, copy, modify, merge, publish, distribute, sublicense,
* and/or sell copies of the Software, and to permit persons to whom the
* Software is furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
* IN THE SOFTWARE.
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using Discord;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NinjaCatDiscordBot
{
    internal static class Extensions
    {
        #region Methods

        public static bool HasStringPrefixLower(this IUserMessage msg, string str, ref int argPos)
        {
            var text = msg.Content.ToLowerInvariant();
            if (text.StartsWith(str))
            {
                argPos = str.Length;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Waits asynchronously for the process to exit. From http://stackoverflow.com/a/19104345.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
        /// immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public static Task WaitForExitAsync(this Process process,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default(CancellationToken))
                cancellationToken.Register(tcs.SetCanceled);

            return tcs.Task;
        }

        #endregion
    }

    public class ServicedURL
    {
        public string ShortURL { get; set; }
        public string LongURL { get; set; }
        public ServicedURL()
        {
            this.ShortURL = string.Empty;
            this.LongURL = string.Empty;
        }
    }

}
