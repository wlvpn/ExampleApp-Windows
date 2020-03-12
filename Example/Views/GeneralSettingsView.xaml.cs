// <copyright file="GeneralSettingsView.xaml.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;
using Example.ViewModel;
using VpnSDK.Interfaces;

namespace Example.Views
{
    /// <summary>
    /// Interaction logic for GeneralSettingsView.xaml
    /// </summary>
    public partial class GeneralSettingsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralSettingsView"/> class.
        /// </summary>
        public GeneralSettingsView()
        {
            DataContext = new GeneralSettingsViewModel();
            InitializeComponent();
        }
    }
}
