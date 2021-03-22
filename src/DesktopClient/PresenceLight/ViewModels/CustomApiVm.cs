using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using MediatR;

using Microsoft.Extensions.Logging;

using PresenceLight.Core;
using PresenceLight.Services;

namespace PresenceLight.ViewModels
{
    public class CustomApiVm : INotifyPropertyChanged
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomApiVm> _logger;

        public event PropertyChangedEventHandler? PropertyChanged;

        public static List<string> ApiMethods { get; } = new List<string>
        {
            "",
            "GET",
            "POST",
            "DELETE"
        };

        public static List<string> AvailabilityStates { get; } = new List<string>
        {
            "",
            "Available",
            "Busy",
            "BeRightBack",
            "Away",
            "DoNotDisturb",
            "AvailableIdle",
            "Offline",
            "Off",
        };

        public static List<string> ActivityStates { get; } = new List<string>
        {
            "",
            "Available",
            "Presenting",
            "InACall",
            "InAMeeting",
            "Busy",
            "Away",
            "BeRightBack",
            "DoNotDisturb",
            "Idle",
            "Offline",
            "Off",
        };

        public ICommand SaveCommand { get; }
        public ObservableCollection<ApiItem> SubscribedItems { get; } = new ObservableCollection<ApiItem>();
        public bool IsEnabled { get; set; }        
        public string LastResponse { get; set; } = string.Empty;
        public bool IsSuccessfulResponse { get; set; } = true;
        public bool IsSaving { get; set; }
        public string SettingsSavedMessage { get; set; } = string.Empty;
        public CustomApi Config => SettingsHandlerBase.Config.LightSettings.CustomApi;

        public CustomApiVm(IMediator mediator, ILogger<CustomApiVm> logger)
        {
            _mediator = mediator;
            _logger = logger;

            SaveCommand = new RelayCommand(SaveSettings, CanSaveSettings);

            LoadSettings(SettingsHandlerBase.Config.LightSettings.CustomApi);
        }

        public void UpdateLastResponse(CustomApiResponse response)
        {
            if (response != CustomApiResponse.None)
            {
                LastResponse = response?.ToString();
                IsSuccessfulResponse = response?.IsSuccessful ?? true;
            }
        }

        private void LoadSettings(CustomApi config)
        {
            IsEnabled = config.IsEnabled;
            foreach (CustomApiSetting setting in config.Subscriptions)
            {
                SubscribedItems.Add(ApiItem.FromSetting(setting));
            }
        }

        private bool CanSaveSettings(object? _) => !IsSaving;

        private async void SaveSettings(object? _)
        {
            try
            {
                IsSaving = true;
                SettingsSavedMessage = string.Empty;

                SettingsHandlerBase.Config.LightSettings.CustomApi = SerializeConfig();

                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

                SettingsSavedMessage = "Settings updated";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured Saving Custom Api Settings");
                //TODO: Revisit if Telemtry isnt working through serilog
                //_diagClient.TrackException(ex);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private CustomApi SerializeConfig() => new CustomApi
        {
            IsEnabled = IsEnabled,
            Subscriptions = SubscribedItems.Where(x => x.IsValid).Select(x => x.ToSetting()).ToList()
        };
    }
}
