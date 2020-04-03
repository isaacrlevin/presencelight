using Microsoft.Graph;
using Q42.HueApi;
using Q42.HueApi.Models;
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

        public IEnumerable<Light> Lights { get; set; }

        public string LightId { get; set; }

        public string ProfileImage { get; set; }

        public Presence Presence { get; set; }

        public string AccessToken { get; set; }

        public void SetToken(string token)
        {
            AccessToken = token;
            NotifyStateChanged();
        }

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

        public void SetLights(IEnumerable<Light> lights)
        {
            Lights = lights;
            NotifyStateChanged();
        }

        public void SetLight(string lightId)
        {
            LightId = lightId;
            NotifyStateChanged();
        }
    }
}
