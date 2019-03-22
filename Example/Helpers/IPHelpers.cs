// <copyright file="IPHelpers.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Example.Helpers
{
    /// <summary>
    /// IPHelpers,  methods used with networking
    /// </summary>
    public class IPHelpers
    {
        /// <summary>
        /// GetLocalIPAddresses, returns a list of the ip address that each adapter represents
        /// </summary>
        /// <returns>List of strings with each string representing the IP address of an adapter</returns>
        public static List<IPAddress> GetLocalIPAddresses()
        {
            IPAddress[] localIPS = Dns.GetHostAddresses(Dns.GetHostName());
            return localIPS != null ? localIPS.ToList() : new List<IPAddress>();
        }

        /// <summary>
        /// GetNetworkAdapters, returns the list of network adapters currently configured on the machine
        /// </summary>
        /// <returns>The list of NetworkInterface on the machine</returns>
        public static List<NetworkInterface> GetNetworkAdapters()
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            return adapters != null ? adapters.ToList() : new List<NetworkInterface>();
        }
    }
}
