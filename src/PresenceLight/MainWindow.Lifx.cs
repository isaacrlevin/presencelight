using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using LifxCloud.NET.Models;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region Lifx Panel

        private async void SaveLifxSettings_Click(object sender, RoutedEventArgs e)
        {
            await SettingsService.SaveSettings(Config);
            lblLifxSaved.Visibility = Visibility.Visible;
        }

        private async void CheckLifxSettings()
        {
            SolidColorBrush fontBrush = new SolidColorBrush();
            try
            {
                if (Config.IsLifxEnabled && !string.IsNullOrEmpty(Config.LifxApiKey) && !string.IsNullOrEmpty(Config.SelectedLifxItemId))
                {
                    ddlLifxLights.ItemsSource = await _lifxService.GetAllLightsAsync();

                    foreach (var item in ddlLifxLights.Items)
                    {
                        var light = (Light)item;
                        if ($"id:{light.Id}" == Config.SelectedLifxItemId)
                        {
                            ddlLifxLights.SelectedItem = item;
                        }
                    }

                    if (ddlLifxLights.SelectedItem == null)
                    {
                        ddlLifxLights.ItemsSource = await _lifxService.GetAllGroupsAsync();

                        foreach (var item in ddlLifxLights.Items)
                        {
                            var group = (LifxCloud.NET.Models.Group)item;
                            if ($"group_id:{group.Id}" == Config.SelectedLifxItemId)
                            {
                                ddlLifxLights.SelectedItem = item;
                            }
                        }
                    }

                    if (ddlLifxLights.SelectedItem != null)
                    {
                        
                        ddlLifxLights.Visibility = Visibility.Visible;
                        lblLifxMessage.Text = "Connected to Lifx Cloud";
                        fontBrush.Color = MapColor("#009933");
                        lblLifxMessage.Foreground = fontBrush;
                    }
                }
            }
            catch
            {
                lblLifxMessage.Text = "Error Occured Connecting to Lifx, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lblLifxMessage.Foreground = fontBrush;
            }
        }

        private void ddlLifxLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlLifxLights.SelectedItem != null)
            {
                // Get whether item is group or light
                if (ddlLifxLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Group))
                {
                    Config.SelectedLifxItemId = $"group_id:{((LifxCloud.NET.Models.Group)ddlLifxLights.SelectedItem).Id}";
                }

                if (ddlLifxLights.SelectedItem.GetType() == typeof(LifxCloud.NET.Models.Light))
                {
                    Config.SelectedLifxItemId = $"id:{((LifxCloud.NET.Models.Light)ddlLifxLights.SelectedItem).Id}";

                }
                _options.SelectedLifxItemId = Config.SelectedLifxItemId;

            }
        }

        private async void CheckLifx_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush fontBrush = new SolidColorBrush();

            if (!string.IsNullOrEmpty(lifxApiKey.Text))
            {
                try
                {
                    _options.LifxApiKey = lifxApiKey.Text;
                    Config.LifxApiKey = lifxApiKey.Text;

                    if (((System.Windows.Controls.Button)sender).Name == "btnGetLifxGroups")
                    {
                        ddlLifxLights.ItemsSource = await _lifxService.GetAllGroupsAsync();
                    }
                    else
                    {
                        ddlLifxLights.ItemsSource = await _lifxService.GetAllLightsAsync();
                    }

                    ddlLifxLights.Visibility = Visibility.Visible;
                    lblLifxMessage.Text = "Connected to Lifx Cloud";
                    fontBrush.Color = MapColor("#009933");
                    lblLifxMessage.Foreground = fontBrush;
                }
                catch
                {
                    ddlLifxLights.Visibility = Visibility.Collapsed;
                    lblLifxMessage.Text = "Error Occured Connecting to Lifx, please try again";
                    fontBrush.Color = MapColor("#ff3300");
                    lblLifxMessage.Foreground = fontBrush;
                }
            }
            else
            {

                Run run1 = new Run("Valid Lifx Key Required ");
                Run run2 = new Run(" https://cloud.lifx.com/settings");

                Hyperlink hyperlink = new Hyperlink(run2)
                {
                    NavigateUri = new Uri("https://cloud.lifx.com/settings")
                };
                hyperlink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(Hyperlink_RequestNavigate); //to be implemented
                lblLifxMessage.Inlines.Clear();
                lblLifxMessage.Inlines.Add(run1);
                lblLifxMessage.Inlines.Add(hyperlink);


                fontBrush.Color = MapColor("#ff3300");
                lblLifxMessage.Foreground = fontBrush;

            }
        }

        private void cbIsLifxEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.IsLifxEnabled)
            {
                pnlLifx.Visibility = Visibility.Visible;
            }
            else
            {
                pnlLifx.Visibility = Visibility.Collapsed;
            }
        }

        #endregion


    }
}
