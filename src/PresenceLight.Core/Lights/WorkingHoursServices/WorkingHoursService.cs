using System;
using System.Globalization;

using Microsoft.Extensions.Options;

namespace PresenceLight.Core
{
    public interface IWorkingHoursService
    {
        public bool UseWorkingHours();

        public bool IsInWorkingHours();
    }

    public class WorkingHoursService : IWorkingHoursService

    {
        private readonly BaseConfig _options;

        public WorkingHoursService(IOptionsMonitor<BaseConfig> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public WorkingHoursService(BaseConfig options)
        {
            _options = options;
        }

        /// <summary>
        /// Exposes a config value should you want to short circuit the working hours test.
        /// </summary>
        public bool UseWorkingHours()
        {

            return _options.LightSettings.UseWorkingHours;

        }
        public bool IsInWorkingHours()
        {

            bool IsWorkingHours = false;

            if (string.IsNullOrEmpty(_options.LightSettings.WorkingHoursStartTime) || string.IsNullOrEmpty(_options.LightSettings.WorkingHoursEndTime) || string.IsNullOrEmpty(_options.LightSettings.WorkingDays))
            {
                IsWorkingHours = false;
                return false;
            }

            if (!_options.LightSettings.WorkingDays.Contains(DateTime.Now.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                IsWorkingHours = false;
                return false;
            }

            // convert datetime to a TimeSpan
            bool validStart = TimeSpan.TryParse(_options.LightSettings.WorkingHoursStartTime, out TimeSpan start);
            bool validEnd = TimeSpan.TryParse(_options.LightSettings.WorkingHoursEndTime, out TimeSpan end);
            if (!validEnd || !validStart)
            {
                IsWorkingHours = false;
                return false;
            }

            TimeSpan now = DateTime.Now.TimeOfDay;
            // see if start comes before end
            if (start < end)
            {
                IsWorkingHours = start <= now && now <= end;
                return IsWorkingHours;
            }
            // start is after end, so do the inverse comparison

            IsWorkingHours = !(end < now && now < start);

            return IsWorkingHours;

        }
    }
}
