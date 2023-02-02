using System;
using System.Collections.Generic;

namespace Utility.Helpers
{
    public static class DisposerHelper
    {
        public static IDisposable DisposeWith(this IDisposable disposable, ICollection<IDisposable> disposer)
        {
            disposer.Add(disposable);
            return disposable;
        }
    }
}