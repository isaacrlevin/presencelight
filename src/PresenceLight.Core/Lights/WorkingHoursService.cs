using System;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace PresenceLight.Core
{
    public interface IWorkingHoursService
    {
        public bool UseWorkingHours { get; }

        public bool OutsideWorkingHours { get; }
    }

    public class WorkingHoursService : IWorkingHoursService

    {
        private readonly ConfigWrapper _options;

        public WorkingHoursService(IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;
        }

        public WorkingHoursService(ConfigWrapper options)
        {
            _options = options;
        }

        /// <summary>
        /// Exposes a config value should you want to short circuit the working hours test.
        /// </summary>
        public bool UseWorkingHours
        {
            get
            {
                return _options.LightSettings.UseWorkingHours;
            }
        }

        /// <summary>
        /// If the current datetime is outside of the range of teh working hours then return false.
        /// </summary>
        public bool OutsideWorkingHours
        {
            get
            {
                DateTime startDate = DateTime.Parse(_options.LightSettings.WorkingHoursStartTime, CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.Parse(_options.LightSettings.WorkingHoursEndTime, CultureInfo.InvariantCulture);

                if (_options.LightSettings.WorkingDays.Contains(DateTime.Now.DayOfWeek.ToString()) &&
                    DateTime.Now >= startDate &&
                    DateTime.Now < endDate)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }
    }
}
