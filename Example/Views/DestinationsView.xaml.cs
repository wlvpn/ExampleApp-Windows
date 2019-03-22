// <copyright file="DestinationsView.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;
using Example.ViewModel;

namespace Example.Views
{
    /// <summary>
    /// Interaction logic for ServersView.xaml
    /// </summary>
    public partial class DestinationsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationsView"/> class.
        /// </summary>
        public DestinationsView()
        {
            InitializeComponent();
            DataContext = new DestinationsViewModel();
        }
    }
}
