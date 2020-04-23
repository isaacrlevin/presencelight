using LifxCloud.NET.Models;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PresenceLight.Worker
{
    public class AppState
    {
        private void NotifyStateChanged() => OnChange?.Invoke();

        public event Action OnChange;

        public User User { get; set; }

        public IEnumerable<Q42.HueApi.Light> HueLights { get; set; }

        public string HueLightId { get; set; }

        public IEnumerable<LifxCloud.NET.Models.Light> LifxLights { get; set; }

        public string LifxLightId { get; set; }

        public string ProfileImage { get; set; }

        public Presence Presence { get; set; }

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

        public void SetHueLights(IEnumerable<Q42.HueApi.Light> lights)
        {
            HueLights = lights;
            NotifyStateChanged();
        }

        public void SetHueLight(string lightId)
        {
            HueLightId = lightId;
            NotifyStateChanged();
        }

        public void SetLifxLights(IEnumerable<LifxCloud.NET.Models.Light> lights)
        {
            LifxLights = lights;
            NotifyStateChanged();
        }

        public void SetLifxLight(string lightId)
        {
            LifxLightId = lightId;
            NotifyStateChanged();
        }
    }
}
