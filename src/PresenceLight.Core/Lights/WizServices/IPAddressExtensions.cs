using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PresenceLight.Core.Lights.WizServices
{
    internal static class IPAddressExtensions
    {
        /// <summary>
        /// Check if given IPv4 is a link-local (auto configuration) address (according to RFC3927)
        /// </summary>
        /// <remarks>https://tools.ietf.org/html/rfc3927</remarks>
        /// <param name="ip">The IPv4</param>
        /// <returns>True if link-local, false otherwise</returns>
        public static bool IsIPv4LinkLocal(this IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork)
            {
                // Not an IPv4, simply return false
                return false;
            }

            return ip.ToString().StartsWith("169.254.");
        }

        /// <summary>
        /// Check if given IP is a loopback
        /// <para>This is just a helper extension around the static method IPAddress.IsLoopback</para>
        /// </summary>
        /// <param name="ip">The IP</param>
        /// <returns>True if loopback, False otherwise</returns>
        public static bool IsLoopback(this IPAddress ip)
        {
            return IPAddress.IsLoopback(ip);
        }

        /// <summary>
        /// Check if given IPv4 is in private range (according to RFC1918)
        /// </summary>
        /// <remarks>https://tools.ietf.org/html/rfc1918</remarks>
        /// <param name="ip">The IPv4</param>
        /// <returns>True if private, false otherwise</returns>
        public static bool IsIPv4Private(this IPAddress ip)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork)
            {
                // Not an IPv4, simply return false
                return false;
            }

            byte[] bytes = ip.GetAddressBytes();

            switch (bytes[0])
            {
                // 10.0.0.0 - 10.255.255.255 (10/8 prefix)
                case 10:
                    return true;

                // 172.16.0.0 - 172.31.255.255 (172.16/12 prefix)
                case 172:
                    return bytes[1] < 32 && bytes[1] >= 16;

                // 192.168.0.0 - 192.168.255.255 (192.168/16 prefix)
                case 192:
                    return bytes[1] == 168;

                // Others
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get a list of all IPv4 addresses in a specified network
        /// </summary>
        /// <exception cref="ArgumentException">If IP or mask not IPv4</exception>
        /// <param name="ip">Any IP from the network</param>
        /// <param name="mask">The network mask (subnet)</param>
        /// <returns>A list of IPAddress</returns>
        public static IPAddress GetNetworkLoopbackAddress(this IPAddress ip, IPAddress mask)
        {
            if (ip.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Not an IPv4 address", nameof(ip));
            }

            if (mask.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new ArgumentException("Not an IPv4 address", nameof(mask));
            }

            List<IPAddress> range = new List<IPAddress>();

            byte[] maskBytes = mask.GetAddressBytes();
            byte[] ipBytes = ip.GetAddressBytes();

            // Start IP (network IP) = IP AND MASK
            byte[] startIpBytes = Enumerable.Range(0, 4)
              .Select(i => (byte)(ipBytes[i] & maskBytes[i]))
              .ToArray();

            // Last IP (broadcast IP) = IP OR NOT MASK
            byte[] endIpBytes = Enumerable.Range(0, 4)
              .Select(i => (byte)(ipBytes[i] | ~maskBytes[i]))
              .ToArray();

            if (!Enumerable.Range(0, 4).Any(i => startIpBytes[i] > endIpBytes[i]))
            {
                for (int b0 = startIpBytes[0]; b0 <= endIpBytes[0]; b0++)
                {
                    for (int b1 = startIpBytes[1]; b1 <= endIpBytes[1]; b1++)
                    {
                        for (int b2 = startIpBytes[2]; b2 <= endIpBytes[2]; b2++)
                        {
                            for (int b3 = startIpBytes[3]; b3 <= endIpBytes[3]; b3++)
                            {
                                range.Add(new IPAddress(new byte[] { (byte)b0, (byte)b1, (byte)b2, (byte)b3 }));
                            }
                        }
                    }
                }
            }
            else
            {
                // Something went wrong : a start byte is above an end byte and thus will lead to bad results
            }

            return range.Last();
        }
    }

    internal static class NetworkInterfaceExtensions
    {
        /// <summary>
        /// Retrieve the list of all first private IPv4 of each interfaces that are up
        /// </summary>
        /// <returns>List of private IPv4 information</returns>
        public static UnicastIPAddressInformation GetAllUpNetworkInterfacesFirstPrivateIPv4()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                  // Keep only connected interfaces
                  .Where(itf => itf.OperationalStatus == OperationalStatus.Up)
                  // Retrieve all unicast addresses of each interface
                  .SelectMany(itf => itf.GetIPProperties().UnicastAddresses)
                  // Keep only private IPv4
                  .Where(info => !info.Address.IsLoopback() && !info.Address.IsIPv4LinkLocal() && info.Address.IsIPv4Private() && info.PrefixOrigin == PrefixOrigin.Dhcp)
                  .FirstOrDefault();
        }
    }
}
