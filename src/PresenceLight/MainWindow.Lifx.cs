using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;
using PresenceLight.Telemetry;
using System.Windows.Navigation;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region LIFX Panel

        private async void LIFXToken_Get(object sender, RequestNavigateEventArgs e)
        {
            string accessToken = await _lIFXOAuthHelper.InitiateTokenRetrieval();
            if (!string.IsNullOrEmpty(accessToken))
            {
                lifxApiKey.Text = accessToken;
                Config.LightSettings.LIFX.LIFXApiKey = accessToken;
                SyncOptions();
            }
            this.Activate();
        }

        private async void SaveLIFX_Click(object sender, RoutedEventArgs e)
        {
            btnLIFX.IsEnabled = false;
            await SettingsService.SaveSettings(Config);
            lblLIFXSaved.Visibility = Visibility.Visible;
            btnLIFX.IsEnabled = true;
        }

        private async void CheckLIFX()
        {
            SolidColorBrush fontBrush = new SolidColorBrush();
            try
            {
                if (Config.LightSettings.LIFX.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LightSettings.LIFX.LIFXApiKey) && !string.IsNullOrEmpty(Config.LightSettings.LIFX.SelectedLIFXItemId))
                {
                    ddlLIFXLights.ItemsSource = await _lifxService.GetAllLightsAsync();

                    foreach (var item in ddlLIFXLights.Items)
                    {
                        if (item != null)
                        {
                            var light = (Light)item;
                            if ($"id:{light?.Id}" == Config.LightSettings.LIFX.SelectedLIFXItemId)
                            {
                                ddlLIFXLights.SelectedItem = item;
                            }
                        }
                    }

                    if (ddlLIFXLights.SelectedItem == null)
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllGroupsAsync();

                        foreach (var item in ddlLIFXLights.Items)
                        {
                            if (item != null)
                            {
                                var group = (LifxCloud.NET.Models.Group)item;
                                if ($"group_id:{group?.Id}" == Config.LightSettings.LIFX.SelectedLIFXItemId)
                                {
                                    ddlLIFXLights.SelectedItem = item;
                                }
                            }
                        }
                    }

                    if (ddlLIFXLights.SelectedItem != null)
                    {

                        pnlLIFXBrigthness.Visibility = Visibility.Visible;
                        lblLIFXMessage.Text = "Connected to LIFX Cloud";
                        fontBrush.Color = MapColor("#009933");
                        lblLIFXMessage.Foreground = fontBrush;
                    }
                }
            }
            catch (Exception ex)
            {
                DiagnosticsClient.TrackException(ex);

                lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lblLIFXMessage.Foreground = fontBrush;
            }
        }

        private void ddlLIFXLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlLIFXLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Group))
                {
                    Config.LightSettings.LIFX.SelectedLIFXItemId = $"group_id:{((LifxCloud.NET.Models.Group)ddlLIFXLights.SelectedItem).Id}";
                }

                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    Config.LightSettings.LIFX.SelectedLIFXItemId = $"id:{((LifxCloud.NET.Models.Light)ddlLIFXLights.SelectedItem).Id}";
                }
                
                SyncOptions();
            }
            e.Handled = true;
        }

        private async void CheckLIFX_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(lifxApiKey.Text))
            {
                try
                {
                    Config.LightSettings.LIFX.LIFXApiKey = lifxApiKey.Text;

                    SyncOptions();
                    if (((System.Windows.Controls.Button)sender).Name == "btnGetLIFXGroups")
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllGroupsAsync();
                    }
                    else
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllLightsAsync();
                    }

                    pnlLIFXBrigthness.Visibility = Visibility.Visible;
                    lblLIFXMessage.Text = "Connected to LIFX Cloud";
                    fontBrush.Color = MapColor("#009933");
                    lblLIFXMessage.Foreground = fontBrush;
                }
                catch (Exception ex)
                {
                    DiagnosticsClient.TrackException(ex);

                    pnlLIFXBrigthness.Visibility = Visibility.Collapsed;
                    lblLIFXMessage.Text = "Error Occured Connecting to LIFX, please try again";
                    fontBrush.Color = MapColor("#ff3300");
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


                fontBrush.Color = MapColor("#ff3300");
                lblLIFXMessage.Foreground = fontBrush;

            }
        }

        private void cbIsLIFXEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.LightSettings.LIFX.IsLIFXEnabled)
            {
                getTokenLink.Visibility = Visibility.Visible;
                pnlLIFX.Visibility = Visibility.Visible;
            }
            else
            {
                getTokenLink.Visibility = Visibility.Collapsed;
                pnlLIFX.Visibility = Visibility.Collapsed;
            }
            
            SyncOptions();
            e.Handled = true;
        }

        #endregion
    }
}
