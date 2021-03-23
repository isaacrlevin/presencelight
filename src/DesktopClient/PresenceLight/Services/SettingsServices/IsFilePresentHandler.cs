using MediatR;

using Microsoft.Extensions.Options;

using PresenceLight.Core;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace PresenceLight.Services
{
    internal class IsFilePresentHandler : SettingsHandlerBase, IRequestHandler<IsFilePresentCommand, bool>
    {
        ISettingsService _service;
        public IsFilePresentHandler(ISettingsService settingsService) : base()
        {
            _service = settingsService;
        }

        public async Task<bool> Handle(IsFilePresentCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
