using System;
using Microsoft.Graph;
using System.Collections.Generic;
using YeelightAPI;
using Device = YeelightAPI.Device;
using PresenceLight.Core.WizServices;

namespace PresenceLight.Worker
{
    public class AppState
    {
        private void NotifyStateChanged() => OnChange?.Invoke();

        public event Action OnChange;

        public User User { get; set; }

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

        //public bool IsUserAuthenticated { get; set; }

        //public GraphServiceClient GraphServiceClient { get; set; }

        //public void SetGraphServiceClient(GraphServiceClient graphServiceClient)
        //{
        //    GraphServiceClient = graphServiceClient;
        //    NotifyStateChanged();
        //}

        public void SetUserInfo(User user, string photo, Presence presence)
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

        //public void SetUserAuthentication(bool isAuthenticated)
        //{
        //    IsUserAuthenticated = isAuthenticated;
        //    NotifyStateChanged();
        //}

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
