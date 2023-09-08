// <copyright file="SplitTunnelingView.xaml.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;
using Example.ViewModel;

namespace Example.Views
{
    /// <summary>
    /// Interaction logic for SplitTunnelingView.xaml
    /// </summary>
    public partial class SplitTunnelingView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplitTunnelingView"/> class.
        /// </summary>
        public SplitTunnelingView()
        {
            DataContext = new SplitTunnelingViewModel();
            InitializeComponent();
        }
    }
}
