﻿using System;
using Microsoft.Extensions.Options;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PresenceLight.Core
{
    public interface IRemoteHueService
    { 
    Task SetColor(string availability, string lightId);
    Task<string> RegisterBridge();
    Task<IEnumerable<Light>> CheckLights();

}
public class RemoteHueService : IRemoteHueService
    {
        private readonly ConfigWrapper _options;
        private RemoteHueClient _client;
        private readonly IRemoteAuthenticationClient _authClient;

        public RemoteHueService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
            _authClient = new RemoteAuthenticationClient(_options.LightSettings.Hue.RemoteHueClientId, _options.LightSettings.Hue.RemoteHueClientSecret, _options.LightSettings.Hue.RemoteHueClientAppName);
        }

        public RemoteHueService(ConfigWrapper options)
        {
            _options = options;
            _authClient = new RemoteAuthenticationClient(_options.LightSettings.Hue.RemoteHueClientId, _options.LightSettings.Hue.RemoteHueClientSecret, _options.LightSettings.Hue.RemoteHueClientAppName);
        }

        public async Task<string> RegisterBridge()
        {
            try
            {
                Uri authorizeUri = _authClient.BuildAuthorizeUri(_options.LightSettings.Hue.RemoteHueClientAppName, _options.LightSettings.Hue.RemoteHueClientAppName);

                string redirectURI = "";
                var http = new System.Net.HttpListener();

                foreach (var i in new int[] { 17236 })
                {
                    try
                    {
                        redirectURI = string.Format("http://localhost:{0}/", i);
                        http.Prefixes.Add(redirectURI);
                        http.Start();

                        break;
                    }
                    catch (Exception e)
                    {
                        http = new System.Net.HttpListener();
                    }
                }

                Helpers.OpenBrowser(authorizeUri.ToString());

                var context = await http.GetContextAsync();
                var response = context.Response;

                string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://www.philips-hue.com/'></head><body>Please return to the app.</body></html>");
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
                {
                    responseOutput.Close();
                    http.Stop();
                    Debug.WriteLine("HTTP server stopped.");
                });


                // extracts the code
                var code = context.Request.QueryString.Get("code") ?? "";
                var incoming_state = context.Request.QueryString.Get("state");

                var accessToken = await _authClient.GetToken(code);

                _client = new RemoteHueClient(_authClient.GetValidToken);
                var bridges = await _client.GetBridgesAsync();

                //Register app
                return await _client.RegisterAsync(bridges.First().Id, _options.LightSettings.Hue.RemoteHueClientAppName);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task SetColor(string availability, string lightId)
        {
            _client = new RemoteHueClient(_authClient.GetValidToken);
            _client.Initialize(_options.LightSettings.Hue.HueApiKey);

            var command = new LightCommand();

            switch (availability)
            {
                case "Available":
                    command.SetColor(new RGBColor("#009933"));
                    break;
                case "Busy":
                    command.SetColor(new RGBColor("#ff3300"));
                    break;
                case "BeRightBack":
                    command.SetColor(new RGBColor("#ffff00"));
                    break;
                case "Away":
                    command.SetColor(new RGBColor("#ffff00"));
                    break;
                case "DoNotDisturb":
                    command.SetColor(new RGBColor("#B03CDE"));
                    break;
                case "Offline":
                    command.SetColor(new RGBColor("#FFFFFF"));
                    break;
                case "Off":
                    command.SetColor(new RGBColor("#FFFFFF"));
                    break;
                default:
                    command.SetColor(new RGBColor(availability));
                    break;
            }

            if (availability == "Off")
            {
                command.On = false;
                await _client.SendCommandAsync(command, new List<string> { lightId });
                return;
            }

            if (_options.LightSettings.UseDefaultBrightness)
            {
                if (_options.LightSettings.DefaultBrightness == 0)
                {
                    command.On = false;
                }
                else
                {
                    command.On = true;
                    command.Brightness = Convert.ToByte(((Convert.ToDouble(_options.LightSettings.DefaultBrightness) / 100) * 254));
                    command.TransitionTime = new TimeSpan(0);
                }
            }
            else
            {
                if (_options.LightSettings.Hue.HueBrightness == 0)
                {
                    command.On = false;
                }
                else
                {
                    command.On = true;
                    command.Brightness = Convert.ToByte(((Convert.ToDouble(_options.LightSettings.Hue.HueBrightness) / 100) * 254));
                    command.TransitionTime = new TimeSpan(0);
                }
            }

            await _client.SendCommandAsync(command, new List<string> { lightId });
        }

        public async Task<IEnumerable<Light>> CheckLights()
        {
            if (_client == null)
            {
                _client = new RemoteHueClient(_authClient.GetValidToken);
                _client.Initialize(_options.LightSettings.Hue.HueApiKey);
            }
            var lights = await _client.GetLightsAsync();
            // if there are no lights, get some
            if (lights.Count() == 0)
            {
                await _client.SearchNewLightsAsync();
                Thread.Sleep(40000);
                lights = await _client.GetNewLightsAsync();
            }
            return lights;
        }

        public Task<string> FindBridge()
        {
            throw new NotImplementedException();
        }
    }
}