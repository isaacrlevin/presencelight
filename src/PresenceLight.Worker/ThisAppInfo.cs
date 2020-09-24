using System;
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

        internal static string GetDotNetRuntimeInfo()
        {
            return typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
