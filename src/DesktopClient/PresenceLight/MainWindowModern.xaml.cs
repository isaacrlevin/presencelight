using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using ModernWpf.Controls;
using ModernWpf.Navigation;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

using PresenceLight.Graph;
using PresenceLight.Services;
using PresenceLight.Telemetry;
using PresenceLight.Pages;
using ModernWpf;
using System.IO;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using PresenceLight.Core;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PresenceLight.Core.PubSub;
using ModernWpf.Media.Animation;
using System.Collections;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindowModern.xaml
    /// </summary>
    public partial class MainWindowModern : Window
    {
        public string LightMode;

        public ObservableCollection<LogEvent> _events = new();
        private Queue<Serilog.Events.LogEvent> _logs = new(25);
        public bool loopRunning;
        static object logsLockObject = new();

        public Presence presence { get; set; }
        public DateTime settingsLastSaved = DateTime.MinValue;
        public MediatR.IMediator _mediator;
        public LIFXOAuthHelper _lIFXOAuthHelper;
        public readonly IGraphService _graphservice;
        public IWorkingHoursService _workingHoursService;
        public WindowState lastWindowState;
        private readonly ILogger<MainWindowModern> _logger;
        private DiagnosticsClient _diagClient;

        public User? profile { get; set; }

        public Stream? photo { get; set; }

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

            PresenceEventsLogSink.PresenceEventsLogHandler += Handler;

            _diagClient = diagClient;

            _workingHoursService = workingHoursService;
            _graphservice = graphService;

            _mediator = mediator;
            _lIFXOAuthHelper = lifxOAuthHelper;

            DataContext = SettingsHandlerBase.Config;

            this.Dispatcher.Invoke(() =>
            {
                DataContext = SettingsHandlerBase.Config;

                switch (SettingsHandlerBase.Config.Theme)
                {
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        break;
                    case "Use system setting":
                        ThemeManager.Current.ApplicationTheme = null;
                        break;
                    default:
                        ThemeManager.Current.ApplicationTheme = null;
                        break;
                }
            });

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
                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
                await _mediator.Publish(new SetColorNotification("Off", "Off"));
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
                NavView.SelectedItem = FindPage(e.SourcePageType());
            }

            NavView.Header = (NavView.SelectedItem as NavigationViewItem)?.Content?.ToString() ?? NavView.Header;
        }

        private NavigationViewItem? FindPage(Type pageType) => FindPage(NavView.MenuItems, pageType);

        private NavigationViewItem? FindPage(IEnumerable menuItems, Type pageType)
        {
            if (menuItems == null)
            {
                return null;
            }

            foreach (NavigationViewItem item in menuItems.OfType<NavigationViewItem>())
            {
                if (GetPageType(item) == pageType)
                {
                    return item;
                }

                NavigationViewItem? fromChild = FindPage(item.MenuItems, pageType);
                if (fromChild != null)
                {
                    return fromChild;
                }
            }

            return null;
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

        private void Navigate(object item)
        {
            if (item is NavigationViewItem menuItem)
            {
                var pageType = GetPageType(menuItem);
                if (pageType != null)
                {
                    Navigate(pageType);
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

        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await _mediator.Publish(new SetColorNotification("Off")).ConfigureAwait(true);
                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);

                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured");
                _diagClient.TrackException(ex);
            }
            _logger.LogInformation("PresenceLight Exiting");
        }

        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = lastWindowState;
        }

        private void OnNotifyIconDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Show();
                this.WindowState = this.lastWindowState;
            }
        }

        private void OnTurnOnSyncClick(object sender, RoutedEventArgs e)
        {
            LightMode = "Graph";

            turnOffButton.Visibility = Visibility.Visible;
            turnOnButton.Visibility = Visibility.Collapsed;

            WindowState = lastWindowState;
            _logger.LogInformation("Turning On PresenceLight Sync");
        }

        private async void OnTurnOffSyncClick(object sender, RoutedEventArgs e)
        {
            try
            {
                LightMode = "Custom";

                await _mediator.Publish(new SetColorNotification("Off")).ConfigureAwait(true);

                turnOffButton.Visibility = Visibility.Collapsed;
                turnOnButton.Visibility = Visibility.Visible;

                notificationIcon.Text = PresenceConstants.Inactive;
                notificationIcon.Icon = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));

                WindowState = lastWindowState;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occured");
                _diagClient.TrackException(ex);
            }
            _logger.LogInformation("Turning Off PresenceLight Sync");
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
            this.Hide();

        }

        private void Handler(object? sender, Serilog.Events.LogEvent e)
        {
            _logs.Enqueue(e);
            UpdateCollection(e);
        }



        private void UpdateCollection(LogEvent e)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                lock (logsLockObject)
                {

                    _events.Add(e);
                    //if (_events.Count > MaxRowCount)
                    //{
                    var oldEvents = _events.OrderByDescending(a => a.Timestamp).ToArray();//.Skip(MaxRowCount).ToArray();
                    oldEvents.ToList().ForEach(oe =>
                    {
                        _events.Remove(oe);
                    });
                    //}

                }
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                     new Action(() =>
                     {
                         UpdateCollection(e);
                     }));

            }
        }
    }
}
