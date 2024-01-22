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

        public static string GetInstallLocation()
        {
            return System.AppContext.BaseDirectory;
        }

        public static string GetInstallationDate()
        {
            var date = System.IO.File.GetLastWriteTime(System.AppContext.BaseDirectory);
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        }

        public string GetApplicationVersion()
        {
            return _config["AppVersion"];
        }

        public static string GetDotNetRuntimeInfo()
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

            if (new DesktopBridge.Helpers().IsRunningAsUwp())
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
