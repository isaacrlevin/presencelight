
namespace PresenceLight.Core.Graph
{
    public static class GraphConstants
    {
        public static string Scopes = "https://graph.microsoft.com/.default";
        public static string BaseGraphEndPoint = "https://graph.microsoft.com/beta";
        public static string BatchGraphEndPoint = $"{BaseGraphEndPoint}/$batch";
        public static string PresenceGraphEndPoint = $"{BaseGraphEndPoint}/me/presence";
        public static string MSALLoginUrl = "https://login.microsoftonline.com/";
    }

    public static class PresenceConstants
    {
        private static string Base = "Presence Light Status";
        public static string Available = $"{Base} - Available";
        public static string Busy = $"{Base} - Busy";
        public static string BeRightBack = $"{Base} - Be Right Back";
        public static string Away = $"{Base} - Away";
        public static string DoNotDisturb = $"{Base} - Do Not Disturb";
        public static string Inactive = $"{Base} - Not Logged In";

    }

    public static class IconConstants
    {
        private static string Base = "pack://application:,,,/PresenceLight.WPFApplication;component/icons";
        public static string Available = $"{Base}/Available.ico";
        public static string Busy = $"{Base}/Busy.ico";
        public static string BeRightBack = $"{Base}/BeRightBack.ico";
        public static string Away = $"{Base}/Away.ico";
        public static string DoNotDisturb = $"{Base}/DoNotDisturb.ico";
        public static string Inactive = $"{Base}/Inactive.ico";

    }
}
