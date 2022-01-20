
namespace PresenceLight.Graph
{
    public static class PresenceConstants
    {
        private static string Base = "PresenceLight Status";
        public static string Available = $"{Base} - Available";
        public static string Busy = $"{Base} - Busy";
        public static string BeRightBack = $"{Base} - Be Right Back";
        public static string Away = $"{Base} - Away";
        public static string DoNotDisturb = $"{Base} - Do Not Disturb";
        public static string OutOfOffice = $"{Base} - Out Of Office";
        public static string Inactive = $"{Base} - Not Logged In";
    }

    public static class IconConstants
    {
        private static string Base = "pack://application:,,,/PresenceLight;component/icons/";
        public static string Available = $"Available.ico";
        public static string Busy = $"Busy.ico";
        public static string BeRightBack = $"BeRightBack.ico";
        public static string Away = $"Away.ico";
        public static string OutOfOffice = $"OutOfOffice.ico";
        public static string DoNotDisturb = $"DoNotDisturb.ico";
        public static string Inactive = $"Inactive.ico";


        public static string GetIcon(string iconType, string status)
        {
            if (iconType == "Transparent")
            {
                return $"{Base}t_{status}";
            }
            else
            {
                return $"{Base}{status}";
            }
        }
    }
}
