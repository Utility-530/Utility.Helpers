using System;
using System.Collections.Generic;
using System.Text;
using Splat;

namespace Utility
{
    public static class ErrorHelper
    {
        public static T? TryCatch<T>(Func<T> theFunction, ILogger logger)
        {
            try
            {
                return theFunction();
            }
            catch (Exception ex)
            {
                logger.Write(ex, "Exception caught" , LogLevel.Error);
                return default;
                // You'll need to either rethrow here, or return default(T) etc
            }
        }
    }
}
