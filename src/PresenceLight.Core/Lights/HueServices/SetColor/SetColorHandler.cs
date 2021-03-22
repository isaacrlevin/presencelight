using Microsoft.Extensions.Options;

using PresenceLight.Core.Lights;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.HueServices.HueService
{
    internal class SetColorHandler : ColorHandlerBase<Hue>
    {
        private readonly IHueService _service;

        public SetColorHandler(IHueService hueService, IOptionsMonitor<BaseConfig> optionsAccessor)
            : base(optionsAccessor, baseConfig => baseConfig.LightSettings.Hue, IsEnabled)
        {
            _service = hueService;
        }

        protected override async Task SetColor(string availability, string activity, CancellationToken cancellationToken)
        {
            await _service.SetColor(availability, activity, Config.SelectedItemId);
        }

        private static bool IsEnabled(Hue config) =>
            config.IsEnabled &&
            !string.IsNullOrEmpty(config.HueApiKey) &&
            !string.IsNullOrEmpty(config.HueIpAddress) &&
            !string.IsNullOrEmpty(config.SelectedItemId) &&
            !config.UseRemoteApi;
    }
}
