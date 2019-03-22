// <copyright file="SDKViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DynamicData;
using DynamicData.Binding;
using Example.Helpers;
using VpnSDK.Public;
using VpnSDK.Public.Enums;
using VpnSDK.Public.Exceptions;
using VpnSDK.Public.Interfaces;
using VpnSDK.Public.Messages;

namespace Example.ViewModel
{
    /// <summary>
    /// SDKViewModel
    /// </summary>
    public class SDKViewModel : BindableBase
    {
        private readonly ISDK _manager;
        private bool _isLoggedIn = false;
        private IDisposable _serverListLoader;
        private IDisposable _serverListRefresh;
        private bool _isConnected = false;
        private string _connectionState = ConnectionStatus.Disconnected.ToString();
        private bool _isBusy = false;
        private bool _isConnecting = false;
        private string _externIP = "Retrieving...";
        private string _currentLocation = "Retrieving...";
        private DateTime _startTime;
        private DateTime _endTime;
        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _killSwitch = false;
        private NetworkConnectionType _connectionProtocol = NetworkConnectionType.IKEv2;
        private bool _openVpnScramble = false;
        private NetworkProtocolType _selectedOpenVpnProtocol = NetworkProtocolType.UDP;

        // FOR TESTS
        private MainViewModel _mainViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SDKViewModel"/> class.
        /// </summary>
        /// <param name="vm">Injected the MainViewModel instance</param>
        public SDKViewModel(MainViewModel vm)
        {
            string logFilesPath = Path.Combine(
                Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.Create),
                "SampleApp",
                "Logs",
                "SDK.log");

            if (string.IsNullOrEmpty(Properties.Settings.Default.API_KEY) ||
                string.IsNullOrEmpty(Properties.Settings.Default.AUTHORIZATION_TOKEN))
            {
                throw new VpnSDKInvalidConfigurationException("API key or Authorization token was not set.");
            }

            /*
             The values for the API_KEY and AUTHORIZATION_TOKEN for this example app are stored in Properties.Settings.
             You must replace those placeholder values with a real key and authorization token
             To obtain these values you need to be a registered WLVPN reseller.
             If you have not done so already, please visit https://wlvpn.com/#contact to get started.
             */
            _manager = new SDKBuilder()
                            .SetApiKey(Properties.Settings.Default.API_KEY)
                            .SetApplicationName(Properties.Settings.Default.APPLICATION_NAME)
                            .SetAuthenticationToken(Properties.Settings.Default.AUTHORIZATION_TOKEN)
                            .SetLogFilesPath(logFilesPath)
                            .SetOpenVpnConfiguration(new OpenVpnConfiguration
                            {
                                OpenVpnCertificateFileName = "ca.crt",
                                OpenVpnDirectory = "OpenVPN",
                                OpenVpnExecutableFileName = "openvpn.exe",
                                OpenVpnConfigDirectory = "OpenVPN",
                                OpenVpnConfigFileName = "config.ovpn",
                                OpenVpnLogFileName = "openvpn.log",
                                TapDeviceDescription = "TAP-Windows Adapter V9",
                                TapDeviceName = "tap0901"
                            })
                            .SetServerListCache(TimeSpan.FromDays(1))
                            .SetRasConfiguration(new RasConfiguration
                            {
                                RasDeviceDescription = "VPNSDK RAS"
                            })
                            .Create();

            _mainViewModel = vm;

            var filter = _mainViewModel.WhenValueChanged(t => t.Search)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(RegionFilter);

            _serverListLoader = _manager.RegionsList.Connect()
                .Filter(x => x is IRegion || x is IBestAvailable)
                .Filter(filter)
                .Sort(LocationsComparer.Ascending(r =>
                {
                    if (r is IRegion region)
                    {
                        return region.Country;
                    }
                    return r.CountryCode;
                }))
                .ObserveOnDispatcher()
                .Bind(Destinations)
                .Subscribe();

