// <copyright file="MainViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using Example.Helpers;
using Example.Views;
using VpnSDK.Public.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// MainViewModel
    /// </summary>
    public class MainViewModel : BindableBase
    {
        private string _userID = string.Empty;
        private string _pswd = string.Empty;
        private string _search = string.Empty;
        private ILocation _selServer = null;
        private RelayCommand _logInCmd = null;
        private RelayCommand _logOutCmd = null;
        private RelayCommand _connectCmd = null;
        private RelayCommand _disConnectCmd = null;
        private RelayCommand _cancelConnectionCmd = null;
        private RelayCommand _installTapDriverCmd = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            SDKVM = new SDKViewModel(this);
            InitViews();
        }

        /// <summary>
        /// Gets a value that represents the SDK View Model instance
        /// </summary>
        /// <value>The SDK View Model.</value>
        public SDKViewModel SDKVM { get; private set; }

        /// <summary>
        /// Gets a value that represents the list of views available to the main window
        /// </summary>
        /// <value>list of available views</value>
        public SmartCollection<ViewDefinition> AvailableViews { get; private set; } = new SmartCollection<ViewDefinition>();

        /// <summary>
        /// Gets or sets a value that represents the userid for the current user
        /// </summary>
        /// <value>the user id</value>
        public string UserID
        {
            get { return _userID; }
            set { SetProperty(ref _userID, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the password for the current user
        /// </summary>
        /// <value>the password</value>
        public string Pswd
        {
            get { return _pswd; }
            set { SetProperty(ref _pswd, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the current filter value for the search of the servers.
        /// </summary>
        /// <value>the search string</value>
        public string Search
        {
            get { return _search; }
            set { SetProperty(ref _search, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the currently selected location/destination for the applciation.  This is what the outside world should see as
        /// your location. The GeoIP used by the external world may sometimes not recognize it properly.  But it is what we aer connecting to.
        /// </summary>
        /// <value>the currently selected destination</value>
        public ILocation SelectedDestination
        {
            get
            {
                return _selServer;
            }

            set
            {
                SetProperty(ref _selServer, value);
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the user logs in.
        /// </summary>
        /// <value>the Command</value>
        public RelayCommand LogInCmd
        {
            get
            {
                if (_logInCmd == null)
                {
                    _logInCmd = new RelayCommand(
                    (parm) =>
                    {
                        SDKVM.Login(UserID, Pswd);
                    },
                    (parm) => !SDKVM.IsLoggedIn);
                }

                return _logInCmd;
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the user logs out.
        /// </summary>
        /// <value>the Command</value>
        public RelayCommand LogOutCmd
        {
            get
            {
                if (_logOutCmd == null)
                {
                    _logOutCmd = new RelayCommand(
                    (parm) =>
                    {
                        SelectedDestination = null;
                        SDKVM.Logout();
                    },
                    (parm) =>
                    SDKVM.IsLoggedIn && !SDKVM.IsConnected);
                }

                return _logOutCmd;
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the user wants to connect to a server
        /// </summary>
        /// <value>the Command</value>
        public RelayCommand ConnectCmd
        {
            get
            {
                if (_connectCmd == null)
                {
                    _connectCmd = new RelayCommand(
                    (parm) =>
                    {
                        SDKVM.IsConnecting = true;
                        SDKVM.Connect(SelectedDestination);
                    },
                    (parm) => SelectedDestination != null && !SDKVM.IsConnected && !SDKVM.IsConnecting);
                }

                return _connectCmd;
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the user wants to disconnect from a server
        /// </summary>
        /// <value>the Command</value>
        public RelayCommand DisConnectCmd
        {
            get
            {
                if (_disConnectCmd == null)
                {
                    _disConnectCmd = new RelayCommand(
                    (parm) =>
                    {
                        SDKVM.Disconnect();
                    },
                    (parm) => SDKVM.IsConnected);
                }

                return _disConnectCmd;
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the cancels a connectin attempt
        /// </summary>
        /// <value>the Command</value>
        public RelayCommand CancelConnectionCmd
        {
            get
            {
                if (_cancelConnectionCmd == null)
                {
                    _cancelConnectionCmd = new RelayCommand(
                        async (parm) =>
                        {
                            // tbd
                            await SDKVM.Cancel();
                        },
                        (parm) => !SDKVM.IsConnected);
                }

                return _cancelConnectionCmd;
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the user wants to install or reinstall TAP adapter.
        /// </summary>
        /// <value>the Command</value>
        public RelayCommand InstallTapDriverCmd
        {
            get
            {
                if (_installTapDriverCmd == null)
                {
                    _installTapDriverCmd = new RelayCommand(
                    (parm) =>
                    {
                        SDKVM.InstallTapDriver();
                    }, (parm) => true);
                }

                return _installTapDriverCmd;
            }
        }

        /// <summary>
        /// This is code that will create the navigation objects used by the main window.
        /// </summary>
        private void InitViews()
        {
            AvailableViews.Add(new ViewDefinition { DisplayName = "Destinations", View = new DestinationsView(), IsSelected = true });
            AvailableViews.Add(new ViewDefinition { DisplayName = "General Settings", View = new GeneralSettingsView(), IsSelected = false });
            AvailableViews.Add(new ViewDefinition { DisplayName = "Connections", View = new ConnectionView(), IsSelected = false });
            AvailableViews.Add(new ViewDefinition { DisplayName = "Information", View = new InformationView(), IsSelected = false });
        }
    }
}