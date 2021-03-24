using MediatR;

using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.Lights
{
    internal abstract class ColorHandlerBase<T, TSubscription> : INotificationHandler<SetColorNotification>
        where T : Subscriber<TSubscription>
        where TSubscription: Subscription
    {
        private readonly IOptionsMonitor<BaseConfig> _optionsAccessor;
        private readonly Func<BaseConfig, T> _configSelector;
        private readonly Func<T, bool> _enablerFunc;

        protected T Config => _configSelector(_optionsAccessor.CurrentValue);

        public ColorHandlerBase(IOptionsMonitor<BaseConfig> optionsAccessor, Func<BaseConfig, T> configSelector, Func<T, bool> enablerFunc)
        {
            _optionsAccessor = optionsAccessor;
            _configSelector = configSelector;
            _enablerFunc = enablerFunc;
        }

        public async Task Handle(SetColorNotification notification, CancellationToken cancellationToken)
        {
            if (!_enablerFunc(Config))
                return;

            await SetColor(notification.Availability, notification.Activity, cancellationToken);
        }

        protected abstract Task SetColor(string availability, string activity, CancellationToken cancellationToken);
    }
}
