// <copyright file="ConfigurableBooleanToVisibilityConverter.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Example.Converters
{
    /// <summary>
    /// ConfigurableBooleanToVisibilityConverter
    /// </summary>
    public class ConfigurableBooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Gets or sets the WhenTrue value for the converter.
        /// </summary>
        /// <value>a Visibility</value>
        [DefaultValue(Visibility.Visible)]
        public Visibility WhenTrue { get; set; }

        /// <summary>
        /// Gets or sets the WhenFalse value for the converter.
        /// </summary>
        /// <value>a Visibility</value>
        [DefaultValue(Visibility.Collapsed)]
        public Visibility WhenFalse { get; set; }

        /// <summary>
        /// ProvideValue
        /// </summary>
        /// <param name="serviceProvider">Converter method called by WPF to convert a value</param>
        /// <returns>object</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <summary>
        /// Convert a boolean into to a Visibility.  The conversion is defined by the WhenTrue and WhenFalse properties.
        /// </summary>
        /// <param name="value">A boolean to convert to a visibility</param>
        /// <param name="targetType">The type that the converter returns</param>
        /// <param name="parameter">Any other input to the convert process</param>
        /// <param name="culture">what language is being used</param>
        /// <returns>a visbility based on the WhenTrue and WhenFalse properties</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? WhenTrue : WhenFalse;
            }

            throw new InvalidOperationException("value is not boolean");
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">a Visability</param>
        /// <param name="targetType">Type to convert to</param>
        /// <param name="parameter">Any other inputs for the conversion</param>
        /// <param name="culture">Language used</param>
        /// <returns>This currently will always throw an exception</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
