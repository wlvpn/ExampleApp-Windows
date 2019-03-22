// <copyright file="LoginDialogViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Windows;
using Example.Helpers;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// LoginDialogViewModel
    /// </summary>
    public class LoginDialogViewModel : BindableBase
    {
        private string _username = "demo";
        private string _password = "demo";
        private bool _isLoggedIn = false;
        private IAsyncCommand _loginCommand = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginDialogViewModel"/> class.
        /// </summary>
        public LoginDialogViewModel()
        {
            SDK.AuthenticationStatusChanged += OnAuthenticationStatusChanged;
        }

        /// <summary>
        /// Gets SDK Instance.
        /// </summary>
        public ISDK SDK => SDKFactory.GetInstance();

        /// <summary>
        /// Gets or sets a value that represents the username for the current user
        /// </summary>
        /// <value>the user id</value>
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the password for the current user
        /// </summary>
        /// <value>the password</value>
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
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
        /// Gets a value that represents the command to execute when the user logs in.
        /// </summary>
        /// <value>the Command</value>
        public IAsyncCommand Login => _loginCommand ?? (_loginCommand = new AsyncCommand(
                           execute: () => SDK.Login(Username, Password),
                           canExecute: () => !IsLoggedIn,
                           errorHandler: HandleError));

        /// <summary>
        /// Show the user an error
        /// </summary>
        /// <param name="error">The exception that occurred</param>
        public void HandleError(Exception error)
        {
            MessageBox.Show(error.Message, "Error");
        }

        private void OnAuthenticationStatusChanged(ISDK sender, AuthenticationStatus status)
        {
            IsLoggedIn = status == AuthenticationStatus.Authenticated;
        }
    }
}
