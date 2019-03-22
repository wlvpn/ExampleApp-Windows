// <copyright file="App.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Windows;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Formatting.Json;

namespace Example
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private LoggingLevelSwitch _dynamicLevelSwitch = new LoggingLevelSwitch();

        /// <inheritdoc/>
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(_dynamicLevelSwitch)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}")
                .WriteTo.File("log.txt", outputTemplate: "{Timestamp:HH:mm} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}")
                .CreateLogger();

            _dynamicLevelSwitch.MinimumLevel = Example.Properties.Settings.Default.LoggingLevel;
            Example.Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;
            base.OnStartup(e);
        }

        /// <inheritdoc/>
        protected override void OnExit(ExitEventArgs e)
        {
            if (!SDKFactory.GetInstance().IsDisposed)
            {
                SDKFactory.GetInstance().Dispose();
            }
        }

        private void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("LoggingLevel"))
            {
                _dynamicLevelSwitch.MinimumLevel = Example.Properties.Settings.Default.LoggingLevel;
                Log.Information("Logging Level Changed to " + _dynamicLevelSwitch.MinimumLevel);
                Example.Properties.Settings.Default.Save();
            }
        }
    }
}