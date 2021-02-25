using MediatR;

using Microsoft.Extensions.Options;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Services
{
    internal class LoadSettingsHandler : SettingsHandlerBase, IRequestHandler<LoadSettingsCommand, BaseConfig?>
    {
        ISettingsService _service;
        public LoadSettingsHandler(ISettingsService settingsService) : base()
        {

            _service = settingsService;
        }

        public async Task<BaseConfig?> Handle(LoadSettingsCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
