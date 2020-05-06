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
                Config.LIFXApiKey = accessToken;
                SyncOptions();
            }
            this.Activate();
        }

        private async void SaveLIFXSettings_Click(object sender, RoutedEventArgs e)
        {
            btnLIFX.IsEnabled = false;
            await SettingsService.SaveSettings(Config);
            lblLIFXSaved.Visibility = Visibility.Visible;
            btnLIFX.IsEnabled = true;
        }

        private async void CheckLIFXSettings()
        {
            SolidColorBrush fontBrush = new SolidColorBrush();
            try
            {
                if (Config.IsLIFXEnabled && !string.IsNullOrEmpty(Config.LIFXApiKey) && !string.IsNullOrEmpty(Config.SelectedLIFXItemId))
                {
                    ddlLIFXLights.ItemsSource = await _lifxService.GetAllLightsAsync();

                    foreach (var item in ddlLIFXLights.Items)
                    {
                        if (item != null)
                        {
                            var light = (Light)item;
                            if ($"id:{light?.Id}" == Config.SelectedLIFXItemId)
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
                                if ($"group_id:{group?.Id}" == Config.SelectedLIFXItemId)
                                {
                                    ddlLIFXLights.SelectedItem = item;
                                }
                            }
                        }
                    }

                    if (ddlLIFXLights.SelectedItem != null)
                    {

                        ddlLIFXLights.Visibility = Visibility.Visible;
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
                    Config.SelectedLIFXItemId = $"group_id:{((LifxCloud.NET.Models.Group)ddlLIFXLights.SelectedItem).Id}";
                }

                if (ddlLIFXLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    Config.SelectedLIFXItemId = $"id:{((LifxCloud.NET.Models.Light)ddlLIFXLights.SelectedItem).Id}";
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
                    Config.LIFXApiKey = lifxApiKey.Text;

                    SyncOptions();
                    if (((System.Windows.Controls.Button)sender).Name == "btnGetLIFXGroups")
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllGroupsAsync();
                    }
                    else
                    {
                        ddlLIFXLights.ItemsSource = await _lifxService.GetAllLightsAsync();
                    }

                    ddlLIFXLights.Visibility = Visibility.Visible;
                    lblLIFXMessage.Text = "Connected to LIFX Cloud";
                    fontBrush.Color = MapColor("#009933");
                    lblLIFXMessage.Foreground = fontBrush;
                }
                catch (Exception ex)
                {
                    DiagnosticsClient.TrackException(ex);

                    ddlLIFXLights.Visibility = Visibility.Collapsed;
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
            if (Config.IsLIFXEnabled)
            {
                pnlLIFX.Visibility = Visibility.Visible;
            }
            else
            {
                pnlLIFX.Visibility = Visibility.Collapsed;
            }
            
            SyncOptions();
            e.Handled = true;
        }

        #endregion
    }
}
