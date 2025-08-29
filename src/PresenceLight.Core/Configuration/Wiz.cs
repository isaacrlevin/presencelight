using System.ComponentModel.DataAnnotations;

namespace PresenceLight.Core
{
    public class Wiz : BaseLight
    {
        [Required]
        [RegularExpression(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", ErrorMessage = "Not a valid IP Address")]
        public string? IPAddress { get; set; }
    }
}
