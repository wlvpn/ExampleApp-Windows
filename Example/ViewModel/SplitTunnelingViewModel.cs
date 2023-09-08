// <copyright file="SplitTunnelingViewModel.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Example.Helpers;
using VpnSDK.Common.Enums;
using VpnSDK.Common.Settings;
using VpnSDK.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// SplitTunnelingViewModel
    /// </summary>
    public class SplitTunnelingViewModel : BindableBase
    {
        private const string SupportedExtension = ".exe";
        private const string UnsupportedApp = "Example";

        private ICommand _addDomainCommand;
        private ICommand _pickManuallyCommand;
        private ICommand _deleteDomainCommand;
        private ICommand _deleteApplicationCommand;
        private ICommand _includeSubDomainsCommand;
        private ICommand _excludeSubDomainsCommand;
        private string _domainName;
        private string _validationError;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitTunnelingViewModel"/> class.
        /// </summary>
        public SplitTunnelingViewModel()
        {
            SDK.SplitTunnelMode = SplitTunnelMode.RouteSelectedTrafficOutsideVpn;
            SDK.SplitTunnelAllowedApps = new List<SplitTunnelApp>();
            SDK.SplitTunnelAllowedDomains = new List<SplitTunnelDomain>();
            Applications = new ObservableCollection<SplitTunnelApp>(SDK.SplitTunnelAllowedApps);
            Domains = new ObservableCollection<SplitTunnelDomain>(SDK.SplitTunnelAllowedDomains);
        }

        /// <summary>
        /// Gets or sets a value indicating whether split tunneling feature is on or off.
        /// </summary>
        public bool IsSplitTunnelingOn
        {
            get => SDK.IsSplitTunnelEnabled;
            set
            {
                SDK.IsSplitTunnelEnabled = value;
            }
        }

        /// <summary>
        /// Gets SDK Instance.
        /// </summary>
        public ISDK SDK => SDKFactory.GetInstance();

        /// <summary>
        /// Gets or sets Domain name.
        /// </summary>
        public string DomainName { get => _domainName; set => SetProperty(ref _domainName, value); }

        /// <summary>
        /// Gets or sets error message while adding new domain.
        /// </summary>
        public string ValidationError { get => _validationError; set => SetProperty(ref _validationError, value); }

        /// <summary>
        /// Gets or sets domain list.
        /// </summary>
        public ObservableCollection<SplitTunnelDomain> Domains { get; set; }

        /// <summary>
        /// Gets or sets application list.
        /// </summary>
        public ObservableCollection<SplitTunnelApp> Applications { get; set; }

        /// <summary>
        /// Gets a value that represents the command to execute when the user add domains to split tunneling.
        /// </summary>
        public ICommand AddDomainCommand => _addDomainCommand ?? (_addDomainCommand = new RelayCommand(
                   execute: (p) => AddDomain()));

        /// <summary>
        /// Gets a value that represents the command to execute when the user add domains to split tunneling.
        /// </summary>
        public ICommand PickManuallyCommand => _pickManuallyCommand ?? (_pickManuallyCommand = new RelayCommand(
                   execute: (p) => PickManually()));

        /// <summary>
        /// Gets a value that represents the command to execute when the user delete domain from split tunneling.
        /// </summary>
        public ICommand DeleteDomainCommand => _deleteDomainCommand ?? (_deleteDomainCommand = new RelayCommand(
                   execute: (p) => DeleteDomain(p as SplitTunnelDomain)));

        /// <summary>
        /// Gets a value that represents the command to execute when the user delete domain from split tunneling.
        /// </summary>
        public ICommand DeleteApplicationCommand => _deleteApplicationCommand ?? (_deleteApplicationCommand = new RelayCommand(
                   execute: (p) => DeleteApplication(p as SplitTunnelApp)));

        /// <summary>
        /// Gets a value that represents the command to execute when the user include all subdomains for given domain in ST filter.
        /// </summary>
        public ICommand IncludeSubDomainsCommand => _includeSubDomainsCommand ?? (_includeSubDomainsCommand = new RelayCommand(
                   execute: (p) => UpdateIncludeSubDomains(true, p as SplitTunnelDomain)));

        /// <summary>
        /// Gets a value that represents the command to execute when the user exclude all subdomains for given domain in ST filter.
        /// </summary>
        public ICommand ExcludeSubDomainsCommand => _excludeSubDomainsCommand ?? (_excludeSubDomainsCommand = new RelayCommand(
                   execute: (p) => UpdateIncludeSubDomains(false, p as SplitTunnelDomain)));

        /// <summary>
        /// Deletes application from split tunneling filter
        /// </summary>
        /// <param name="application">Application that need to be removed from split tunneling.</param>
        public void DeleteApplication(SplitTunnelApp application)
        {
            try
            {
                Applications.Remove(application);
                SDK.SplitTunnelAllowedApps.Remove(application);
            }
            catch (Exception)
            {
                // ignore.
            }
        }

        /// <summary>
        /// Deletes domain from split tunneling filter
        /// </summary>
        /// <param name="domain">Domain that need to be removed from split tunneling.</param>
        public void DeleteDomain(SplitTunnelDomain domain)
        {
            try
            {
                Domains.Remove(domain);
                SDK.SplitTunnelAllowedDomains.Remove(domain);
            }
            catch (Exception)
            {
                // ignore.
            }
        }

        /// <summary>
        /// Updates include subdomain property for domain added to split tunneling.
        /// </summary>
        /// <param name="includeAllSubDomains">bool value determining whether to include subdomains in split tunneling filter or not.</param>
        /// <param name="domain">domain that needs to update includesubdomain property.</param>
        public void UpdateIncludeSubDomains(bool includeAllSubDomains, SplitTunnelDomain domain)
        {
            domain.IncludeAllSubdomains = includeAllSubDomains;
        }

        /// <summary>
        /// Adds new domain to split tunneling filter.
        /// </summary>
        public void AddDomain()
        {
            try
            {
                ValidationError = string.Empty;
                var domain = new SplitTunnelDomain(DomainName);
                if (Domains.Any(d => d.DomainName.Equals(domain.DomainName, StringComparison.OrdinalIgnoreCase)))
                {
                    ValidationError = "This domain already exists, please enter a different domain.";
                    return;
                }

                Domains.Add(domain);
                SDK.SplitTunnelAllowedDomains.Add(domain);
                DomainName = string.Empty;
            }
            catch (Exception ex)
            {
                ValidationError = ex.Message;
            }
        }

        /// <summary>
        /// Opens up file picker dialog from which user can select application.
        /// </summary>
        public void PickManually()
        {
            var filter = string.Join("|", SupportedExtension
                                  .Select(_ => $"Executable (*{SupportedExtension})|*{SupportedExtension}"));

            // Create OpenFileDialog
            OpenFileDialog openFileDlg = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                DefaultExt = SupportedExtension,
                Filter = filter,
                CheckFileExists = true,
                Title = "Add Applications",
                CheckPathExists = true,
                Multiselect = true,
            };
            openFileDlg.FileOk += OpenFileDlg_FileOk;

            openFileDlg.ShowDialog();
        }

        private void OpenFileDlg_FileOk(object sender, CancelEventArgs e)
        {
            if (sender is OpenFileDialog openFileDlg)
            {
                foreach (var filePath in openFileDlg.FileNames)
                {
                    var extension = Path.GetExtension(filePath);
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (!SupportedExtension.Equals(extension, System.StringComparison.OrdinalIgnoreCase) ||
                        UnsupportedApp.Equals(fileName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        var message = string.Format("Unsupported file chosen: {0}", fileName);
                        System.Windows.MessageBox.Show(message, "Invalid Application", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        continue;
                    }

                    if (Applications.Any(app => app.Path == filePath))
                    {
                        var message = string.Format("Application already added: {0}", fileName);
                        System.Windows.MessageBox.Show(message, "Duplicate Application", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        continue;
                    }

                    SplitTunnelApp splitTunnelApp = new SplitTunnelApp(fileName, filePath);
                    Applications.Add(splitTunnelApp);
                    SDK.SplitTunnelAllowedApps.Add(splitTunnelApp);
                }
            }
        }
    }
}
