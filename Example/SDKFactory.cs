// <copyright file="SDKFactory.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.IO;
using VpnSDK;
using VpnSDK.Interfaces;

namespace Example
{
    /// <summary>
    /// Class SDKManager.
    /// </summary>
    public static class SDKFactory
    {
        private static ISDK _instance;

        internal static ISDK GetInstance()
        {
            if (_instance != null && _instance.IsDisposed)
            {
                throw new ObjectDisposedException("SDK instance is disposed.");
            }

            return _instance ?? (_instance = CreateInstance());
        }

        private static ISDK CreateInstance()
        {
            if (string.IsNullOrEmpty(Helpers.Resource.Get<string>("ApiKey")) ||
                string.IsNullOrEmpty(Helpers.Resource.Get<string>("AuthorizationSuffix")))
            {
                throw new VpnSDK.InvalidConfigurationException("API key or Authorization token was not set. Please see ApiAccessData.xaml file for more info.");
            }

            return new SDKBuilder<ISDKInternal>()
                            .SetApiKey(Helpers.Resource.Get<string>("ApiKey"))
                            .SetAuthenticationToken(Helpers.Resource.Get<string>("AuthorizationSuffix"))
                            .SetApplicationName("Example Application")
                            .SetAutomaticRefreshTokenHandling(true)
                            .SetServerListCache(TimeSpan.FromDays(1))
                            .Create();
        }
    }
}
