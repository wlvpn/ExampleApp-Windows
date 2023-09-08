// <copyright file="BooleanInverseConverter.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace Example.Converters
{
    /// <summary>
    /// Convert a boolean to boolean inverted.
    /// </summary>
    public class BooleanInverseConverter : IValueConverter
    {
        /// <summary>
        /// Convetrs boolean to inverse.
        /// </summary>
        /// <param name="value">bool value input from the binding.</param>
        /// <param name="targetType">the return type.</param>
        /// <param name="parameter">any additional info needed.</param>
        /// <param name="culture">users culture.</param>
        /// <returns>the converted value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        /// <summary>
        /// converts the value back to a orignal bool alue.
        /// </summary>
        /// <param name="value">value from the binding.</param>
        /// <param name="targetType">type to convert to.</param>
        /// <param name="parameter">any additional info needed.</param>
        /// <param name="culture">Users Culture.</param>
        /// <returns>the converted value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
