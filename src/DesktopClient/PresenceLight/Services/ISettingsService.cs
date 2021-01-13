using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PresenceLight.Core;

namespace PresenceLight.Services
{
    public interface ISettingsService
    {
        public Task<BaseConfig?> LoadSettings();
        public Task<bool> SaveSettings(BaseConfig data);
        public  Task<bool> DeleteSettings();
        public Task<bool> IsFilePresent();
    }
}
