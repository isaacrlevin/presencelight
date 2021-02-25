using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ModernWpf.Controls;
using ModernWpf.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

using PresenceLight.Core;
using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;

using Media = System.Windows.Media;
using System.Reflection;
using PresenceLight.Pages;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindowModern.xaml
    /// </summary>
    public partial class MainWindowModern : Window
    {

        public string LightMode;

        public Presence presence { get; set; }
        public DateTime settingsLastSaved = DateTime.MinValue;

        public MediatR.IMediator _mediator;
        public LIFXOAuthHelper _lIFXOAuthHelper;


        private DiagnosticsClient _diagClient;

        public readonly IGraphService _graphservice;

        public IWorkingHoursService _workingHoursService;
        public WindowState lastWindowState;

        public readonly ILogger<MainWindowModern> _logger;


        public MainWindowModern(IGraphService graphService,
                          IWorkingHoursService workingHoursService,
                          MediatR.IMediator mediator,
                          LIFXOAuthHelper lifxOAuthHelper,


          DiagnosticsClient diagClient,
        ILogger<MainWindowModern> logger)
        {
            _logger = logger;
            InitializeComponent();
            System.Windows.Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);

            _diagClient = diagClient;

            _workingHoursService = workingHoursService;
            _graphservice = graphService;

            _mediator = mediator;
            _lIFXOAuthHelper = lifxOAuthHelper;

            DataContext = SettingsHandlerBase.Config;

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
            Navigate(NavView.SelectedItem);

            Loaded += delegate
            {
                UpdateAppTitle();
            };
        }


        void UpdateAppTitle()
        {
            //ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, TitleBar.GetSystemOverlayRightInset(this), currMargin.Bottom);
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            ContentFrame.GoBack();
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                Navigate(typeof(SettingsPage));
            }
            else
            {
                Navigate(args.InvokedItemContainer);
            }
        }
        private async void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueApiKey) && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.HueIpAddress) && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId))
                {
                    if (SettingsHandlerBase.Config.LightSettings.Hue.UseRemoteApi)
                    {
                        await _mediator.Send(new Core.RemoteHueServices.SetColorCommand
                        {
                            Availability = "Off",
                            LightId = SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId,
                            BridgeId = SettingsHandlerBase.Config.LightSettings.Hue.RemoteBridgeId
                        }).ConfigureAwait(true);

                    }
                    else
                    {
                        await _mediator.Send(new Core.HueServices.SetColorCommand() { Availability = "Off", LightID = SettingsHandlerBase.Config.LightSettings.Hue.SelectedItemId, Activity = "" }).ConfigureAwait(true);

                    }
                }

                if (SettingsHandlerBase.Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.LIFX.LIFXApiKey))
                {
                    await _mediator.Send(new PresenceLight.Core.LifxServices.SetColorCommand { Activity = "", Availability = "Off", LightId = SettingsHandlerBase.Config.LightSettings.LIFX.SelectedItemId }).ConfigureAwait(true);


                }

                if (SettingsHandlerBase.Config.LightSettings.CustomApi.IsEnabled && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiOff.Method) && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.CustomApi.CustomApiOff.Uri))
                {
                    await _mediator.Send(new Core.CustomApiServices.SetColorCommand() { Activity = "Off", Availability = "Off" });

                }
                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured");
                _diagClient.TrackException(ex);
            }

            _logger.LogInformation("PresenceLight Session Ending");
        }
        private void NavView_PaneOpening(NavigationView sender, object args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void NavView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness((sender.CompactPaneLength * 2), currMargin.Top, currMargin.Right, currMargin.Bottom);

            }
            else
            {
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }

            UpdateAppTitleMargin(sender);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType() == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else
            {
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => GetPageType(x) == e.SourcePageType());
            }
        }

        private void UpdateAppTitleMargin(NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            Thickness currMargin = AppTitle.Margin;

            if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                     sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }
        private void LoadApp()
        {
            var settings = System.Windows.Application.Current.Windows.OfType<Pages.SettingsPage>().First();
            var profilePage = System.Windows.Application.Current.Windows.OfType<Pages.ProfilePage>().First();

            try
            {


                profilePage.CheckAAD();


                if (SettingsHandlerBase.Config.IconType == "Transparent")
                {
                    settings.Transparent.IsChecked = true;
                }
                else
                {
                    settings.White.IsChecked = true;
                }

                switch (SettingsHandlerBase.Config.LightSettings.HoursPassedStatus)
                {
                    case "Keep":
                        settings.HourStatusKeep.IsChecked = true;
                        break;
                    case "White":
                        settings.HourStatusWhite.IsChecked = true;
                        break;
                    case "Off":
                        settings.HourStatusOff.IsChecked = true;
                        break;
                    default:
                        settings.HourStatusKeep.IsChecked = true;
                        break;
                }

                PopulateWorkingDays();

                profilePage.notificationIcon.Text = PresenceConstants.Inactive;
                profilePage.notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));

                profilePage.turnOffButton.Visibility = Visibility.Collapsed;
                profilePage.turnOnButton.Visibility = Visibility.Collapsed;

                SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTimeAsDate = string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime) ? null : DateTime.Parse(SettingsHandlerBase.Config.LightSettings.WorkingHoursStartTime, null);
                SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTimeAsDate = string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime) ? null : DateTime.Parse(SettingsHandlerBase.Config.LightSettings.WorkingHoursEndTime, null);


                profilePage.CallGraph().ConfigureAwait(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured - {e.Message}");
                _diagClient.TrackException(e);
            }
        }
        private void PopulateWorkingDays()
        {
            var settings = System.Windows.Application.Current.Windows.OfType<Pages.SettingsPage>().First();

            if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.WorkingDays))
            {
                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Monday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Monday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Tuesday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Tuesday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Wednesday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Wednesday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Thursday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Thursday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Friday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Friday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Saturday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Saturday.IsChecked = true;
                }

                if (SettingsHandlerBase.Config.LightSettings.WorkingDays.Contains("Sunday", StringComparison.OrdinalIgnoreCase))
                {
                    settings.Sunday.IsChecked = true;
                }
            }
        }
        private void Navigate(object item)
        {
            if (item is NavigationViewItem menuItem)
            {
                var pageType = GetPageType(menuItem);
                if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
                {
                    ContentFrame.Navigate(pageType);
                }
            }
        }

        private void Navigate(Type sourcePageType)
        {
            if (ContentFrame.CurrentSourcePageType != sourcePageType)
            {
                ContentFrame.Navigate(sourcePageType);
            }
        }

        private Type? GetPageType(NavigationViewItem item)
        {
            return item.Tag as Type;
        }
    }
}
