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
namespace PresenceLight.WPFApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigWrapper _options;
        private IPublicClientApplication _application;
        private AuthenticationResult authResult;
        private bool stopPolling;
        List<string> _scopes;

        public MainWindow(IGraphService graphService, IOptionsMonitor<ConfigWrapper> optionsAccessor)
        {
            _options = optionsAccessor.CurrentValue;

            _scopes = new List<string>()
            {
             GraphConstants.Scopes
            };

            _application = CreateAuthorizationProvider();
            InitializeComponent();
        }

        private IPublicClientApplication CreateAuthorizationProvider()
        {
            var clientId = _options.ApplicationId;
            var redirectUri = _options.RedirectUri;
            var authority = $"{GraphConstants.MSALLoginUrl}{_options.TenantId}";

            var pca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .WithRedirectUri(redirectUri)
                                                    .Build();

            TokenCacheHelper.EnableSerialization(pca.UserTokenCache);
            return pca;
        }

        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            authResult = null;

            var accounts = await _application.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            try
            {
                authResult = await _application.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
                .ExecuteAsync();

            }
            catch (MsalUiRequiredException)
            {
                try
                {
                    authResult = await _application.AcquireTokenInteractive(_scopes)
                       .WithParentActivityOrWindow(new WindowInteropHelper(this).Handle)
                       .ExecuteAsync();
                }
                catch (MsalException)
                {

                }
            }
            catch (Exception)
            { }


            if (authResult != null)
            {
                var response = await System.Threading.Tasks.Task.Run(() => GetBatchContent());

                Microsoft.Graph.Serializer s = new Serializer();

                var image = LoadImage(Convert.FromBase64String(response["2"]));
                var profile = s.DeserializeObject<User>(response["1"]);
                var presence = s.DeserializeObject<Presence>(response["3"]);

                MapUI(presence, profile, image);

                this.signInPanel.Visibility = Visibility.Collapsed;
                dataPanel.Visibility = Visibility.Visible;
                while (true)
                {
                    if (stopPolling)
                    {
                        stopPolling = false;
                        MyNotifyIcon.ToolTipText = PresenceConstants.Inactive;
                        MyNotifyIcon.IconSource = new BitmapImage(new Uri(IconConstants.Inactive));
                        return;
                    }
                    await Task.Delay(5000);
                    try
                    {
                        var presenceResponse = await System.Threading.Tasks.Task.Run(() => GetPresence());
                        presence = s.DeserializeObject<Presence>(presenceResponse);

                        MapUI(presence, null, null);
                    }
                    catch { }
                }
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var accounts = await _application.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await _application.RemoveAsync(accounts.FirstOrDefault());
                    this.signInPanel.Visibility = Visibility.Visible;
                    dataPanel.Visibility = Visibility.Collapsed;
                    stopPolling = true;


                    MyNotifyIcon.ToolTipText = PresenceConstants.Inactive;
                    MyNotifyIcon.IconSource = new BitmapImage(new Uri(IconConstants.Inactive));
                }
                catch (MsalException)
                {
                    // ResultText.Text = $"Error signing-out user: {ex.Message}";
                }
            }
        }

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
                    image = new BitmapImage(new Uri(IconConstants.Available));
                    color = MapColor("#009933");
                    MyNotifyIcon.ToolTipText = PresenceConstants.Available;
                    break;
                case "Busy":
                    image = new BitmapImage(new Uri(IconConstants.Busy));
                    color = MapColor("#ff3300");
                    MyNotifyIcon.ToolTipText = PresenceConstants.Busy;
                    break;
                case "BeRightBack":
                    image = new BitmapImage(new Uri(IconConstants.BeRightBack));
                    color = MapColor("#ffff00");
                    MyNotifyIcon.ToolTipText = PresenceConstants.BeRightBack;
                    break;
                case "Away":
                    image = new BitmapImage(new Uri(IconConstants.Away));
                    color = MapColor("#ffff00");
                    MyNotifyIcon.ToolTipText = PresenceConstants.Away;
                    break;
                case "DoNotDisturb":
                    image = new BitmapImage(new Uri(IconConstants.DoNotDisturb));
                    color = MapColor("#800000");
                    MyNotifyIcon.ToolTipText = PresenceConstants.DoNotDisturb;
                    break;
                default:
                    image = new BitmapImage(new Uri(IconConstants.Inactive));
                    color = MapColor("#FFFFFF");
                    MyNotifyIcon.ToolTipText = PresenceConstants.Inactive;
                    break;
            }

            if (profileImageBit != null)
            {
                profileImage.Source = profileImageBit;
            }

            MyNotifyIcon.IconSource = image;
            mySolidColorBrush.Color = color;
            status.Fill = mySolidColorBrush;
            status.StrokeThickness = 1;
            status.Stroke = System.Windows.Media.Brushes.Black;

            if (profile != null)
            {
                Name.Content = profile.DisplayName;
            }
        }

        public async Task<string> GetPresence()
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, GraphConstants.PresenceGraphEndPoint);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public async Task<Dictionary<string, string>> GetBatchContent()
        {
            var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders
      .Accept
      .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, GraphConstants.BatchGraphEndPoint);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                var content = new StringContent(
                  System.IO.File.ReadAllText("BatchRequestJson.json"), Encoding.UTF8,
                                    "application/json");
                request.Content = content;
                response = await httpClient.SendAsync(request);
                var batchResponseContent = new BatchResponseContent(response);
                var responses = await batchResponseContent.GetResponsesAsync();

                var list = new Dictionary<string, string>();

                foreach (var r in responses)
                {
                    list.Add(r.Key, await r.Value.Content.ReadAsStringAsync());
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}