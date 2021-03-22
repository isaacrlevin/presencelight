using Microsoft.Extensions.Options;

using PresenceLight.Core.Lights;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.LifxServices
{
    internal class SetColorHandler : ColorHandlerBase<LIFX>
    {
        private readonly LIFXService _service;

        public SetColorHandler(LIFXService service, IOptionsMonitor<BaseConfig> optionsAccessor)
            : base(optionsAccessor, baseConfig => baseConfig.LightSettings.LIFX, IsEnabled)
        {
            _service = service;
        }

        protected override async Task SetColor(string availability, string activity, CancellationToken cancellationToken)
        {
            await _service.SetColor(availability, activity, Config.SelectedItemId, Config.LIFXApiKey);
        }

        private static bool IsEnabled(LIFX config) =>
            config.IsEnabled &&
            !string.IsNullOrEmpty(config.LIFXApiKey);
    }
}
