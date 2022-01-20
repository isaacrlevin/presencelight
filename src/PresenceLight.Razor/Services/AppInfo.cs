using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace PresenceLight.Razor
{
    public class AppInfo
    {
        private readonly IConfiguration _config;
        public AppInfo(IConfiguration Configuration)
        {
            _config = Configuration;
        }

        public string GetInstallLocation()
        {
            return System.AppContext.BaseDirectory;
        }

        public string GetInstallationDate()
        {
            var date = System.IO.File.GetLastWriteTime(System.AppContext.BaseDirectory);
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        }

        public string GetApplicationVersion()
        {
            var fileVersion = Process.GetCurrentProcess().MainModule.FileVersionInfo;
            return fileVersion.FileVersion;
        }

        public string GetDotNetRuntimeInfo()
        {
            return typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        public string GetAppInstallType()
        {
            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            {
                return "Container";
            }

            if (_config["AppType"] == "Web")
            {
                return "Web";
            }

            if (Convert.ToBoolean(_config["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                return "AppPackage";
            }
            else
            {
                return "Standalone";
            }
        }
    }
}
