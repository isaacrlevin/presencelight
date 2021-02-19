using System.ComponentModel.DataAnnotations;

namespace PresenceLight.Core
{
    public class Wiz : BaseLight
    {
        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
        ErrorMessage = "Not a valid IP Address")]
        public string WizIpAddress { get; set; }

        [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
        ErrorMessage = "Not a valid MAC Address")]
        public string WizMacAddress { get; set; }
    }
}
