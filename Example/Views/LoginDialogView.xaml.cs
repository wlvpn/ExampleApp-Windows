// <copyright file="LoginDialogView.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;
using Example.ViewModel;

namespace Example.Views
{
    /// <summary>
    /// Interaction logic for LoginDialogView.xaml
    /// </summary>
    public partial class LoginDialogView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginDialogView"/> class.
        /// </summary>
        public LoginDialogView()
        {
            DataContext = new LoginDialogViewModel();
            InitializeComponent();
        }
    }
}
