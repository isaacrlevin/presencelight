using MediatR;

namespace PresenceLight.Core.PubSub
{
    public class InitializeNotification : INotification
    {
        public BaseConfig Config { get; }

        public InitializeNotification(BaseConfig config)
        {
            Config = config;
        }
    }
}
