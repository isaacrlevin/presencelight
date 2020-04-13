using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using OSVersionHelper;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;

namespace PresenceLight
{
    internal class ThisAppInfo
    {
        internal static string GetDisplayName()
        {
            if (WindowsVersionHelper.HasPackageIdentity)
            {
                return Package.Current.DisplayName;
            }
            return "Not packaged";
        }

        internal static string GetThisAssemblyVersion()
        {
            if (typeof(MainWindow).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>() == null)
            {
                return "";
            }
            else
            {
                return typeof(MainWindow).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            }
        }

        internal static string GetInstallLocation()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        internal static string GetInstallationDate()
        {
            var date = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        }

        internal static string GetPackageVersion()
        {
            if (WindowsVersionHelper.HasPackageIdentity)
            {
                return $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
            }
            return "Not packaged";
        }

        internal static string GetPackageChannel()
        {
            if(WindowsVersionHelper.HasPackageIdentity)
            {
                return Package.Current.Id.Name.Substring(Package.Current.Id.Name.LastIndexOf('.') + 1);
            }

            return null;
        }

        internal static string GetDotNetInfo()
        {
            var runTimeDir = new FileInfo(typeof(string).Assembly.Location);
            var entryDir = new FileInfo(Assembly.GetEntryAssembly().Location);
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

            if (!WindowsVersionHelper.HasPackageIdentity) return "Not packaged";

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

        internal static string GetDotNetRuntimeInfo()
        {
            return typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Uri GetAppInstallerInfoUri(Package p)
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
