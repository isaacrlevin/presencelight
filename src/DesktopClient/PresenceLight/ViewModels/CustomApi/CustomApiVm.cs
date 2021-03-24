
using MediatR;

using Microsoft.Extensions.Logging;

using PresenceLight.Core;

namespace PresenceLight.ViewModels
{
    public class CustomApiVm : BaseVm<CustomApi, CustomApiSubscription>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomApiVm> _logger;

        public string LastResponse { get; set; } = string.Empty;
        public bool IsSuccessfulResponse { get; set; } = true;

        public CustomApiVm(IMediator mediator, ILogger<CustomApiVm> logger)
            : base(c => c.LightSettings.CustomApi, mediator, logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public void UpdateLastResponse(CustomApiResponse response)
        {
            if (response != CustomApiResponse.None)
            {
                LastResponse = response?.ToString();
                IsSuccessfulResponse = response?.IsSuccessful ?? true;
            }
        }
    }
}
