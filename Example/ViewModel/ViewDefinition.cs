// <copyright file="ViewDefinition.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using Example.Helpers;

namespace Example.ViewModel
{
    /// <summary>
    /// Defines the a view for navigation
    /// </summary>
    public class ViewDefinition : BindableBase
    {
        /// <summary>
        /// Gets or sets a value for the name of the defined view
        /// </summary>
        /// <value>Name displayed to the user for this view</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value representing the view instance
        /// </summary>
        /// <value>the instance of the view</value>
        public object View { get; set; }
    }
}
