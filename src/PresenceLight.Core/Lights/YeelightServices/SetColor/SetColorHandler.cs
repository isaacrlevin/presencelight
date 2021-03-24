using Microsoft.Extensions.Options;

using PresenceLight.Core.Lights;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.YeelightServices
{
    internal class SetColorHandler : ColorHandlerBase<Yeelight, ColorSubscription>
    {
        private readonly IYeelightService _service;

        public SetColorHandler(IYeelightService service, IOptionsMonitor<BaseConfig> optionsAccessor)
            : base(optionsAccessor, baseConfig => baseConfig.LightSettings.Yeelight, c => c.IsEnabled)
        {
            _service = service;
        }

        protected override async Task SetColor(string availability, string activity, CancellationToken cancellationToken)
        {
            await _service.SetColor(availability, activity, Config.SelectedItemId);
        }
    }
}
