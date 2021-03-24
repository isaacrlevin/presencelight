using MediatR;

using Microsoft.Extensions.Options;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Services
{
    internal class LoadSettingsHandler : SettingsHandlerBase, IRequestHandler<LoadSettingsCommand, Unit>
    {
        ISettingsService _service;

        public LoadSettingsHandler(ISettingsService settingsService) : base()
        {
            _service = settingsService;
        }

        public async Task<Unit> Handle(LoadSettingsCommand command, CancellationToken cancellationToken)
        {
            var cfg = await _service.LoadSettings();

            if (cfg == null) throw new NullReferenceException("Settings Load Service Returned null");

            Config = cfg;

            return Unit.Value;
        }
    }
}
