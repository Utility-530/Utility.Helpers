using MintPlayer.ObservableCollection;
using Utility.Changes;

namespace Utility.Extensions
{
    public static class CollectionExtensions
    {

        public static ObservableCollection<T> ToCollection<T>(this IObservable<Change<T>> observable, out IDisposable disposable)
        {
            var obs = new ObservableCollection<T>();

            disposable = observable
                .Subscribe(a =>
                {
                    switch (a.Type)
                    {
                        case Utility.Changes.Type.Remove:
                            obs.Remove(a.Value);
                            break;
                        case Utility.Changes.Type.Add:
                            obs.Add(a.Value);
                            break;
                        case Utility.Changes.Type.Reset:
                            obs.Clear();
                            break;
                    }
                });
            return obs;
        }
    }
}
