namespace PresenceLight.Core
{
    public class BaseLight
    {
        public bool IsEnabled { get; set; }

        public string? SelectedItemId { get; set; }

        public int Brightness { get; set; }

        public PresenceLightStatuses Statuses { get; set; }

        public bool UseActivityStatus { get; set; }
    }
}
