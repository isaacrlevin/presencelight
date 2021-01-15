using System;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenWiz;
namespace PresenceLight.Core
{
    public interface IWizService
    {
        Task SetColor(string availability, string lightId);
        Task<string> ConnectToLight();
        Task<IEnumerable<WizHandle>> CheckLights();
    }
    public class WizService : IWizService
    {
        private readonly BaseConfig _options;
        
        public WizService(IOptionsMonitor<BaseConfig> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public WizService(BaseConfig options)
        {
            _options = options;
        }

        public async Task SetColor(string availability, string lightId)
        {}
        
        public async Task<string> ConnectToLight()
        {
            var _wizClient = new OpenWiz.WizHandle(_options.LightSettings.Wiz.WizMacAddress, new System.Net.IPAddress(_options.LightSettings.Wiz.WizIpAddress,));

            return "";
        }

       public async Task<IEnumerable<WizHandle>> CheckLights()
       {
           return new List<WizHandle>();
       }
    }
}       
