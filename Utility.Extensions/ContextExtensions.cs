using Utility.Interfaces.NonGeneric;

namespace Utility.Extensions
{
    public static class ContextExtensions
    {
        public static void UI(this IContext context, Action action)
        {
            if (SynchronizationContext.Current != null)
            {
                action();
            }
            else
            {
                context.UI.Post(a => action(), null);
            }
        }
    }
}