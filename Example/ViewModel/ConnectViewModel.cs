// <copyright file="ConnectViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Threading;
using Example.Helpers;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// GeneralSettingsViewModel
    /// </summary>
    public class ConnectViewModel : BindableBase
    {
        private ConnectionStatus _connectionStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectViewModel"/> class.
        /// </summary>
        public ConnectViewModel()
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

        private void OnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            ConnectionStatus = current;
        }
    }
}