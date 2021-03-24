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
        private readonly IRefreshable[] _refreshables;

        public InitializeHandler(IEnumerable<IRefreshable> refreshables)
        {
            _refreshables = refreshables.ToArray();
        }

        public async Task Handle(InitializeNotification notification, CancellationToken cancellationToken)
        {
            foreach (var refreshable in _refreshables)
            {
                await refreshable.Refresh();
            }
        }
    }
}
