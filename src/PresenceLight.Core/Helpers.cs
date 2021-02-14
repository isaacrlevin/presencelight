using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace PresenceLight.Core
{
    public enum HoursPassedStatus
    {
        Off,
        Keep,
        White
    }

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
                Console.WriteLine("Hello");
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

        public static string HumanifyText(string text)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Replace(text, " ");
        }

        public static BaseConfig CleanColors(BaseConfig config)
        {
            ReadPropertiesRecursive(config);
            return config;
        }

        public static object ReadPropertiesRecursive(object obj)
        {
            if (obj != null)
            {
                foreach (var property in obj.GetType().GetProperties())
                {
                    //for value types
                    if (property.PropertyType.IsPrimitive || property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                    {
                        if (property.Name == "Colour")
                        {
                            var uncleanColor = property.GetValue(obj).ToString();
                            uncleanColor = uncleanColor.Replace("#", "");

                            string cleanColor = "";
                            switch (uncleanColor.Length)
                            {
                                case var length when uncleanColor.Length == 6:
                                    cleanColor = uncleanColor;
                                    // Do Nothing
                                    break;
                                case var length when uncleanColor.Length > 6:
                                    // Get last 6 characters
                                    cleanColor = uncleanColor.Substring(uncleanColor.Length - 6);
                                    break;
                                default:
                                    throw new ArgumentException("Supplied Color had an issue");
                            }
                            property.SetValue(obj, $"#{cleanColor}");
                        }

                    }
                    //for complex types
                    else if (property.PropertyType.IsClass && !typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        ReadPropertiesRecursive(property.GetValue(obj));
                    }
                    //for Enumerables
                    else
                    {
                        var enumerablePropObj1 = property.GetValue(obj) as IEnumerable;

                        if (enumerablePropObj1 == null) continue;

                        var objList = enumerablePropObj1.GetEnumerator();

                        while (objList.MoveNext())
                        {
                            objList.MoveNext();
                            ReadPropertiesRecursive(objList.Current);
                        }
                    }
                }
            }

            return obj;
        }

        public static string HoursPassedStatusString(HoursPassedStatus status) =>
            status switch
            {
                HoursPassedStatus.Keep => "Keep",
                HoursPassedStatus.White => "White",
                HoursPassedStatus.Off => "Off",
                _ => throw new ArgumentException(message: "Invalid HoursPassedStatus Value", paramName: nameof(status)),
            };
    }
}
