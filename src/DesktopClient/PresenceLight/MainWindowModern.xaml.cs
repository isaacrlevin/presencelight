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

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindowModern.xaml
    /// </summary>
    public partial class MainWindowModern : Window
    {
        private readonly BaseConfig _options;
        public BaseConfig Config { get; set; }

        public string lightMode;

        public Presence presence { get; set; }
        public DateTime settingsLastSaved = DateTime.MinValue;

        public MediatR.IMediator _mediator;
        public LIFXOAuthHelper _lIFXOAuthHelper;



        public readonly IGraphService _graphservice;
        public DiagnosticsClient _diagClient;
        public ISettingsService _settingsService;
        public IWorkingHoursService _workingHoursService;
        public WindowState lastWindowState;
        public bool previousRemoteFlag;
        public readonly ILogger<MainWindowModern> _logger;

   
        public MainWindowModern(IGraphService graphService,
                          IWorkingHoursService workingHoursService,
                          MediatR.IMediator mediator,
                          IOptionsMonitor<BaseConfig> optionsAccessor,
                          LIFXOAuthHelper lifxOAuthHelper,
                          DiagnosticsClient diagClient,
                          ILogger<MainWindowModern> logger,
                          ISettingsService settingsService)
        {
            _logger = logger;
            InitializeComponent();
            //System.Windows.Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);

            _workingHoursService = workingHoursService;
            _graphservice = graphService;

            _mediator = mediator;
            _options = optionsAccessor != null ? optionsAccessor.CurrentValue : throw new NullReferenceException("Options Accessor is null");
            _lIFXOAuthHelper = lifxOAuthHelper;
            _diagClient = diagClient;
            _settingsService = settingsService;

            LoadSettings().ContinueWith(
        t =>
        {
            if (t.IsFaulted)
            { }

            this.Dispatcher.Invoke(() =>
            {
                //LoadApp();

                //var tbContext = landingPage.notificationIcon.DataContext;
                DataContext = Config;
                //landingPage.notificationIcon.DataContext = tbContext;
            });
        }, TaskScheduler.Current);


            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
            Navigate(NavView.SelectedItem);

            Loaded += delegate
            {
                UpdateAppTitle();
            };
        }



        private async Task LoadSettings()
        {
            try
            {
                if (!(await _settingsService.IsFilePresent().ConfigureAwait(true)))
                {
                    await _settingsService.SaveSettings(_options).ConfigureAwait(true);
                }

                Config = await _settingsService.LoadSettings().ConfigureAwait(true) ?? throw new NullReferenceException("Settings Load Service Returned null");            
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured Loading Settings");
                _diagClient.TrackException(e);
            }
        }


        public void SyncOptions()
        {
            PropertyInfo[] properties = typeof(BaseConfig).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object? value = property?.GetValue(Config);

                if (property?.PropertyType == typeof(string) && value != null && string.IsNullOrEmpty(value.ToString()))
                {
                    property.SetValue(_options, $"{value}".Trim());
                }
                else
                {
                    property?.SetValue(_options, value);
                }
            }
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

        private void Navigate(object item)
        {
            if (item is NavigationViewItem menuItem)
            {
               var pageType = GetPageType(menuItem);
                if (ContentFrame.CurrentSourcePageType != pageType)
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
