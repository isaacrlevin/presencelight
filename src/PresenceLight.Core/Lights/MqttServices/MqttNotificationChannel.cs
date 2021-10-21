using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Publishing;

namespace PresenceLight.Core.Lights.MqttServices
{
    internal class MqttNotificationChannel
    {
        public MqttNotificationChannel(
            Func<IMqttClient> clientFactory,
            MqttSettings settings,
            ILogger<MqttNotificationChannel> logger
            )
        {
            _clientFactory = clientFactory;
            _settings = settings;
            _logger = logger;
        }

        private readonly Func<IMqttClient> _clientFactory;
        private readonly MqttSettings _settings;
        private readonly ILogger<MqttNotificationChannel> _logger;
        private PresenceMessagePayload? _lastSuccessfulPayload = null;

        public async Task NotifyAsync(string availability, string activity, string? userName)
        {
            var client = _clientFactory();

            var payload = new PresenceMessagePayload(availability, activity, userName);

            if (ShouldPublish(payload))
            {
                var msg = new MqttApplicationMessageBuilder()
               .WithTopic(_settings.BaseTopic)
               .WithPayload(payload.AsJson())
               .WithRetainFlag()
               .Build();

                var result = await client.PublishAsync(msg);

                if (result.ReasonCode == MqttClientPublishReasonCode.Success)
                    _lastSuccessfulPayload = payload;

                _logger.LogDebug("Message published to MQTT broker with result {ReasonCode}", result.ReasonCode);
            }
        }

        private bool ShouldPublish(PresenceMessagePayload payload)
        {
            return _lastSuccessfulPayload == null || !payload.Equals(_lastSuccessfulPayload);
        }
    }
}
