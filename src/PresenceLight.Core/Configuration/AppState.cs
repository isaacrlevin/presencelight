using System;
using System.Collections.Generic;

using Microsoft.Graph.Models;

using PresenceLight.Core.WizServices;

using Device = YeelightAPI.Device;

namespace PresenceLight.Core
{
    /// <summary>
    /// Represents the application state.
    /// </summary>
    public class AppState
    {
        /// <summary>
        /// Event that is triggered when the state changes.
        /// </summary>
        public event Action OnChange;

        /// <summary>
        /// Gets or sets the user information.
        /// </summary>
        public User User { get; set; } = new User();

        /// <summary>
        /// Gets or sets a value indicating whether the user is signed in.
        /// </summary>
        public bool SignedIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a sign-in request has been made.
        /// </summary>
        public bool SignInRequested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a sign-out request has been made.
        /// </summary>
        public bool SignOutRequested { get; set; }

        /// <summary>
        /// Gets or sets the list of Hue lights.
        /// </summary>
        public IEnumerable<object> HueLights { get; set; }

        /// <summary>
        /// Gets or sets the selected Hue light ID.
        /// </summary>
        public string HueLightId { get; set; }

        /// <summary>
        /// Gets or sets the list of Yeelight lights.
        /// </summary>
        public List<Device> YeelightLights { get; set; }

        /// <summary>
        /// Gets or sets the selected Yeelight light ID.
        /// </summary>
        public string YeelightLightId { get; set; }

        /// <summary>
        /// Gets or sets the list of local serial hosts.
        /// </summary>
        public IEnumerable<string> LocalSerialHosts { get; set; }

        /// <summary>
        /// Gets or sets the selected local serial host.
        /// </summary>
        public string LocalSerialHostSelected { get; set; }

        /// <summary>
        /// Gets or sets the list of LIFX lights.
        /// </summary>
        public IEnumerable<object> LIFXLights { get; set; }

        /// <summary>
        /// Gets or sets the selected LIFX light ID.
        /// </summary>
        public string LIFXLightId { get; set; }

        /// <summary>
        /// Gets or sets the list of Wiz lights.
        /// </summary>
        public IEnumerable<WizLight> WizLights { get; set; }

        /// <summary>
        /// Gets or sets the selected Wiz light ID.
        /// </summary>
        public string WizLightId { get; set; }

        /// <summary>
        /// Gets or sets the profile image URL.
        /// </summary>
        public string ProfileImage { get; set; }

        /// <summary>
        /// Gets or sets the presence information.
        /// </summary>
        public Presence Presence { get; set; }

        /// <summary>
        /// Gets or sets the light mode.
        /// </summary>
        public string LightMode { get; set; }

        /// <summary>
        /// Gets or sets the custom color.
        /// </summary>
        public string CustomColor { get; set; }

        /// <summary>
        /// Gets or sets the base configuration.
        /// </summary>
        public BaseConfig Config { get; set; } = new BaseConfig();

        /// <summary>
        /// Sets the configuration.
        /// </summary>
        /// <param name="config">The configuration to set.</param>
        public void SetConfig(BaseConfig config)
        {
            Config = config;
        }

        /// <summary>
        /// Sets the user information.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <param name="presence">The presence information.</param>
        /// <param name="photo">The profile image URL.</param>
        public void SetUserInfo(User? user, Presence? presence, string? photo = null)
        {
            User = user;
            Presence = presence;
            ProfileImage = photo;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the presence information.
        /// </summary>
        /// <param name="presence">The presence information.</param>
        public void SetPresence(Presence presence)
        {
            Presence = presence;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the custom color.
        /// </summary>
        /// <param name="color">The custom color.</param>
        public void SetCustomColor(string color)
        {
            CustomColor = color;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the light mode.
        /// </summary>
        /// <param name="lightMode">The light mode.</param>
        public void SetLightMode(string lightMode)
        {
            LightMode = lightMode;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the list of Hue lights.
        /// </summary>
        /// <param name="lights">The list of Hue lights.</param>
        public void SetHueLights(IEnumerable<object> lights)
        {
            HueLights = lights;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the selected Hue light ID.
        /// </summary>
        /// <param name="lightId">The selected Hue light ID.</param>
        public void SetHueLight(string lightId)
        {
            HueLightId = lightId;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the list of Yeelight lights.
        /// </summary>
        /// <param name="lights">The list of Yeelight lights.</param>
        public void SetYeelightLights(List<Device> lights)
        {
            YeelightLights = lights;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the selected Yeelight light ID.
        /// </summary>
        /// <param name="lightId">The selected Yeelight light ID.</param>
        public void SetYeelightLight(string lightId)
        {
            YeelightLightId = lightId;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the list of LIFX lights.
        /// </summary>
        /// <param name="lights">The list of LIFX lights.</param>
        public void SetLIFXLights(IEnumerable<object> lights)
        {
            LIFXLights = lights;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the selected LIFX light ID.
        /// </summary>
        /// <param name="lightId">The selected LIFX light ID.</param>
        public void SetLIFXLight(string lightId)
        {
            LIFXLightId = lightId;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the list of Wiz lights.
        /// </summary>
        /// <param name="lights">The list of Wiz lights.</param>
        public void SetWizLights(IEnumerable<WizLight> lights)
        {
            WizLights = lights;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the list of local serial hosts.
        /// </summary>
        /// <param name="lights">The list of local serial hosts.</param>
        public void SetLocalSerialHosts(IEnumerable<string> lights)
        {
            LocalSerialHosts = lights;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the selected local serial host.
        /// </summary>
        /// <param name="port">The selected local serial host.</param>
        public void SetLocalSerialHost(string port)
        {
            LocalSerialHostSelected = port;
            NotifyStateChanged();
        }

        /// <summary>
        /// Sets the selected Wiz light ID.
        /// </summary>
        /// <param name="lightId">The selected Wiz light ID.</param>
        public void SetWizLight(string lightId)
        {
            WizLightId = lightId;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
