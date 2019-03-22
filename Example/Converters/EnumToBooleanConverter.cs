// <copyright file="EnumToBooleanConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Example.Converters
{
    /// <summary>
    /// EnumToBooleanConverter
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts an Enum to a boolean based on == to the parameter
        /// </summary>
        /// <param name="value">the enum value</param>
        /// <param name="targetType">the type of the result</param>
        /// <param name="parameter">the enum value to compare to</param>
        /// <param name="culture">the current language/culture</param>
        /// <returns>true if the value is equal to the parameter, false otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        /// <summary>
        /// Converts the boolean back to the enum
        /// </summary>
        /// <param name="value">true or false</param>
        /// <param name="targetType">the type of the result</param>
        /// <param name="parameter">the value to convert to if true.</param>
        /// <param name="culture">the curernt langauge</param>
        /// <returns>the enum specified in the parameter if the value is true</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
