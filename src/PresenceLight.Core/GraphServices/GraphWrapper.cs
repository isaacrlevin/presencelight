using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;

using Polly;
using Polly.Retry;

namespace PresenceLight.Core
{
  
    public class GraphWrapper
    {
        private readonly ILogger<GraphWrapper> _logger;
        private GraphServiceClient _graphServiceClient;
        internal AsyncRetryPolicy _retryPolicy;

        public bool IsInitialized { get; set; }

        public GraphWrapper(ILogger<GraphWrapper> logger)
        {
            _logger = logger;

            _retryPolicy = Policy
                      .Handle<Exception>()
                      .WaitAndRetryAsync(2, retryAttempt =>
                      {
                          var timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                          return timeToWait;
                      }
                      );
        }

        public void Initialize(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
            IsInitialized = true;
        }

        public async Task<Presence> GetPresence(CancellationToken token)
        {
            return await _retryPolicy.ExecuteAsync<Presence>(async () => await _graphServiceClient.Me.Presence.Request().GetAsync(token).ConfigureAwait(true));
        }

        public async Task<System.IO.Stream> GetPhoto(CancellationToken token)
        {
            return await _retryPolicy.ExecuteAsync<Stream>(async () => await _graphServiceClient.Me.Photo.Content.Request().GetAsync(token).ConfigureAwait(true));
        }

        public async Task<User> GetProfile(CancellationToken token)
        {
            return await _retryPolicy.ExecuteAsync<User>(async () => await _graphServiceClient.Me.Request().GetAsync(token).ConfigureAwait(true));
        }

        public async Task<(User User, Presence Presence)> GetProfileAndPresence(CancellationToken token)
        {
            return await _retryPolicy.ExecuteAsync<(User User, Presence Presence)>(async () => await GetBatchContent(token));
        }

        private async Task<(User User, Presence Presence)> GetBatchContent(CancellationToken token)
        {

            _logger.LogInformation("Getting Graph Data: Profle, Image, Presence");
            try
            {
                IUserRequest userRequest = _graphServiceClient.Me.Request();
                IPresenceRequest presenceRequest = _graphServiceClient.Me.Presence.Request();

                BatchRequestContent batchRequestContent = new BatchRequestContent();

                var userRequestId = batchRequestContent.AddBatchRequestStep(userRequest);
                var presenceRequestId = batchRequestContent.AddBatchRequestStep(presenceRequest);

                BatchResponseContent returnedResponse = await _graphServiceClient.Batch.Request().PostAsync(batchRequestContent,token).ConfigureAwait(true);

                User user = await returnedResponse.GetResponseByIdAsync<User>(userRequestId).ConfigureAwait(true);
                Presence presence = await returnedResponse.GetResponseByIdAsync<Presence>(presenceRequestId).ConfigureAwait(true);

                return (User: user, Presence: presence);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occured Getting Batch Content");
                throw;
            }

        }

    }
}
