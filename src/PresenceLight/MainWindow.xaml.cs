using Microsoft.Graph;
using Microsoft.Identity.Client;
using PresenceLight.Core;
using PresenceLight.Core.Graph;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using Media = System.Windows.Media;
using System.Diagnostics;
using System.Windows.Navigation;
using PresenceLight.Core.Helpers;
using Newtonsoft.Json;
using System.Web.UI.WebControls;

namespace PresenceLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigWrapper _options;
        private bool stopPolling;
        private readonly IHueService _hueService;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IGraphService _graphservice;

        public MainWindow(IGraphService graphService, IHueService hueService, IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;


            InitializeComponent();

            if (string.IsNullOrEmpty(_options.ApplicationId) || string.IsNullOrEmpty(_options.TenantId) || string.IsNullOrEmpty(_options.RedirectUri))
            {
                configErrorPanel.Visibility = Visibility.Visible;
                dataPanel.Visibility = Visibility.Hidden;
                signInPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                _graphservice = graphService;
                _graphServiceClient = _graphservice.GetAuthenticatedGraphClient(typeof(WPFAuthorizationProvider));
            }

            if (string.IsNullOrEmpty(_options.HueApiKey))
            {
              lblMessage.Text = "Missing App Registration, please button on bridge than click 'Register Bridge'";
            }
            else
            {
                lblMessage.Text = "App Registered with Bridge";
            }

            hueIpAddress.Text = _options.HueIpAddress;

            if (_options.IconType == "Transparent")
            {
                Transparent.IsChecked = true;
            }
            else
            {
                White.IsChecked = true;
            }


            _hueService = hueService;

            notificationIcon.ToolTipText = PresenceConstants.Inactive;
            notificationIcon.IconSource = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            var (profile, presence) = await System.Threading.Tasks.Task.Run(() => GetBatchContent());
            var photo = await System.Threading.Tasks.Task.Run(() => GetPhoto());

            MapUI(presence, profile, LoadImage(photo));
            
            if (!string.IsNullOrEmpty(_options.HueApiKey) && string.IsNullOrEmpty(_options.HueIpAddress))
            {
                await _hueService.SetColor(presence.Availability);
            }

            this.signInPanel.Visibility = Visibility.Collapsed;
            dataPanel.Visibility = Visibility.Visible;
            while (true)
            {
                if (stopPolling)
                {
                    stopPolling = false;
                    notificationIcon.ToolTipText = PresenceConstants.Inactive;
                    notificationIcon.IconSource = new BitmapImage(new Uri(IconConstants.GetIcon(String.Empty, IconConstants.Inactive)));
                    return;
                }
                await Task.Delay(5000);
                try
                {
                    presence = await System.Threading.Tasks.Task.Run(() => GetPresence());
                    if (!string.IsNullOrEmpty(_options.HueApiKey) && string.IsNullOrEmpty(_options.HueIpAddress))
                    {
                        await _hueService.SetColor(presence.Availability);
                    }
                    MapUI(presence, null, null);
                }
                catch { }
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var accounts = await WPFAuthorizationProvider._application.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await WPFAuthorizationProvider._application.RemoveAsync(accounts.FirstOrDefault());
                    this.signInPanel.Visibility = Visibility.Visible;
                    dataPanel.Visibility = Visibility.Collapsed;
                    stopPolling = true;

                    notificationIcon.ToolTipText = PresenceConstants.Inactive;
                    notificationIcon.IconSource = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                }
                catch (MsalException)
                {
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        #region UI Helpers
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public Media.Color MapColor(string hexColor)
        {
            return (Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hexColor);
        }

        public void MapUI(Presence presence, User profile, BitmapImage profileImageBit)
        {
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            System.Windows.Media.Color color;
            BitmapImage image;
            switch (presence.Availability)
            {
                case "Available":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(_options.IconType, IconConstants.Available)));
                    color = MapColor("#009933");
                    notificationIcon.ToolTipText = PresenceConstants.Available;
                    break;
                case "Busy":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(_options.IconType, IconConstants.Busy)));
                    color = MapColor("#ff3300");
                    notificationIcon.ToolTipText = PresenceConstants.Busy;
                    break;
                case "BeRightBack":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(_options.IconType, IconConstants.BeRightBack)));
                    color = MapColor("#ffff00");
                    notificationIcon.ToolTipText = PresenceConstants.BeRightBack;
                    break;
                case "Away":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(_options.IconType, IconConstants.Away)));
                    color = MapColor("#ffff00");
                    notificationIcon.ToolTipText = PresenceConstants.Away;
                    break;
                case "DoNotDisturb":
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(_options.IconType, IconConstants.DoNotDisturb)));
                    color = MapColor("#800000");
                    notificationIcon.ToolTipText = PresenceConstants.DoNotDisturb;
                    break;
                default:
                    image = new BitmapImage(new Uri(IconConstants.GetIcon(string.Empty, IconConstants.Inactive)));
                    color = MapColor("#FFFFFF");
                    notificationIcon.ToolTipText = PresenceConstants.Inactive;
                    break;
            }

            if (profileImageBit != null)
            {
                profileImage.Source = profileImageBit;
            }

            notificationIcon.IconSource = image;
            mySolidColorBrush.Color = color;
            status.Fill = mySolidColorBrush;
            status.StrokeThickness = 1;
            status.Stroke = System.Windows.Media.Brushes.Black;

            if (profile != null)
            {
                Name.Content = profile.DisplayName;
            }
        }
        #endregion

        #region Graph Calls
        public async Task<Presence> GetPresence()
        {
            if (!stopPolling)
            {
                return await _graphServiceClient.Me.Presence.Request().GetAsync();
            }
            else
            {
                throw new Exception();
            }
        }

        public async Task<byte[]> GetPhoto()
        {
            return ReadFully(await _graphServiceClient.Me.Photo.Content.Request().GetAsync());
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async Task<(User User, Presence Presence)> GetBatchContent()
        {
            IUserRequest userRequest = _graphServiceClient.Me.Request();
            IPresenceRequest presenceRequest = _graphServiceClient.Me.Presence.Request();

            BatchRequestContent batchRequestContent = new BatchRequestContent();

            var userRequestId = batchRequestContent.AddBatchRequestStep(userRequest);
            var presenceRequestId = batchRequestContent.AddBatchRequestStep(presenceRequest);

            BatchResponseContent returnedResponse = await _graphServiceClient.Batch.Request().PostAsync(batchRequestContent);

            User user = await returnedResponse.GetResponseByIdAsync<User>(userRequestId);
            Presence presence = await returnedResponse.GetResponseByIdAsync<Presence>(presenceRequestId);

            return (User: user, Presence: presence);
        }
        #endregion

        private void SaveHueSettings_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}/appsettings.json", JsonConvert.SerializeObject(_options));

            var _hueService = new HueService(_options);
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Transparent.IsChecked == true)
            {
                _options.IconType = "Transparent";
            }
            else
            {
                _options.IconType = "White";
            }

            System.IO.File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}/appsettings.json", JsonConvert.SerializeObject(_options));
        }

        private async void registerBridge_Click(object sender, RoutedEventArgs e)
        {
            await _hueService.RegisterBridge();

            if (!string.IsNullOrEmpty(_options.HueApiKey))
            {
                lblMessage.Text = "App Registered with Bridge";
                System.IO.File.WriteAllText($"{System.IO.Directory.GetCurrentDirectory()}/appsettings.json", JsonConvert.SerializeObject(_options));
            }
        }
    }
}