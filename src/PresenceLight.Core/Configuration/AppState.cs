using System;
using System.Collections.Generic;

using Microsoft.Graph;

using PresenceLight.Core.WizServices;

using Device = YeelightAPI.Device;

namespace PresenceLight.Core
{
    public class AppState
    {
        private void NotifyStateChanged() => OnChange?.Invoke();

        public event Action OnChange;

        public User User { get; set; }

        public bool SignedIn { get; set; }

        public bool SignInRequested { get; set; }

        public bool SignOutRequested { get; set; }

        public IEnumerable<object> HueLights { get; set; }

        public string HueLightId { get; set; }

        public List<Device> YeelightLights { get; set; }

        public string YeelightLightId { get; set; }

        public IEnumerable<object> LIFXLights { get; set; }

        public string LIFXLightId { get; set; }

        public IEnumerable<WizLight> WizLights { get; set; }

        public string WizLightId { get; set; }

        public string ProfileImage { get; set; }

        public Presence Presence { get; set; }

        public string LightMode { get; set; }

        public string CustomColor { get; set; }

        public BaseConfig Config { get; set; }

        public void SetConfig(BaseConfig config)
        {
            Config = config;
        }

        public void SetUserInfo(User user, Presence presence, string photo = null)
        {
            User = user;
            Presence = presence;
            ProfileImage = photo;
            NotifyStateChanged();
        }

        public void SetPresence(Presence presence)
        {
            Presence = presence;
            NotifyStateChanged();
        }



        public void SetCustomColor(string color)
        {
            CustomColor = color;
            NotifyStateChanged();
        }

        public void SetLightMode(string lightMode)
        {
            LightMode = lightMode;
            NotifyStateChanged();
        }

        public void SetHueLights(IEnumerable<object> lights)
        {
            HueLights = lights;
            NotifyStateChanged();
        }

        public void SetHueLight(string lightId)
        {
            HueLightId = lightId;
            NotifyStateChanged();
        }

        public void SetYeelightLights(List<Device> lights)
        {
            YeelightLights = lights;
            NotifyStateChanged();
        }

        public void SetYeelightLight(string lightId)
        {
            YeelightLightId = lightId;
            NotifyStateChanged();
        }

        public void SetLIFXLights(IEnumerable<object> lights)
        {
            LIFXLights = lights;
            NotifyStateChanged();
        }

        public void SetLIFXLight(string lightId)
        {
            LIFXLightId = lightId;
            NotifyStateChanged();
        }

        public void SetWizLights(IEnumerable<WizLight> lights)
        {
            WizLights = lights;
            NotifyStateChanged();
        }

        public void SetWizLight(string lightId)
        {
            WizLightId = lightId;
            NotifyStateChanged();
        }
    }
}
