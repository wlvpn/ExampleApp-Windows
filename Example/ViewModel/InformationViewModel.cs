// <copyright file="InformationViewModel.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Example.Helpers;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// InformationViewModel
    /// </summary>
    public class InformationViewModel : BindableBase
    {
        private IDisposable _timer;
        private DateTime _startTime;
        private DateTime _endTime;
        private string _externIP = "Retrieving...";
        private string _currentLocation = "Retrieving...";

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationViewModel "/> class.
        /// </summary>
        public InformationViewModel()
        {
            SDK.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
            SDK.UserLocationStatusChanged += OnUserLocationStatusChanged;
            UpdateInformation();
        }

        /// <summary>
        /// Gets SDK Instance.
        /// </summary>
        public ISDK SDK => SDKFactory.GetInstance();

        /// <summary>
        /// Gets a value indicating whether the elapsed time of the last/current VPN connection
        /// </summary>
        public TimeSpan TimeElapsed
        {
            get { return _endTime.Subtract(_startTime); }
        }

        /// <summary>
        /// Gets a value that represents the list of IP addresses for all of the adapters
        /// </summary>
        /// <value>an observable collection of strings that are the IP addresses</value>
        public SmartCollection<IPAddress> IPAddresses { get; private set; } = new SmartCollection<IPAddress>();

        /// <summary>
        /// Gets or sets a value indicating the current external IP address
        /// </summary>
        /// <value>IP address that this machine currently presents to the outside world</value>
        public string ExternalIPAddress
        {
            get { return _externIP; }
            set { SetProperty(ref _externIP, value); }
        }

        /// <summary>
        /// Gets or sets the current location on the information view.
        /// </summary>
        /// <value>city and country name of current location.</value>
        public string CurrentLocation
        {
            get { return _currentLocation; }
            set { SetProperty(ref _currentLocation, value); }
        }

        /// <summary>
        /// Gets a value that is the list of network adapters present on the machine
        /// </summary>
        /// <value>an observable collection of NetworkInterface</value>
        public SmartCollection<NetworkInterface> NetworkAdapters { get; private set; } = new SmartCollection<NetworkInterface>();

        /// <summary>
        /// Updates the Connection State, List of Network Adapters and the IP Address for this machine.
        /// </summary>
        private void UpdateInformation()
        {
            Task.Run(async () =>
            {
               NetworkAdapters.Clear();
               var networkAdapters = await Task.Run(() => IPHelpers.GetNetworkAdapters());
               NetworkAdapters.Repopulate(networkAdapters);
               var localIp = await Task.Run(() => IPHelpers.GetLocalIPAddresses());
               IPAddresses.Repopulate(localIp);
            });
        }

        private void OnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            if (current == ConnectionStatus.Connected)
            {
                _startTime = DateTime.Now;
                _endTime = DateTime.Now;

                _timer = Observable.Interval(TimeSpan.FromSeconds(1))
                    .StartWith(0)
                    .Subscribe(tick =>
                    {
                        _endTime = DateTime.Now;
                        OnPropertyChanged(nameof(TimeElapsed));
                    });

                UpdateInformation();
            }

            if (current == ConnectionStatus.Disconnected)
            {
                _timer?.Dispose();
                _timer = null;

                UpdateInformation();
            }
        }

        private void OnUserLocationStatusChanged(ISDK sender, OperationStatus status, NetworkGeolocation args)
        {
            switch (status)
            {
                case OperationStatus.InProgress:
                    CurrentLocation = "Retrieving...";
                    ExternalIPAddress = "Retrieving...";
                    break;

                case OperationStatus.Completed:
                    CurrentLocation = $"{args.City}, {args.Country}";
                    ExternalIPAddress = args.IPAddress.ToString();
                    break;
            }
        }
    }
}