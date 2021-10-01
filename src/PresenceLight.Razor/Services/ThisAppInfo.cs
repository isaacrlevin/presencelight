using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace PresenceLight
{
    internal class ThisAppInfo
    {
        internal static string GetInstallLocation()
        {
            return System.AppContext.BaseDirectory;
        }

        internal static string GetInstallationDate()
        {
            var date = System.IO.File.GetLastWriteTime(System.AppContext.BaseDirectory);
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
        }

        public static string GetApplicationVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.FileVersion;
        }

        internal static string GetDotNetRuntimeInfo()
        {
            return typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
