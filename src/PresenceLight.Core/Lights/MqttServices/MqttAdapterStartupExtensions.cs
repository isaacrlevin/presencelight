using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions;
using MQTTnet.Extensions.ManagedClient;

namespace PresenceLight.Core.MqttServices;

public static class MqttAdapterStartupExtensions
{
    private static Lazy<IMqttClient> _lazyClient;
    public static IServiceCollection AddMqttNotificationChannel(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetSection(MqttSettings.SectionName).Get<MqttSettings>() ?? new MqttSettings();

        _lazyClient = new Lazy<IMqttClient>(() => CreateClient(settings));
        services.AddSingleton<Func<IMqttClient>>(() => _lazyClient.Value);
        services.AddSingleton(settings);
        services.AddSingleton<MqttNotificationChannel>();

        return services;
    }

    internal static IMqttClient CreateClient(MqttSettings settings)
    {
        var messageBuilder = new MqttClientOptionsBuilder()
            .WithClientId(settings.ClientId ?? Guid.NewGuid().ToString())
            .WithCredentials(settings.UserName, settings.Password)
            .WithConnectionUri(settings.BrokerUrl)
            .WithCleanSession();

        var options = settings.Secure
            ? messageBuilder
                .WithTls()
                .Build()
            : messageBuilder
                .Build();

        var managedOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(settings.ReconnectDelaySeconds))
            .WithClientOptions(options)
            .Build();

        var client = new MqttFactory().CreateMqttClient();
        client.ConnectAsync(messageBuilder.Build()).Wait();

        return client;
    }
}
