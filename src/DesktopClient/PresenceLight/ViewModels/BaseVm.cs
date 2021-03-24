using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

using MediatR;

using Microsoft.Extensions.Logging;

using PresenceLight.Core;
using PresenceLight.Services;

namespace PresenceLight.ViewModels
{
    public interface IViewModel
    {
        Task Refresh();
    }

    public abstract class BaseVm<T, TSubscription> : INotifyPropertyChanged, IViewModel
        where T: Subscriber<TSubscription>
        where TSubscription: Subscription
    {
        private readonly Func<BaseConfig, T> _configSelector;

        protected IMediator Mediator { get; }
        protected ILogger Logger { get; }

        public virtual event PropertyChangedEventHandler? PropertyChanged;

        public virtual T Config => _configSelector(SettingsHandlerBase.Config);

        public virtual List<TSubscription> SubscribedItems => Config.Subscriptions;

        public virtual bool IsEnabled
        {
            get => Config.IsEnabled;
            set => Config.IsEnabled = value;
        }

        public virtual bool IsSaving { get; set; }

        public virtual string SettingsSavedMessage { get; set; } = string.Empty;

        public virtual ICommand SaveCommand { get; }

        protected BaseVm(Func<BaseConfig, T> configSelector, IMediator mediator, ILogger logger)
        {
            _configSelector = configSelector;
            Mediator = mediator;
            Logger = logger;

            SaveCommand = new RelayCommand(SaveSettings, CanSaveSettings);
        }

        protected void RaisePropertyChanged(string property) => PropertyChanged.Raise(this, property);

        public virtual Task Refresh()
        {
            RaisePropertyChanged(nameof(IsEnabled));
            RaisePropertyChanged(nameof(SubscribedItems));

            return Task.CompletedTask;
        }

        private bool CanSaveSettings(object? _) => !IsSaving;

        private async void SaveSettings(object? _)
        {
            try
            {
                IsSaving = true;
                SettingsSavedMessage = string.Empty;

                await Mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

                SettingsSavedMessage = "Settings updated";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error when saving settings");
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
