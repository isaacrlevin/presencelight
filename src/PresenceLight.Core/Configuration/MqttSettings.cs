namespace PresenceLight.Core
{
    public class MqttSettings
    {
        public static string SectionName => "MqttSettings";

        public string BrokerUrl { get; set; } = "http://";
        public int BrokerPort { get; set; } = 1833;

        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

        public string BaseTopic { get; set; } = "presencelight";
        public string? ClientId { get; set; }
        public bool Secure { get; set; } = false;
        public int ReconnectDelaySeconds { get; set; } = 5;
        public bool IsEnabled { get; set; } = false;
    }
}
