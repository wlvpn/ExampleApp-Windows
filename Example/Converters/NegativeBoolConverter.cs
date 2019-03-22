// <copyright file="NegativeBoolConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace Example.Converters
{
    /// <summary>
    /// NegativeBoolConverter
    /// </summary>
    public class NegativeBoolConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean into its negative.  true -> false, false -> true.
        /// </summary>
        /// <param name="value">A boolean to negate</param>
        /// <param name="targetType">The type that the converter returns</param>
        /// <param name="parameter">Any other input to the convert process</param>
        /// <param name="culture">what language is being used</param>
        /// <returns>the negative of the input</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool bValue = (bool)value;
                return !bValue;
            }
            catch
            {
            }

            return true;
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">a boolean</param>
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
