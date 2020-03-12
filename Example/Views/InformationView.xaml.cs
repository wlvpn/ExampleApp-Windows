// <copyright file="InformationView.xaml.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;
using Example.ViewModel;

namespace Example.Views
{
    /// <summary>
    /// Interaction logic for Information.xaml
    /// </summary>
    public partial class InformationView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InformationView"/> class.
        /// </summary>
        public InformationView()
        {
            DataContext = new InformationViewModel();
            InitializeComponent();
        }
    }
}
