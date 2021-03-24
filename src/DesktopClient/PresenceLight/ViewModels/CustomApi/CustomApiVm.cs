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

        public ICommand SaveCommand { get; }
        public List<CustomApiSetting> SubscribedItems => Config.Subscriptions;
        public bool IsEnabled
        {
            get => Config.IsEnabled;
            set => Config.IsEnabled = value;
        }
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
        }

        public void UpdateLastResponse(CustomApiResponse response)
        {
            if (response != CustomApiResponse.None)
            {
                LastResponse = response?.ToString();
                IsSuccessfulResponse = response?.IsSuccessful ?? true;
            }
        }

        public void Refresh()
        {
            PropertyChanged.Raise(this, nameof(IsEnabled));
            PropertyChanged.Raise(this, nameof(SubscribedItems));
        }

        private bool CanSaveSettings(object? _) => !IsSaving;

        private async void SaveSettings(object? _)
        {
            try
            {
                IsSaving = true;
                SettingsSavedMessage = string.Empty;

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
    }
}
