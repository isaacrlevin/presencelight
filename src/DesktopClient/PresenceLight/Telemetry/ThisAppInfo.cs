using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
//using OSVersionHelper;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;

namespace PresenceLight
{
    internal class ThisAppInfo
    {
        internal static string GetDisplayName()
        {
            try
            {
                return Package.Current.DisplayName;
            }
            catch
            {
                return "Not packaged";
            }
        }

        internal static string GetInstallLocation()
        {
            return System.AppContext.BaseDirectory;
        }

        internal static string GetInstallationDate()
        {
            var date = System.IO.File.GetLastWriteTime(System.AppContext.BaseDirectory);
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        }

        internal static string GetAppInstallType()
        {
            if (Convert.ToBoolean(App.StaticConfig["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                return "AppPackage";
            }
            else
            {
                return "Standalone";
            }
        }

        internal static string GetSettingsLocation()
        {
            string settingsFileName = "settings.json";
            string settingsPath = "";

            if (Convert.ToBoolean(App.StaticConfig["IsAppPackaged"], CultureInfo.InvariantCulture))
            {
                StorageFolder _settingsFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                settingsPath = _settingsFolder.Path;
            }
            else
            {
               settingsPath = System.AppContext.BaseDirectory;
            }
            return $"{settingsPath}{settingsFileName}";
        }

        internal static string GetPackageVersion()
        {
            try
            {
                return $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
            }
            catch
            {
                return "Not packaged";
            }
        }

        internal static string? GetPackageChannel()
        {
            try
            {
                return Package.Current.Id.Name.Substring(Package.Current.Id.Name.LastIndexOf('.') + 1);
            }
            catch
            {
                return null;
            }
        }

        internal static string GetDotNetInfo()
        {
            var runTimeDir = new FileInfo(System.AppContext.BaseDirectory);
            var entryDir = new FileInfo(System.AppContext.BaseDirectory);
            var IsSelfContaied = runTimeDir.DirectoryName == entryDir.DirectoryName;

            var result = ".NET Framework - ";
            if (IsSelfContaied)
            {
                result += "Self Contained Deployment";
            }
            else
            {
                result += "Framework Dependent Deployment";
            }

            return result;
        }

        internal static string GetAppInstallerUri()
        {
            string result;

            try
            {
                if (ApiInformation.IsMethodPresent("Windows.ApplicationModel.Package", "GetAppInstallerInfo"))
                {
                    var aiUri = GetAppInstallerInfoUri(Package.Current);
                    if (aiUri != null)
                    {
                        result = aiUri.ToString();
                    }
                    else
                    {
                        result = "not present";
                    }
                }
                else
                {
                    result = "Not Available";
                }

                return result;
            }
            catch
            {
                return "Not packaged";
            }
        }

        internal static string GetDotNetRuntimeInfo()
        {
            return typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Uri? GetAppInstallerInfoUri(Package p)
        {
            var aiInfo = p.GetAppInstallerInfo();
            if (aiInfo != null)
            {
                return aiInfo.Uri;
            }
            return null;
        }
    }
}
