// <copyright file="ConnectView.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;
using Example.ViewModel;

namespace Example.Views
{
    /// <summary>
    /// Interaction logic for ConnectView.xaml
    /// </summary>
    public partial class ConnectView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectView"/> class.
        /// </summary>
        public ConnectView()
        {
            DataContext = new ConnectViewModel();
            InitializeComponent();
        }
    }
}
