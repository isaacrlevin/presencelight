using MediatR;

using Microsoft.Extensions.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.Lights
{
    internal abstract class ColorHandlerBase<TConfig> : INotificationHandler<SetColorNotification>
        where TConfig : BaseLight
    {
        private readonly IOptionsMonitor<BaseConfig> _optionsAccessor;
        private readonly Func<BaseConfig, TConfig> _configSelector;
        private readonly Func<TConfig, bool> _enablerFunc;

        protected TConfig Config => _configSelector(_optionsAccessor.CurrentValue);

        public ColorHandlerBase(IOptionsMonitor<BaseConfig> optionsAccessor, Func<BaseConfig, TConfig> configSelector, Func<TConfig, bool> enablerFunc)
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
