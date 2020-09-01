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
            return Assembly.GetExecutingAssembly().Location;
        }

        internal static string GetInstallationDate()
        {
            var date = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
            return $"{date.ToShortDateString()} {date.ToShortTimeString()}";
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

        internal static string GetDotNetRuntimeInfo()
        {
            return typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }
    }
}
