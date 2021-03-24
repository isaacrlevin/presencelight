using MediatR;

using Microsoft.Extensions.Options;

using PresenceLight.Core.Lights;

using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Core.CustomApiServices
{
    internal class SetColorHandler : ColorHandlerBase<CustomApi, CustomApiSubscription>
    {
        readonly ICustomApiService _service;
        private readonly IMediator _mediator;

        public SetColorHandler(ICustomApiService service, IMediator mediator, IOptionsMonitor<BaseConfig> optionsAccessor)
            : base(optionsAccessor, baseConfig => baseConfig.LightSettings.CustomApi, c => c.IsEnabled)
        {
            _service = service;
            _mediator = mediator;
        }

        protected override async Task SetColor(string availability, string activity, CancellationToken cancellationToken)
        {
            CustomApiResponse response = await _service.SetColor(availability, activity, cancellationToken);
            await _mediator.Publish(new CustomApiResponseNotification(response));
        }
    }
}
