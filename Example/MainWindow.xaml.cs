// <copyright file="MainWindow.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Windows;
using Example.ViewModel;

namespace Example
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
        }

        /// <summary>
        /// Handles the on Closed event for the window.  And tells the View model about it.
        /// </summary>
        /// <param name="e">the normal OnClosed event</param>
        protected override void OnClosed(EventArgs e)
        {
            ((MainViewModel)DataContext).DisConnectCmd.Execute(null);
            base.OnClosed(e);
        }
    }
}
