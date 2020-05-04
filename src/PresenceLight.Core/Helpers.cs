using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PresenceLight.Core
{
    public static class Helpers
    {
        public static void OpenBrowser(string url)
        {
            // Opens request in the browser.
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo(url));
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    System.Diagnostics.Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    System.Diagnostics.Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    System.Diagnostics.Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
