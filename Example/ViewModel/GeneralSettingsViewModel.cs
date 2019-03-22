// <copyright file="GeneralSettingsViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using Example.Helpers;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// GeneralSettingsViewModel
    /// </summary>
    public class GeneralSettingsViewModel : BindableBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralSettingsViewModel"/> class.
        /// </summary>
        public GeneralSettingsViewModel()
        {
            SDK.AuthenticationStatusChanged += OnAuthenticationStatusChanged;
        }

        /// <summary>
        /// Gets SDK Instance.
        /// </summary>
        public ISDK SDK => SDKFactory.GetInstance();

        /// <summary>
        /// Gets or sets a value indicating whether the application should shut down the networks if the VPN stops
        /// </summary>
        /// <value>true or false</value>
        public bool KillSwitch
        {
            get => Properties.Settings.Default.KillSwitch;

            set
            {
                SDK.AllowOnlyVPNConnectivity = value;
                AllowLANTraffic = value;
                Properties.Settings.Default.KillSwitch = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should shut down the networks if the VPN stops
        /// </summary>
        /// <value>true or false</value>
        public bool AllowLANTraffic
        {
            get => Properties.Settings.Default.AllowLANTraffic;

            set
            {
                SDK.AllowLANTraffic = value;
                Properties.Settings.Default.AllowLANTraffic = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the VPN connection should allow DNS to leak.
        /// </summary>
        /// <value>true or false</value>
        public bool DisableDNSLeakProtection
        {
            get => Properties.Settings.Default.DisableDNSLeakProtection;

            set
            {
                SDK.DisableDNSLeakProtection = value;
                Properties.Settings.Default.DisableDNSLeakProtection = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the VPN connection should allow IPv6 to leak.
        /// </summary>
        /// <value>true or false</value>
        public bool DisableIPv6LeakProtection
        {
            get => Properties.Settings.Default.DisableIPv6LeakProtection;

            set
            {
                SDK.DisableIPv6LeakProtection = value;
                Properties.Settings.Default.DisableIPv6LeakProtection = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        private void OnAuthenticationStatusChanged(ISDK sender, AuthenticationStatus status)
        {
            // Restoring KillSwitch options after login
            if (status == AuthenticationStatus.Authenticated && Properties.Settings.Default.KillSwitch)
            {
                SDK.AllowOnlyVPNConnectivity = true;
            }

            // Disabling KillSwitch on logout
            if (status == AuthenticationStatus.NotAuthenticated)
            {
                SDK.AllowOnlyVPNConnectivity = false;
            }
        }
    }
}