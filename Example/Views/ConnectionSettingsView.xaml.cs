// <copyright file="ConnectionSettingsView.xaml.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;
using Example.ViewModel;

namespace Example.Views
{
    /// <summary>
    /// Interaction logic for ConnectionSettingsView.xaml
    /// </summary>
    public partial class ConnectionSettingsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionSettingsView"/> class.
        /// </summary>
        public ConnectionSettingsView()
        {
            DataContext = new ConnectionSettingsViewModel();
            InitializeComponent();
        }
    }
}
