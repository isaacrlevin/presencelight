using Microsoft.Extensions.Options;

using PresenceLight.Core.Lights;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.RemoteHueServices
{
    internal class SetColorHandler : ColorHandlerBase<Hue, ColorSubscription>
    {
        private readonly IRemoteHueService _service;

        public SetColorHandler(IRemoteHueService service, IOptionsMonitor<BaseConfig> optionsAccessor)
            : base(optionsAccessor, baseConfig => baseConfig.LightSettings.Hue, IsEnabled)
        {
            _service = service;
        }

        protected override async Task SetColor(string availability, string activity, CancellationToken cancellationToken)
        {
            await _service.SetColor(availability, Config.SelectedItemId, Config.RemoteBridgeId);
        }

        private static bool IsEnabled(Hue config) =>
            config.IsEnabled &&
            !string.IsNullOrEmpty(config.HueApiKey) &&
            !string.IsNullOrEmpty(config.HueIpAddress) &&
            !string.IsNullOrEmpty(config.SelectedItemId) &&
            config.UseRemoteApi;
    }
}
