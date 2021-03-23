using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PresenceLight.Services
{
    public class ColorService
    {
        readonly MediatR.IMediator _mediator;
        readonly ILogger _logger;
        public ColorService(ILogger<ColorService> logger)
        {
            _logger = logger;
            _mediator = App.ServiceProvider.GetRequiredService<MediatR.IMediator>();
        }

        public async Task SetColor(string color, string activity)
        {
            try
            {
                if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId))
                {
                    if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
                    {
                        if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.RemoteBridgeId))
                        {
                            await _mediator.Send(new Core.RemoteHueServices.SetColorCommand
                            {
                                Availability = color,
                                LightId = SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId,
                                BridgeId = SettingsHandlerBase.Config.LightSettings.Hue.RemoteBridgeId
                            }).ConfigureAwait(true);
                        }
                    }
                    else
                    {
                        await _mediator.Send(new Core.HueServices.SetColorCommand()
                        {
                            Activity = activity,
                            Availability = color,
                            LightID = SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId
                        }).ConfigureAwait(false);

                    }
                }

                if (SettingsHandlerBase.Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _mediator.Send(new PresenceLight.Core.LifxServices.SetColorCommand { Activity = activity, Availability = color, LightId = SettingsHandlerBase.Config.LightSettings.LIFX.SelectedItemId }).ConfigureAwait(true);

                }

                if (SettingsHandlerBase.Config.LightSettings.Yeelight.IsEnabled && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Yeelight.SelectedItemId))
                {
                    await _mediator.Send(new PresenceLight.Core.YeelightServices.SetColorCommand { Activity = activity, Availability = color, LightId = SettingsHandlerBase.Config.LightSettings.Yeelight.SelectedItemId }).ConfigureAwait(true);

                }

                if (SettingsHandlerBase.Config.LightSettings.CustomApi.IsEnabled)
                {
                    string response = await _mediator.Send(new Core.CustomApiServices.SetColorCommand() { Activity = activity, Availability = color });
                }
            }
            catch (Exception e)
            {

                _logger.LogError(e, "Error Occurred");
                //TODO: Come back here if Serilog is not working with APpInsights
                //_diagClient.TrackException(e);
                //throw;
            }
        }
    }
}
