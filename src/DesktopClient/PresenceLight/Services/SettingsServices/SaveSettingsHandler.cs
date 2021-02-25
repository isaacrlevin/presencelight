using MediatR;

using Microsoft.Extensions.Options;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Services
{
    internal class SaveSettingsHandler : SettingsHandlerBase, IRequestHandler<SaveSettingsCommand, bool>
    {
        ISettingsService _service;

        public SaveSettingsHandler(ISettingsService settingsService) : base()
        {
            _service = settingsService;
        }

        public async Task<bool> Handle(SaveSettingsCommand command, CancellationToken cancellationToken)
        {
            Config = Helpers.CleanColors(Config);
            var result =  await _service.SaveSettings(Config);
            SyncOptions();
            return result;
        }
    }
}
