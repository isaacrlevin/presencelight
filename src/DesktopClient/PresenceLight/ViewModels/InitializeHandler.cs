using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using PresenceLight.Core.PubSub;

namespace PresenceLight.ViewModels
{
    public class InitializeHandler : INotificationHandler<InitializeNotification>
    {
        private readonly IViewModel[] _viewModels;

        public InitializeHandler(IEnumerable<IViewModel> viewModels)
        {
            _viewModels = viewModels.ToArray();
        }

        public async Task Handle(InitializeNotification notification, CancellationToken cancellationToken)
        {
            foreach (IViewModel? vm in _viewModels)
            {
                await vm.Refresh();
            }
        }
    }
}
