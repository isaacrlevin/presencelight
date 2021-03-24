using Microsoft.Extensions.Options;

using PresenceLight.Core.Lights;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.WizServices.WizService
{
    internal class SetColorHandler : ColorHandlerBase<Wiz, ColorSubscription>
    {
        private readonly IWizService _service;

        public SetColorHandler(IWizService service, IOptionsMonitor<BaseConfig> optionsAccessor)
            : base(optionsAccessor, baseConfig => baseConfig.LightSettings.Wiz, c => c.IsEnabled)
        {
            _service = service;
        }

        protected override async Task SetColor(string availability, string activity, CancellationToken cancellationToken)
        {
            await _service.SetColor(availability, activity, Config.SelectedItemId);
        }
    }
}
