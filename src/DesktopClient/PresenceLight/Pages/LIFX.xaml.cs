using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

using LifxCloud.NET.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PresenceLight.Core;
using PresenceLight.Core.LifxServices;
using PresenceLight.Services;
using PresenceLight.Telemetry;

namespace PresenceLight.Pages
{
    /// <summary>
    /// Interaction logic for LIFX.xaml
    /// </summary>
    public partial class LIFX
    {
        LIFXOAuthHelper _lifxOAuthHelper;
        private MediatR.IMediator _mediator;
        public DiagnosticsClient _diagClient;
        ILogger _logger;
        public LIFX()
        {
            _mediator = App.ServiceProvider.GetRequiredService<MediatR.IMediator>();
            _logger = App.ServiceProvider.GetRequiredService<ILogger<LIFX>>();
            _diagClient = App.ServiceProvider.GetRequiredService<DiagnosticsClient>();

            _lifxOAuthHelper = App.ServiceProvider.GetRequiredService<LIFXOAuthHelper>();
            InitializeComponent();

            if (SettingsHandlerBase.Config.LightSettings.LIFX.IsEnabled)
            {
                if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.LIFX.LIFXClientId) && !(string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.LIFX.LIFXClientSecret)))
                {
                    getTokenLink.Visibility = Visibility.Visible;
                    pnlLIFX.Visibility = Visibility.Visible;
                    cbIsLIFXEnabled.IsChecked = true;
                }
                SettingsHandlerBase.SyncOptions();
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
                pnlLIFX.Visibility = Visibility.Collapsed;
            }

            if (SettingsHandlerBase.Config.LightSettings.UseDefaultBrightness)
            {
                lifxBrightness.IsEnabled = false;
                lifxBrightnessNum.IsEnabled = false;
                lifxBrightnessText.Visibility = Visibility.Visible;
            }
            else
            {
                lifxBrightness.IsEnabled = true;
                lifxBrightnessNum.IsEnabled = true;
                lifxBrightnessText.Visibility = Visibility.Collapsed;
            }
            

            CheckLIFX();
        }

        #region LIFX Panel

        private async void LIFXToken_Get(object sender, RoutedEventArgs e)
        {
            try
            {
                string accessToken = await _lifxOAuthHelper.InitiateTokenRetrieval().ConfigureAwait(true);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    lifxApiKey.Text = accessToken;
                    SettingsHandlerBase.Config.LightSettings.LIFX.LIFXApiKey = accessToken;
                    btnGetLIFXLights.IsEnabled = true;
                    btnGetLIFXGroups.IsEnabled = true;

                    CheckLIFX();

                    SettingsHandlerBase.SyncOptions();
                }
                else
                {
                    btnGetLIFXLights.IsEnabled = false;
                    btnGetLIFXGroups.IsEnabled = false;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Getting LIFX Token");
                _diagClient.TrackException(ex);
            }
        }

        private async void SaveLIFX_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnLIFX.IsEnabled = false;
                await _mediator.Send(new SaveSettingsCommand()).ConfigureAwait(true);
                lblLIFXSaved.Visibility = Visibility.Visible;
                btnLIFX.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured Saving LIFX Settings");
                _diagClient.TrackException(ex);
            }
        }

        private async void CheckLIFX()
        {
            imgLIFXLoading.Visibility = Visibility.Visible;
            pnlLIFXData.Visibility = Visibility.Collapsed;
            lblLIFXMessage.Visibility = Visibility.Collapsed;

            SolidColorBrush fontBrush = new SolidColorBrush();
            try
            {
                if (SettingsHandlerBase.Config.LightSettings.LIFX.IsEnabled && !string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.LIFX.LIFXApiKey))
                {
                    ddlLIFXLights.ItemsSource = await _mediator.Send(new Core.LifxServices.GetAllLightsCommand()).ConfigureAwait(true);

                    if (!string.IsNullOrEmpty(SettingsHandlerBase.Config.LightSettings.LIFX.SelectedItemId))
                    {
                        foreach (var item in ddlLIFXLights.Items)
                        {
                            if (item != null)
                            {
                                var light = (Light)item;
                                if ($"id:{light?.Id}" == SettingsHandlerBase.Config.LightSettings.LIFX.SelectedItemId)
                                {
                                    ddlLIFXLights.SelectedItem = item;
                                    lifxItemType.Content = "Lights";
                                }
                            }
                        }

                        if (ddlLIFXLights.SelectedItem == null)
                        {
                            ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllGroupsCommand()).ConfigureAwait(true);

                            foreach (var item in ddlLIFXLights.Items)
                            {
                                if (item != null)
                                {
                                    var group = (LifxCloud.NET.Models.Group)item;
                                    if ($"group_id:{group?.Id}" == SettingsHandlerBase.Config.LightSettings.LIFX.SelectedItemId)
                                    {
                                        ddlLIFXLights.SelectedItem = item;
                                        lifxItemType.Content = "Groups";
                                    }
                                }
                            }
                        }

                        if (ddlLIFXLights.SelectedItem != null)
                        {
                            btnGetLIFXLights.IsEnabled = true;
                            btnGetLIFXGroups.IsEnabled = true;

                            pnlLIFXData.Visibility = Visibility.Visible;
                            lblLIFXMessage.Text = "Connected to LIFX Cloud";
                            fontBrush.Color = "#009933".MapColor();
                            lblLIFXMessage.Foreground = fontBrush;
                        }

                        if (SettingsHandlerBase.Config.LightSettings.LIFX.UseActivityStatus)
                        {
                            pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                            pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                            pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        btnGetLIFXLights.IsEnabled = true;
                        btnGetLIFXGroups.IsEnabled = true;

                        pnlLIFXData.Visibility = Visibility.Visible;
                        lblLIFXMessage.Text = "Connected to LIFX Cloud";
                        fontBrush.Color = "#009933".MapColor();
                        lblLIFXMessage.Foreground = fontBrush;

                        if (SettingsHandlerBase.Config.LightSettings.LIFX.UseActivityStatus)
                        {
                            pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                            pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                            pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    btnGetLIFXLights.IsEnabled = false;
                    btnGetLIFXGroups.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                _diagClient.TrackException(ex);
                _logger.LogError(ex, "Error occured Checking LIFX");
                lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                fontBrush.Color = "#ff3300".MapColor();
                lblLIFXMessage.Foreground = fontBrush;
            }

            imgLIFXLoading.Visibility = Visibility.Collapsed;
        }

        private void ddlLIFXLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlLIFXLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Group))
                {
                    SettingsHandlerBase.Config.LightSettings.LIFX.SelectedItemId = $"group_id:{((LifxCloud.NET.Models.Group)ddlLIFXLights.SelectedItem).Id}";
                    lifxItemType.Content = "Groups";
                }

                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    SettingsHandlerBase.Config.LightSettings.LIFX.SelectedItemId = $"id:{((LifxCloud.NET.Models.Light)ddlLIFXLights.SelectedItem).Id}";
                    lifxItemType.Content = "Lights";
                }

                SettingsHandlerBase.SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckLIFX_Click(object sender, RoutedEventArgs e)
        {
            imgLIFXLoading.Visibility = Visibility.Visible;
            pnlLIFXData.Visibility = Visibility.Collapsed;
            lblLIFXMessage.Visibility = Visibility.Collapsed;
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(lifxApiKey.Text))
            {
                try
                {
                    SettingsHandlerBase.Config.LightSettings.LIFX.LIFXApiKey = lifxApiKey.Text;

                    SettingsHandlerBase.SyncOptions();
                    if (((System.Windows.Controls.Button)e.Source).Name == "btnGetLIFXGroups")
                    {
                        ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllGroupsCommand()).ConfigureAwait(true);
                        lifxItemType.Content = "Groups";
                    }
                    else
                    {
                        ddlLIFXLights.ItemsSource = await _mediator.Send(new GetAllLightsCommand()).ConfigureAwait(true);
                        lifxItemType.Content = "Lights";
                    }

                    lblLIFXMessage.Visibility = Visibility.Visible;
                    pnlLIFXData.Visibility = Visibility.Visible;
                    lblLIFXMessage.Text = "Connected to LIFX Cloud";

                    btnGetLIFXLights.IsEnabled = true;
                    btnGetLIFXGroups.IsEnabled = true;
                    fontBrush.Color = "#009933".MapColor();
                    lblLIFXMessage.Foreground = fontBrush;

                    if (SettingsHandlerBase.Config.LightSettings.LIFX.UseActivityStatus)
                    {
                        pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                        pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                        pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Getting LIFX Lights");
                    _diagClient.TrackException(ex);
                    lblLIFXMessage.Visibility = Visibility.Visible;
                    pnlLIFXData.Visibility = Visibility.Collapsed;
                    lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                    fontBrush.Color = "#ff3300".MapColor();

                    btnGetLIFXLights.IsEnabled = false;
                    btnGetLIFXGroups.IsEnabled = false;
                    lblLIFXMessage.Foreground = fontBrush;
                }
            }
            else
            {
                Run run1 = new Run("Valid LIFX Key Required ");
                Run run2 = new Run(" https://cloud.lifx.com/settings");

                Hyperlink hyperlink = new Hyperlink(run2)
                {
                    NavigateUri = new Uri("https://cloud.lifx.com/settings")
                };
                hyperlink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(Hyperlink_RequestNavigate); //to be implemented
                lblLIFXMessage.Inlines.Clear();
                lblLIFXMessage.Inlines.Add(run1);
                lblLIFXMessage.Inlines.Add(hyperlink);

                btnGetLIFXLights.IsEnabled = false;
                btnGetLIFXGroups.IsEnabled = false;
                fontBrush.Color = "#ff3300".MapColor();
                lblLIFXMessage.Foreground = fontBrush;

            }

            imgLIFXLoading.Visibility = Visibility.Collapsed;
        }

        private void cbIsLIFXEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.LIFX.IsEnabled)
            {
                getTokenLink.Visibility = Visibility.Visible;
                pnlLIFX.Visibility = Visibility.Visible;
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
                pnlLIFX.Visibility = Visibility.Collapsed;
            }
            CheckLIFX();

            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void cbUseLIFXActivityStatus(object sender, RoutedEventArgs e)
        {
            if (SettingsHandlerBase.Config.LightSettings.LIFX.UseActivityStatus)
            {
                pnlLIFXAvailableStatuses.Visibility = Visibility.Collapsed;
                pnlLIFXActivityStatuses.Visibility = Visibility.Visible;
            }
            else
            {
                pnlLIFXAvailableStatuses.Visibility = Visibility.Visible;
                pnlLIFXActivityStatuses.Visibility = Visibility.Collapsed;
            }
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void cbLIFXIsDisabledChange(object sender, RoutedEventArgs e)
        {
            CheckBox cb = e.Source as CheckBox ?? throw new ArgumentException("Check Box Not Found");
            var cbName = cb.Name.Replace("Disabled", "Colour");
            var colorpicker = (Xceed.Wpf.Toolkit.ColorPicker)this.FindName(cbName);
            colorpicker.IsEnabled = cb.IsChecked == false;
            SettingsHandlerBase.SyncOptions();
            e.Handled = true;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var url = e.Uri.AbsoluteUri;
            Helpers.OpenBrowser(url);
            e.Handled = true;
        }
        #endregion
    }
}
