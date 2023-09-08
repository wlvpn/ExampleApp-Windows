// <copyright file="MainViewModel.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Example.Helpers;
using Example.Views;

using VpnSDK;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

using MessageBox = System.Windows.MessageBox;

namespace Example.ViewModel
{
    /// <summary>
    /// MainViewModel
    /// </summary>
    public class MainViewModel : BindableBase
    {
        private bool _isBusy = false;
        private bool _isLoggedIn = false;
        private ICommand _logoutCommand = null;
        private IAsyncCommand _connectCommand = null;
        private IAsyncCommand _disconnectCommand = null;
        private ICommand _cancelConnectionCommand = null;
        private CancellationTokenSource _connectionTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            InitViews();

            SDK.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
            SDK.AuthenticationStatusChanged += OnAuthenticationStatusChanged;
            SDK.TapDeviceInstallationStatusChanged += (sender, status) =>
            {
                IsBusy = status == OperationStatus.InProgress;
            };
        }

        /// <summary>
        /// Gets a value that represents the list of views available to the main window
        /// </summary>
        /// <value>list of available views</value>
        public SmartCollection<ViewDefinition> AvailableViews { get; private set; } = new SmartCollection<ViewDefinition>();

        /// <summary>
        /// Gets or sets a value indicating whether the progress Window is shown.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether user is logged in.
        /// </summary>
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set { SetProperty(ref _isLoggedIn, value); }
        }

        /// <summary>
        /// Gets SDK Instance.
        /// </summary>
        public ISDK SDK => SDKFactory.GetInstance();

        /// <summary>
        /// Gets a value that represents the command to execute when the user logs out.
        /// </summary>
        /// <value>the Command</value>
        public ICommand Logout => _logoutCommand ?? (_logoutCommand = new RelayCommand(
                    execute: (p) => SDK.Logout()));

        /// <summary>
        /// Gets a value that represents the command to execute when the user wants to connect to a server
        /// </summary>
        /// <value>the Command</value>
        public IAsyncCommand Connect => _connectCommand ?? (_connectCommand = new AsyncCommand(
                    execute: () => ConnectTask(),
                    canExecute: () => Properties.Settings.Default.SelectedDestination != null && !SDK.IsConnected && !SDK.IsConnecting && !IsBusy));

        /// <summary>
        /// Gets a value that represents the command to execute when the user wants to disconnect from a server
        /// </summary>
        /// <value>the Command</value>
        public IAsyncCommand Disconnect => _disconnectCommand ?? (_disconnectCommand = new AsyncCommand(
                    execute: () => SDK.Disconnect(),
                    canExecute: () => SDK.IsConnected));

        /// <summary>
        /// Gets a value that represents the command to execute when the cancels a connection attempt
        /// </summary>
        /// <value>the Command</value>
        public ICommand CancelConnectionCommand => _cancelConnectionCommand ?? (_cancelConnectionCommand = new RelayCommand(
                    execute: (parm) => { _connectionTokenSource?.Cancel(); },
                    canExecute: (parm) => SDK.IsConnecting));

        /// <summary>
        /// This is code that will create the navigation objects used by the main window.
        /// </summary>
        private void InitViews()
        {
            AvailableViews.Add(new ViewDefinition
            {
                DisplayName = "Destinations",
                View = new DestinationsView(),
                IsSelected = true
            });
            AvailableViews.Add(new ViewDefinition
            {
                DisplayName = "General Settings",
                View = new GeneralSettingsView()
            });
            AvailableViews.Add(new ViewDefinition
            {
                DisplayName = "Connection Settings",
                View = new ConnectionSettingsView()
            });
            AvailableViews.Add(new ViewDefinition
            {
                DisplayName = "Split Tunneling",
                View = new SplitTunnelingView()
            });
            AvailableViews.Add(new ViewDefinition
            {
                DisplayName = "Information",
                View = new InformationView()
            });
        }

        private async Task ConnectTask()
        {
            List<IConnectionConfiguration> configurations = new List<IConnectionConfiguration>();

            // Order is important for Automatic Protocol feature. It starts with first configuration and then uses the next one as a fallback.
            // Lets stick with the next protocols order for now: WG, OpenVPN, IKEv2.
            configurations.Add(new WireGuardConnectionConfigurationBuilder().SetBlockUntunneledTraffic(!Properties.Settings.Default.AllowLanInterfaces).Build());
            configurations.Add(new RasConnectionConfigurationBuilder().SetConnectionType(NetworkConnectionType.IKEv2).Build());
            configurations.Add(
                new OpenVpnConnectionConfigurationBuilder()
                    .SetCipher(Properties.Settings.Default.OpenVpnScramble ? OpenVpnCipherType.AES_128_CBC : OpenVpnCipherType.AES_256_CBC)
                    .SetScramble(Properties.Settings.Default.OpenVpnScramble)
                    .SetNetworkProtocol(Properties.Settings.Default.OpenVpnProtocol)
                    .Build());

            try
            {
                _connectionTokenSource?.Dispose();
                _connectionTokenSource = new CancellationTokenSource();

                // If the network connection type is Unspecified, this means that the Automatic Protocol feature is selected.
                if (Properties.Settings.Default.ConnectionProtocol == NetworkConnectionType.Automatic)
                {
                    await SDK.Connect(Properties.Settings.Default.SelectedDestination, configurations, _connectionTokenSource.Token);
                }
                else
                {
                    // Else, connect to a VPN server using a specific Network connection type.
                    await SDK.Connect(Properties.Settings.Default.SelectedDestination, configurations.First(x => x.ConnectionType == Properties.Settings.Default.ConnectionProtocol), _connectionTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing. Finally block will handle it.
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "VPN Connection Failed");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnAuthenticationStatusChanged(ISDK sender, AuthenticationStatus status)
        {
            IsBusy = status == AuthenticationStatus.InProgress;
            IsLoggedIn = status == AuthenticationStatus.Authenticated;

            if (IsLoggedIn)
            {
                // Setting up SDK filtering settings once logged in.
                SDK.AllowOnlyVPNConnectivity = Properties.Settings.Default.KillSwitch;
                SDK.AllowLANTraffic = Properties.Settings.Default.AllowLANTraffic;
                SDK.AllowLocalAdaptersWhenConnected = Properties.Settings.Default.AllowLanInterfaces;
                SDK.DisableDNSLeakProtection = Properties.Settings.Default.DisableIPv6LeakProtection;
                SDK.DisableIPv6LeakProtection = Properties.Settings.Default.DisableIPv6LeakProtection;
            }
        }

        private void OnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            IsBusy = current == ConnectionStatus.Connecting || current == ConnectionStatus.Disconnecting;

            if (previous == ConnectionStatus.Connected && current == ConnectionStatus.Disconnected)
            {
                MessageBox.Show("VPN connection unexpectedly terminated.", "VPN Connection Error");
            }
        }
    }
}