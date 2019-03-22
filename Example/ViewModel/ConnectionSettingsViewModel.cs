// <copyright file="ConnectionSettingsViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Example.Helpers;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// GeneralSettingsViewModel
    /// </summary>
    public class ConnectionSettingsViewModel : BindableBase
    {
        private ConnectionStatus _connectionStatus;
        private IAsyncCommand _installTapDriverCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionSettingsViewModel"/> class.
        /// </summary>
        public ConnectionSettingsViewModel()
        {
            _connectionStatus = ConnectionStatus.Disconnected;
            SDK.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
        }

        /// <summary>
        /// Gets SDK Instance.
        /// </summary>
        public ISDK SDK => SDKFactory.GetInstance();

        /// <summary>
        /// Gets or sets VPN connection status.
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        /// <summary>
        /// Gets or sets a value indicating what connection protocol the application should use.
        /// </summary>
        public NetworkConnectionType ConnectionProtocol
        {
            get => Properties.Settings.Default.ConnectionProtocol;
            set
            {
                Properties.Settings.Default.ConnectionProtocol = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what Network Protocol OpenVPN should use.
        /// </summary>
        public NetworkProtocolType OpenVpnProtocol
        {
            get => Properties.Settings.Default.OpenVpnProtocol;
            set
            {
                Properties.Settings.Default.OpenVpnProtocol = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should enable Scramble
        /// </summary>
        /// <value>true or false</value>
        public bool OpenVpnScramble
        {
            get => Properties.Settings.Default.OpenVpnScramble;
            set
            {
                Properties.Settings.Default.OpenVpnScramble = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the user wants to install or reinstall TAP adapter.
        /// </summary>
        /// <value>the Command</value>
        public IAsyncCommand InstallTapDriver => _installTapDriverCommand ?? (_installTapDriverCommand = new AsyncCommand(
                    execute: async () =>
                    {
                        DriverInstallResult result = await SDK.InstallTapDriver();

                        switch (result)
                        {
                            case DriverInstallResult.Success:
                                MessageBox.Show("TAP driver was successfully installed.");
                                break;

                            case DriverInstallResult.RebootRequired:
                                MessageBox.Show("A reboot is required to finish TAP driver installation.");
                                break;

                            case DriverInstallResult.Failed:
                                MessageBox.Show("Failed to install TAP driver.");
                                break;
                        }
                    }));

        /// <summary>
        /// Gets a list of the available protocols for OpenVPN
        /// </summary>
        public List<NetworkProtocolType> AvailableOpenVpnProtocols => Enum.GetValues(typeof(NetworkProtocolType)).Cast<NetworkProtocolType>().ToList();

        private void OnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            ConnectionStatus = current;
        }
    }
}