namespace PresenceLight.Core
{
    public class BaseLight : Subscriber<ColorSubscription>
    {
        public string? SelectedItemId { get; set; }

        public int Brightness { get; set; }

        public Statuses Statuses { get; set; }

        public bool UseActivityStatus { get; set; }
    }
}
