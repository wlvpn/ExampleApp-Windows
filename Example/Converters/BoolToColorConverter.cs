// <copyright file="BoolToColorConverter.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Example.Converters
{
    /// <summary>
    /// BoolToColorConverter
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the FalseBrush value for the converter.
        /// </summary>
        /// <value>a Brush</value>
        public Brush FalseBrush { get; set; } = Brushes.Black;

        /// <summary>
        /// Gets or sets the TrueBrush value for the converter.
        /// </summary>
        /// <value>a Brush</value>
        public Brush TrueBrush { get; set; } = Brushes.White;

        /// <summary>
        /// Convert a boolean into to a Brush.  The conversion is defined by the FalseBrush and TrueBrush properties.
        /// </summary>
        /// <param name="value">A boolean to convert to a visibility</param>
        /// <param name="targetType">The type that the converter returns</param>
        /// <param name="parameter">Any other input to the convert process</param>
        /// <param name="culture">what language is being used</param>
        /// <returns>a Brush based on the FalseBrush and TrueBrush properties</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool bValue = (bool)value;
                return bValue ? TrueBrush : FalseBrush;
            }
            catch
            {
            }

            return FalseBrush;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">a Brush</param>
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
