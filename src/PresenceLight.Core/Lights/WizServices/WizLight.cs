using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresenceLight.Core.WizServices
{
    public class WizLight
    {
        public string LightName { get; set; }
        public string MacAddress { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is WizLight light &&
                   LightName == light.LightName &&
                   MacAddress == light.MacAddress;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LightName, MacAddress);
        }
    }
}
