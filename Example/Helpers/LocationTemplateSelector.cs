// <copyright file="LocationTemplateSelector.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VpnSDK.Interfaces;

namespace Example.Helpers
{
    /// <summary>
    /// LocationTemplateSelector
    /// </summary>
    public class LocationTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the template to use for a region
        /// </summary>
        public DataTemplate RegionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template to use for the Best Available row.
        /// </summary>
        public DataTemplate BestAvailableTemplate { get; set; }

        /// <summary>
        /// Returns the proper template for the given data type
        /// </summary>
        /// <param name="item">the object we need a data template for</param>
        /// <param name="container">where the template will be used</param>
        /// <returns>the proper DataTemplate</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IBestAvailable)
            {
                return BestAvailableTemplate;
            }

            if (item is IRegion)
            {
                return RegionTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
