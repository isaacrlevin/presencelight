
using System;
using System.Reflection;

namespace PresenceLight.Graph
{
    public static class PresenceConstants
    {
        public const string Inactive = "Not Logged In";
    }

    public class PresenceColors
    {
        public const string Available = "#009933";
        public const string AvailableIdle = "#FFFF00";
        public const string Busy = "#FF3300";
        public const string BusyIdle = "#FFFF00";
        public const string BeRightBack = "#FFFF00";
        public const string Away = "#FFFF00";
        public const string DoNotDisturb = "#B03CDE";
        public const string OutOfOffice = "#800080";
        public const string Offline = "#FFFFFF";
        public const string Inactive = "#FFFFFF";

        public static string GetColor(string status)
        {
            var pc = new PresenceColors();
            Type type =pc.GetType();
            PropertyInfo[] props = type.GetProperties();

            foreach (var prop in props)
            {
                if (prop.Name == status)
                {
                    return prop.GetValue(pc).ToString();
                }
            }
            return PresenceColors.Inactive;
        }
    }

    public static class IconConstants
    {
        private static string Base = "pack://application:,,,/PresenceLight;component/icons/";

        public static string GetIcon(string iconType, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                status = "Inactive";
            }
            if (iconType == "Transparent")
            {
                return $"{Base}t_{status}.ico";
            }
            else
            {
                return $"{Base}{status}.ico";
            }
        }
    }
}
