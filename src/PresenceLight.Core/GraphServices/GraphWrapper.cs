using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

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
            try
            {
                return await _retryPolicy.ExecuteAsync<Presence>(async () => await _graphServiceClient.Me.Presence.GetAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred Getting Presence from Graph Api Content");
                throw;
            }
        }

        public async Task<System.IO.Stream> GetPhoto(CancellationToken token)
        {
            return await _retryPolicy.ExecuteAsync<Stream>(async () => await _graphServiceClient.Me.Photo.Content.GetAsync());
        }

        public async Task<User> GetProfile(CancellationToken token)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync<User>(async () => await _graphServiceClient.Me.GetAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Getting Profile from Graph Api");
                throw;
            }
        }



        public async Task<(User User, Presence Presence)> GetProfileAndPresence(CancellationToken token)
        {
            return await _retryPolicy.ExecuteAsync<(User User, Presence Presence)>(async () => await GetBatchContent(token));
        }

        private async Task<(User User, Presence Presence)> GetBatchContent(CancellationToken token)
        {

            _logger.LogInformation("Getting Graph Data: Profile, Image, Presence");
            try
            {
                var userRequest = _graphServiceClient.Me.ToGetRequestInformation();
                var presenceRequest = _graphServiceClient.Me.Presence.ToGetRequestInformation();

                BatchRequestContentCollection batchRequestContent = new BatchRequestContentCollection(_graphServiceClient);

                var userRequestId = await batchRequestContent.AddBatchRequestStepAsync(userRequest);
                var presenceRequestId = await batchRequestContent.AddBatchRequestStepAsync(presenceRequest);

                var returnedResponse = await _graphServiceClient.Batch.PostAsync(batchRequestContent);

                var user = await returnedResponse.GetResponseByIdAsync<User>(userRequestId);

                var presence = await returnedResponse.GetResponseByIdAsync<Presence>(presenceRequestId);

                return (User: user, Presence: presence);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Occurred Getting Batch Content from Graph Api");
                throw;
            }
        }
    }
}
