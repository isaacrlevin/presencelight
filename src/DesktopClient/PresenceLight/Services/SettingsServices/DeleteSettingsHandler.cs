using MediatR;

using Microsoft.Extensions.Options;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Services
{
    internal class DeleteSettingsHandler : SettingsHandlerBase, IRequestHandler<DeleteSettingsCommand, bool>
    {
        ISettingsService _service;
        public DeleteSettingsHandler(ISettingsService settingsService) : base()
        {
            _service = settingsService;
        }

        public async Task<bool> Handle(DeleteSettingsCommand command, CancellationToken cancellationToken)
        {

            throw new NotImplementedException();
            SyncOptions();
          
        }
    }
}
