
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using MediatR;

using Microsoft.Extensions.Logging;

using PresenceLight.Core;
using PresenceLight.Core.WizServices;
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight.ViewModels
{
    public class WizVm : BaseVm<Wiz, ColorSubscription>
    {
        private readonly DiagnosticsClient _diagClient;

        public bool IsDataVisible { get; set; } = false;
        public bool IsChecking { get; set; } = false;
        public bool UseDefaultBrightness => SettingsHandlerBase.Config.LightSettings.UseDefaultBrightness;
        public bool IsBrightnessEnabled => !UseDefaultBrightness;

        public ObservableCollection<WizLight> Lights { get; set; } = new ObservableCollection<WizLight>();

        public virtual ICommand RefreshCommand { get; }

        public WizVm(IMediator mediator, ILogger<WizVm> logger, DiagnosticsClient diagClient)
            : base(c => c.LightSettings.Wiz, mediator, logger)
        {
            _diagClient = diagClient;

            RefreshCommand = new RelayCommand(_ => CheckWiz(), _ => !IsChecking);
        }

        public override async Task Refresh()
        {
            await base.Refresh();
            RaisePropertyChanged(nameof(UseDefaultBrightness));
            RaisePropertyChanged(nameof(IsBrightnessEnabled));
            await CheckWiz();
        }

        private async Task CheckWiz()
        {
            try
            {
                IsChecking = true;
                IsDataVisible = false;
                CommandManager.InvalidateRequerySuggested();

                WizLight[] lights = (await Mediator.Send(new GetLightsCommand()).ConfigureAwait(true)).ToArray();

                SyncLights(lights);

                IsDataVisible = true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occured Checking Wiz");
                _diagClient.TrackException(e);
            }
            finally
            {
                IsChecking = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void SyncLights(WizLight[] lights)
        {
            var toRemove = Lights.Where(l1 => !lights.Any(l2 => l1.Equals(l2))).ToArray();
            foreach (WizLight item in toRemove)
            {
                Lights.Remove(item);
            }

            var toAdd = lights.Where(l1 => !Lights.Any(l2 => l1.Equals(l2)));
            foreach (WizLight light in toAdd)
            {
                Lights.Add(light);
            }
        }
    }
}