            _manager.WhenUserLocationChanged.Subscribe(info =>
            {
                switch (info.Status)
                {
                    case PositionInfoStatus.Updating:
                        CurrentLocation = "Retrieving...";
                        ExternalIPAddress = "Retrieving...";
                        break;

                    case PositionInfoStatus.Updated:
                        CurrentLocation = $"{info.City}, {info.Country}";
                        ExternalIPAddress = info.IPAddress.ToString();
                        break;
                }
            });
        }

        /// <summary>
        /// Gets a value that represents the list of ILocation available to the client
        /// </summary>
        /// <value>list of available ILocation</value>
        public IObservableCollection<ILocation> Destinations { get; } = new ObservableCollectionExtended<ILocation>();

        /// <summary>
        /// Gets a value indicating whether OpenVPN protocol is available to use.
        /// </summary>
        public bool IsOpenVpnAvailable
        {
            get
            {
                return _manager.GetAvailableVpnTypes().Any(x => x.Key == NetworkConnectionType.OpenVPN && x.Value == true);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is logged into the API
        /// </summary>
        /// <value>true or false</value>
        public bool IsLoggedIn
        {
            get
            {
                return _isLoggedIn;
            }

            set
            {
                if (value && _isLoggedIn != true)
                {
                    // Subscribing to the observable that is responsible for broadcasting server list refresh events.
                    // This tells to SDK to start refreshing server list periodically.
                    _serverListRefresh = _manager.WhenLocationListChanged
                        .SubscribeOn(ThreadPoolScheduler.Instance)
                        .Subscribe(OnLocationListRefresh);
                }
                else if (!value && _isLoggedIn)
                {
                    // Unsubscribing from the observable that is responsible for broadcasting server list refresh events.
                    // That stops the server list refresh on the SDK side
                    _serverListRefresh.Dispose();
                }

                SetProperty(ref _isLoggedIn, value);
                RunOnDisplayThread(
                    () =>
                    {
                        CommandManager.InvalidateRequerySuggested();
                    });
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user connected to a VPN
        /// </summary>
        /// <value>true or false</value>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            set
            {
                SetProperty(ref _isConnected, value);
                RunOnDisplayThread(CommandManager.InvalidateRequerySuggested);
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the if the current user connection state
        /// </summary>
        /// <value>string</value>
        public string ConnectionState
        {
            get { return _connectionState; }
            set { SetProperty(ref _connectionState, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the system is transitioning states
        /// </summary>
        /// <value>true or false</value>
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the system is connecting to a VPN
        /// </summary>
        /// <value>true or false</value>
        public bool IsConnecting
        {
            get { return _isConnecting; }
            set { SetProperty(ref _isConnecting, value); }
        }

        /// <summary>
        /// Gets a value that represents the list of IP addresses for all of the addapters
        /// </summary>
        /// <value>an observable collection of strings that are the ip addresses</value>
        public SmartCollection<IPAddress> IPAddresses { get; private set; } = new SmartCollection<IPAddress>();

        /// <summary>
        /// Gets or sets a value indicating the current external ip address
        /// </summary>
        /// <value>ip address that this machine currently presents to the outside world</value>
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
        /// Gets or sets a value indicating whether the application should shut down the networks if the VPN stops
        /// </summary>
        /// <value>true or false</value>
        public bool KillSwitch
        {
            get
            {
                return _killSwitch;
            }

            set
            {
                if (_manager != null)
                {
                    _manager.AllowOnlyVPNConnectivity = value;
                    _manager.AllowLANTraffic = value;
                }

                SetProperty(ref _killSwitch, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should enable Scramble
        /// </summary>
        /// <value>true or false</value>
        public bool OpenVpnScramble
        {
            get { return _openVpnScramble; }
            set { SetProperty(ref _openVpnScramble, value); }
        }

        /// <summary>
        /// Gets a list of the available protocols for OpenVPN
        /// </summary>
        public List<NetworkProtocolType> AvailableOpenVpnProtocols => Enum.GetValues(typeof(NetworkProtocolType)).Cast<NetworkProtocolType>().ToList();

        /// <summary>
        /// Gets or sets a value indicating what connection protocol the application should use.
        /// </summary>
        public NetworkConnectionType ConnectionProtocol
        {
            get { return _connectionProtocol; }
            set { SetProperty(ref _connectionProtocol, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating what connection protocol the application should use.
        /// </summary>
        public NetworkProtocolType SelectedOpenVpnProtocol
        {
            get { return _selectedOpenVpnProtocol; }
            set { SetProperty(ref _selectedOpenVpnProtocol, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the start time of the connection to the VPN
        /// </summary>
        /// <value>DateTime</value>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }

            set
            {
                SetProperty(ref _startTime, value);
            }
        }

        /// <summary>
        /// Gets or sets a value that represents if the end time of the VPN connection
        /// </summary>
        /// <value>DateTime</value>
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }

            set
            {
                SetProperty(ref _endTime, value);
                OnPropertyChanged("TimeElapsed");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the elapsed time of the last/current VPN connection
        /// </summary>
        /// <value>true or false</value>
        public TimeSpan TimeElapsed
        {
            get { return EndTime.Subtract(StartTime); }
        }

        /// <summary>
        /// OnLocationListRefresh, indicates an error occurred refreshing the list of destinations/locations
        /// </summary>
        /// <param name="message"><see cref="RefreshLocationListMessage"/></param>
        public void OnLocationListRefresh(RefreshLocationListMessage message)
        {
            if (message.Status == RefreshLocationListStatus.Refreshed && IsBusy == true)
            {
                IsBusy = false;
            }

            if (message.Status == RefreshLocationListStatus.Error)
            {
                MessageBox.Show(message.Exception.Message, "Exception in server list refresh");
                if (message.Exception is VpnSDKOAuthException || message.Exception is VpnSDKNotAuthorizedException)
                {
                    Logout();
                }
            }
        }

        /// <summary>
        /// Log the user into the API
        /// </summary>
        /// <param name="user">the user name to use</param>
        /// <param name="pswd">the password to use</param>
        public void Login(string user, string pswd)
        {
            IsBusy = true;

            _manager.Login(user, pswd)
                .Do(e => Debug.WriteLine($"Authentication process: {e}"))
                .StartWith(AuthenticationStatus.Authenticating)
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Subscribe(
                    async o =>
                    {
                        if (o == AuthenticationStatus.Authenticated)
                        {
                            IsLoggedIn = true;
                            await UpdateInformation(ConnectionStatus.Disconnected);
                        }
                    },
                    exception =>
                    {
                        IsLoggedIn = false;
                        IsBusy = false;
                        MessageBox.Show(exception.Message, "Exception in login.");
                    });
        }

        /// <summary>
        /// Log the user out of the API
        /// </summary>
        public void Logout()
        {
            _manager.Logout()
                .SubscribeOn(Scheduler.Default)
                .ObserveOnDispatcher()
                .Finally(() => IsBusy = false)
                .Subscribe(o =>
                {
                    IsLoggedIn = o != AuthenticationStatus.NotAuthenticated;
                });
        }

        /// <summary>
        /// Connect the user via the selected location to the VPN.
        /// </summary>
        /// <param name="location">the server to conenct to</param>
        public void Connect(ILocation location)
        {
            IConnectionConfiguration configuration = null;

            if (ConnectionProtocol == NetworkConnectionType.OpenVPN)
            {
                var builder = new OpenVpnConnectionConfigurationBuilder()
                    .SetScramble(OpenVpnScramble)
                    .SetNetworkProtocol(SelectedOpenVpnProtocol);

                // We currently support AES-128-CBC cipher with scramble enabled.
                if (OpenVpnScramble)
                {
                    builder.SetCipher(OpenVpnCipherType.AES_128_CBC);
                }
                else
                {
                    builder.SetCipher(OpenVpnCipherType.AES_256_CBC);
                }

                configuration = builder.Build();
            }
            else
            {
                configuration = new RasConnectionConfigurationBuilder()
                    .SetConnectionType(ConnectionProtocol)
                    .Build();
            }

            _manager.Connect(location, configuration)
                .Do(e => Debug.WriteLine($"Connection Process: {e}"))
                .Subscribe(
                    async status =>
                    {
                        switch (status)
                        {
                            case ConnectionStatus.Connecting:
                                IsConnecting = true;
                                IsConnected = false;
                                IsBusy = true;
                                break;

                            case ConnectionStatus.Connected:
                                StartTiming();
                                IsConnecting = false;
                                IsConnected = true;
                                IsBusy = false;
                                await UpdateInformation(status);
                                break;

                            default:
                                IsConnecting = false;
                                IsConnected = false;
                                IsBusy = false;
                                break;
                        }
                    },
                    exception =>
                    {
                        // exceptions will show up here.
                        MessageBox.Show(exception.Message, "Exception in Connect");

                        IsConnecting = false;
                        IsConnected = false;
                        IsBusy = false;
                        ConnectionState = ConnectionStatus.Failed.ToString();
                    });
        }

        /// <summary>
        /// cancel the connection that is currently being made.
        /// </summary>
        /// <returns>Task</returns>
        public async Task Cancel()
        {
            _manager.CancelConnectionProcess();
            await UpdateInformation(ConnectionStatus.Cancelled);
        }

        /// <summary>
        /// Disconnect the currently connected VPN session
        /// </summary>
        public void Disconnect()
        {
            _manager.Disconnect()
                .Do(e => Debug.WriteLine($"Connection Process: {e}"))
                .Subscribe(async status =>
                    {
                        switch (status)
                        {
                            case ConnectionStatus.Disconnecting:
                                IsBusy = true;
                                break;

                            case ConnectionStatus.Disconnected:
                                StopTimer();
                                IsBusy = false;
                                IsConnected = false;
                                break;
                        }
                        await UpdateInformation(status);
                    });
        }

        /// <summary>
        /// Install OpenVPN TAP driver.
        /// </summary>
        public void InstallTapDriver()
        {
            IsBusy = true;
            _manager.InstallTapDriver()
                .Finally(() => { IsBusy = false; })
                .Subscribe(result =>
                    {
                        if (result)
                        {
                            MessageBox.Show("TAP driver has been successfully installed.");
                        }
                        else
                        {
                            MessageBox.Show("Couldn't install TAP driver.");
                        }
                    });
        }

        /// <summary>
        /// Updates the Connection State, List of Network Adapters and the IP Address for this machine.
        /// </summary>
        /// <param name="status">The new connection state</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task UpdateInformation(ConnectionStatus status)
        {
            ConnectionState = status.ToString();
            NetworkAdapters.Clear();
            await Task.Delay(1000);
            NetworkAdapters.Repopulate(IPHelpers.GetNetworkAdapters());
            IPAddresses.Repopulate(IPHelpers.GetLocalIPAddresses());
        }

        /// <summary>
        /// The Region Filter is used to filter the set of ILocations shown in the Destinations list.
        /// </summary>
        /// <param name="searchText">The current filter text</param>
        /// <returns>the Func used to perform the filter</returns>
        private Func<ILocation, bool> RegionFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return location => true;
            }

            return location =>
            {
                if (location is IBestAvailable bestAvailable)
                {
                    return bestAvailable.SearchName.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
                           || bestAvailable.SearchName.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
                }

                return ((IRegion)location).Country.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
                       || ((IRegion)location).City.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
            };
        }

        /// <summary>
        /// Updates the current time for the VPN connection.
        /// </summary>
        private void UpdateTime()
        {
            EndTime = DateTime.Now;
        }

        /// <summary>
        /// Initiates the timing of the VPN connection
        /// </summary>
        private void StartTiming()
        {
            if (!_timer.IsEnabled)
            {
                StartTime = DateTime.Now;
                EndTime = DateTime.Now;
                _timer.Tick += Timer_Tick;
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Start();
            }
        }

        /// <summary>
        /// Tick from the connection timer
        /// </summary>
        /// <param name="sender">_timer</param>
        /// <param name="e">the normal tick event args</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        /// <summary>
        /// Stop the connection timer
        /// </summary>
        private void StopTimer()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
            }
        }
    }
}