﻿using System;
using PresenceLight.Core;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using PresenceLight.Telemetry;

namespace PresenceLight
{
    public partial class MainWindow : Window
    {
        #region Hue Panel

        private async void SaveHueSettings_Click(object sender, RoutedEventArgs e)
        {
            btnHue.IsEnabled = false;
            await SettingsService.SaveSettings(Config);
            _hueService = new HueService(Config);
            CheckHueSettings();
            lblHueSaved.Visibility = Visibility.Visible;
            btnHue.IsEnabled = true;
        }

        private void ddlHueLights_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ddlHueLights.SelectedItem != null)
            {
                Config.SelectedHueLightId = ((Q42.HueApi.Light)ddlHueLights.SelectedItem).Id;
                _options.SelectedHueLightId = Config.SelectedHueLightId;
            }
            e.Handled = true;
        }

        private void HueIpAddress_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (((TextBox)e.OriginalSource).Text.Trim() != ((TextBox)e.Source).Text.Trim())
            {
                if (_options != null)
                {
                    _options.HueApiKey = String.Empty;
                }
                if (Config != null)
                {
                    Config.HueApiKey = String.Empty;
                }
            }
            CheckHueSettings();
            e.Handled = true;
        }
        private async void CheckHueSettings()
        {
            if (Config != null)
            {
                SolidColorBrush fontBrush = new SolidColorBrush();


                if (!CheckHueIp())
                {
                    lblHueMessage.Text = "Valid IP Address Required";
                    fontBrush.Color = MapColor("#ff3300");
                    btnRegister.IsEnabled = false;
                    ddlHueLights.Visibility = Visibility.Collapsed;
                    lblHueMessage.Foreground = fontBrush;
                }
                else
                {
                    if (string.IsNullOrEmpty(Config.HueIpAddress))
                    {
                        Config.HueIpAddress = hueIpAddress.Text;
                    }

                    if (string.IsNullOrEmpty(_options.HueIpAddress))
                    {
                        _options.HueIpAddress = hueIpAddress.Text;
                    }
                    btnRegister.IsEnabled = true;
                    if (string.IsNullOrEmpty(Config.HueApiKey))
                    {
                        lblHueMessage.Text = "Missing App Registration, please press button on bridge then click 'Register Bridge'";
                        fontBrush.Color = MapColor("#ff3300");
                        ddlHueLights.Visibility = Visibility.Collapsed;
                        lblHueMessage.Foreground = fontBrush;
                    }
                    else
                    {
                        ddlHueLights.ItemsSource = await _hueService.CheckLights();

                        foreach (var item in ddlHueLights.Items)
                        {
                            var light = (Q42.HueApi.Light)item;
                            if (light?.Id == Config.SelectedHueLightId)
                            {
                                ddlHueLights.SelectedItem = item;
                            }
                        }
                        ddlHueLights.Visibility = Visibility.Visible;
                        lblHueMessage.Text = "App Registered with Bridge";
                        fontBrush.Color = MapColor("#009933");
                        lblHueMessage.Foreground = fontBrush;
                    }
                }
            }
        }

        private bool CheckHueIp()
        {
            string r2 = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

            Regex r = new Regex(r2);

            if (string.IsNullOrEmpty(hueIpAddress.Text.Trim()) || !r.IsMatch(hueIpAddress.Text.Trim()) || hueIpAddress.Text.Trim().EndsWith("."))
            {
                return false;
            }
            return true;
        }

        private async void FindBridge_Click(object sender, RoutedEventArgs e)
        {
            hueIpAddress.Text = await _hueService.FindBridge();
        }

        private void cbIsPhillipsEnabledChanged(object sender, RoutedEventArgs e)
        {
            if (Config.IsPhillipsEnabled)
            {
                pnlPhillips.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPhillips.Visibility = Visibility.Collapsed;
            }
            _options.IsPhillipsEnabled = Config.IsPhillipsEnabled;
            e.Handled = true;
        }

        private async void RegisterBridge_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxHelper.PrepToCenterMessageBoxOnForm(this);
            MessageBox.Show("Please press the sync button on your Phillips Hue Bridge");

            SolidColorBrush fontBrush = new SolidColorBrush();

            try
            {
                imgLoading.Visibility = Visibility.Visible;
                lblHueMessage.Visibility = Visibility.Collapsed;
                if (string.IsNullOrEmpty(_options.HueIpAddress))
                {
                    _options.HueIpAddress = Config.HueIpAddress;
                }
                Config.HueApiKey = await _hueService.RegisterBridge();
                ddlHueLights.ItemsSource = await _hueService.CheckLights();
                _options.HueApiKey = Config.HueApiKey;
                ddlHueLights.Visibility = Visibility.Visible;
                imgLoading.Visibility = Visibility.Collapsed;
                lblHueMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                DiagnosticsClient.TrackException(ex);

                lblHueMessage.Text = "Error Occured registering bridge, please try again";
                fontBrush.Color = MapColor("#ff3300");
                lblHueMessage.Foreground = fontBrush;
            }

            if (!string.IsNullOrEmpty(Config.HueApiKey))
            {
                lblHueMessage.Text = "App Registered with Bridge";
                fontBrush.Color = MapColor("#009933");
                lblHueMessage.Foreground = fontBrush;
            }

            CheckHueSettings();
        }

        #endregion
    }
}
