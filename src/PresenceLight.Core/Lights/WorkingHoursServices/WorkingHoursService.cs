using System;
using System.Globalization;

namespace PresenceLight.Core
{
    public interface IWorkingHoursService
    {
        public bool UseWorkingHours();

        public bool IsInWorkingHours();
    }

    public class WorkingHoursService : IWorkingHoursService

    {
        private readonly AppState _appState;

        public WorkingHoursService(AppState appState)
        {
            _appState = appState;
        }


        /// <summary>
        /// Exposes a config value should you want to short circuit the working hours test.
        /// </summary>
        public bool UseWorkingHours()
        {

            return _appState.Config.LightSettings.UseWorkingHours;

        }
        public bool IsInWorkingHours()
        {

            bool IsWorkingHours = false;

            if (!Helpers.AreStringsNotEmpty(new string[] {_appState.Config.LightSettings.WorkingHoursStartTime,
                                            _appState.Config.LightSettings.WorkingHoursEndTime,
                                            _appState.Config.LightSettings.WorkingDays}))
            {
                IsWorkingHours = false;
                return false;
            }

            if (!_appState.Config.LightSettings.WorkingDays.Contains(DateTime.Now.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                IsWorkingHours = false;
                return false;
            }

            // convert datetime to a TimeSpan
            bool validStart = TimeSpan.TryParse(_appState.Config.LightSettings.WorkingHoursStartTime, out TimeSpan start);
            bool validEnd = TimeSpan.TryParse(_appState.Config.LightSettings.WorkingHoursEndTime, out TimeSpan end);
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
