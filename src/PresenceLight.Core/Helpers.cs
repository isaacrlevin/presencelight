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
    /// <summary>
    /// Represents the status of hours passed.
    /// </summary>
    public enum HoursPassedStatus
    {
        Off,
        Keep,
        White
    }

    /// <summary>
    /// Provides helper methods for various operations.
    /// </summary>
    public static class Helpers
    {

        public static bool AreStringsNotEmpty(string[] strings)
        {
            bool result = true;

            foreach (var s in strings)
            {
                result = result && !string.IsNullOrEmpty(s);
            }

            return result;
        }

        /// <summary>
        /// Opens the specified URL in the default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
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

        /// <summary>
        /// Converts a camel case or pascal case string into a human-readable format by inserting spaces between words.
        /// </summary>
        /// <param name="text">The input string to be converted.</param>
        /// <returns>The converted string with spaces between words.</returns>
        public static string HumanifyText(string text)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            return r.Replace(text, " ");
        }

        /// <summary>
        /// Converts the given HoursPassedStatus value to its corresponding string representation.
        /// </summary>
        /// <param name="status">The HoursPassedStatus value to convert.</param>
        /// <returns>The string representation of the HoursPassedStatus value.</returns>
        public static string HoursPassedStatusString(HoursPassedStatus status) =>
            status switch
            {
                HoursPassedStatus.Keep => "Keep",
                HoursPassedStatus.White => "White",
                HoursPassedStatus.Off => "Off",
                _ => throw new ArgumentException(message: "Invalid HoursPassedStatus Value", paramName: nameof(status)),
            };
    
        /// <summary>
        /// Replaces the variables in the given body with the provided availability and activity.
        /// </summary>
        /// <param name="body">The body in which to replace the variables.</param>
        /// <param name="availability">The availability to replace the {{availability}} variable.</param>
        /// <param name="activity">The activity to replace the {{activity}} variable.</param>
        /// <returns>The body with the variables replaced.</returns>
        public static string ReplaceVariables(string body, string? availability, string? activity)
        {
            if (body.Contains("{{availability}}"))
            {
                body = body.Replace("{{availability}}", availability ?? string.Empty);
            }

            if (body.Contains("{{activity}}"))
            {
                body = body.Replace("{{activity}}", activity ?? string.Empty);
            }
            return body;
        }

    }
}
