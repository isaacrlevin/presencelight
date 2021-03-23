using System.ComponentModel;

namespace PresenceLight.ViewModels
{
    public static class Extensions
    {
        public static void Raise(this PropertyChangedEventHandler? handler, object sender, string propertyName)
        {
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
