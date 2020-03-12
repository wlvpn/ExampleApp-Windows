// <copyright file="ICustomAuthenticator.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Example.Helpers
{
    /// <summary>
    /// Interface ICustomAuthenticator. Allows implementation of a custom API pre-authentication method.
    /// </summary>
    public interface ICustomAuthenticator
    {
        /// <summary>
        /// Logins the specified username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Task&lt;Tuple&lt;System.String, System.String&gt;&gt; containing the username and password to pass on to the WLVPN API.</returns>
        Task<Tuple<string, string>> Login(string username, string password);
    }
}
