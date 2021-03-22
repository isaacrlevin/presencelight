using System.ComponentModel;

using PresenceLight.Core;

namespace PresenceLight.ViewModels
{
    public class ApiItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Availability { get; set; }
        public string? Activity { get; set; }
        public string ApiMethod { get; set; }
        public string ApiUrl { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ApiMethod) && !string.IsNullOrWhiteSpace(ApiUrl);

        public static ApiItem FromSetting(CustomApiSetting setting) =>
            new ApiItem
            {
                Availability = setting.Availability,
                Activity = setting.Activity,
                ApiMethod = setting.Method,
                ApiUrl = setting.Uri
            };

        public CustomApiSetting ToSetting() =>
            new CustomApiSetting
            {
                Availability = Availability,
                Activity = Activity,
                Method = ApiMethod,
                Uri = ApiUrl
            };
    }
}
