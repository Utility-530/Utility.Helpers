using System;
using System.Threading;

namespace Utility.Helpers
{
    /// <summary>
    /// <a href="https://wiert.me/2009/09/14/netc-exceptioncatcher-and-exceptionhelper-gems/"/></a>
    /// </summary>

    public class ExceptionHelper
    {
        public static ExceptionCatcher Catch(SendOrPostCallback codeBlock)
        {
            ExceptionCatcher exceptionCatcher = new ExceptionCatcher();
            exceptionCatcher.Catch(codeBlock);
            return exceptionCatcher;
        }

        public static bool Failed(SendOrPostCallback codeBlock)
        {
            ExceptionCatcher exceptionCatcher = new ExceptionCatcher();
            bool result = exceptionCatcher.Failed(codeBlock);
            return result;
        }

        public static bool Succeeded(SendOrPostCallback codeBlock)
        {
            ExceptionCatcher exceptionCatcher = new ExceptionCatcher();
            bool result = exceptionCatcher.Succeeded(codeBlock);
            return result;
        }
    }

    public class ExceptionCatcher
    {
        public Exception? Exception { get; private set; }
        public bool Success { get; private set; }

        public object? Catch(SendOrPostCallback codeBlock)
        {
            Exception = null;
            try
            {
                // need 1 argument, because it is a SendOrPostCallback
                var output = codeBlock.DynamicInvoke(1);
                Success = true;
                return output;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Trace.WriteLine("ExceptionCatcher.Succeeded failure", ex.ToString());
#endif
                Exception = ex;
                Success = false;
            }
            return null;
        }

        public bool Failed(SendOrPostCallback codeBlock)
        {
            bool result = !Succeeded(codeBlock);
            return result;
        }

        public bool Failed(out string? exceptionString, SendOrPostCallback codeBlock)
        {
            bool result = !Succeeded(out exceptionString, codeBlock);
            return result;
        }

        public bool Succeeded(SendOrPostCallback codeBlock)
        {
            Catch(codeBlock);
            return Success;
        }

        public bool Succeeded(out string? exceptionString, SendOrPostCallback codeBlock)
        {
            bool result = Succeeded(codeBlock);
            if (result)
                exceptionString = this.Exception?.ToString();
            else
                exceptionString = string.Empty;
            return result;
        }
    }
}