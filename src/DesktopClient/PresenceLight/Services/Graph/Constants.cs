
using System;
using System.Reflection;

namespace PresenceLight.Graph
{
    public static class PresenceConstants
    {
        public static string Inactive = "Not Logged In";
    }

    public class PresenceColors
    {
        public static string Available = "#009933";
        public static string AvailableIdle = "#ffff00";
        public static string Busy = "#ff3300";
        public static string BusyIdle = "#ffff00";
        public static string BeRightBack = "#ffff00";
        public static string Away = "#ffff00";
        public static string DoNotDisturb = "#B03CDE";
        public static string OutOfOffice = "#800080";
        public static string Offline = "#FFFFFF";
        public static string Inactive = "#FFFFFF";

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
