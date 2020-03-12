// <copyright file="DestinationsViewModel.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using Example.Helpers;
using Example.Views;
using VpnSDK.Interfaces;

namespace Example.ViewModel
{
    /// <summary>
    /// GeneralSettingsViewModel
    /// </summary>
    public class DestinationsViewModel : BindableBase
    {
        private readonly SynchronizationContextScheduler _synchronizationContextScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);
        private readonly IDisposable _serverListLoader;
        private string _search = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestinationsViewModel"/> class.
        /// </summary>
        public DestinationsViewModel()
        {
            var filter = this.WhenValueChanged(t => t.Search)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Select(RegionFilter);

            _serverListLoader = SDK.Locations.ToObservableChangeSet().AsObservableList()
                .Connect()
                .Filter(filter)
                .Sort(LocationsComparer.Ascending(r =>
                {
                    return r is IRegion region ? region.Country : r.CountryCode;
                }))
                .ObserveOn(_synchronizationContextScheduler)
                .Bind(Destinations)
                .Subscribe();
        }

        /// <summary>
        /// Gets SDK Instance.
        /// </summary>
        public ISDK SDK => SDKFactory.GetInstance();

        /// <summary>
        /// Gets a value that represents the list of ILocation available to the client
        /// </summary>
        /// <value>list of available ILocation</value>
        public IObservableCollection<ILocation> Destinations { get; } = new ObservableCollectionExtended<ILocation>();

        /// <summary>
        /// Gets or sets a value that represents the currently selected location/destination for the application.  This is what the outside world should see as
        /// your location. The GeoIP used by the external world may sometimes not recognize it properly.  But it is what we aer connecting to.
        /// </summary>
        /// <value>the currently selected destination</value>
        public ILocation SelectedDestination
        {
            get => Properties.Settings.Default.SelectedDestination;

            set
            {
                Properties.Settings.Default.SelectedDestination = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the current filter value for the search of the servers.
        /// </summary>
        /// <value>the search string</value>
        public string Search
        {
            get { return _search; }
            set { SetProperty(ref _search, value); }
        }

        /// <summary>
        /// The Region Filter is used to filter the set of ILocations shown in the Destinations list.
        /// </summary>
        /// <param name="searchText">The current filter text</param>
        /// <returns>the Func&lt;ILocation, bool&gt; used to perform the filter</returns>
        private Func<ILocation, bool> RegionFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return location => true;
            }

            return location =>
            {
                if (location is IBestAvailable bestAvailable)
                {
                    return bestAvailable.SearchName.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
                           || bestAvailable.SearchName.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
                }

                if (location is IRegion region)
                {
                    return region.SearchName.ToLowerInvariant().Split(' ').Any(x => searchText.ToLowerInvariant().Split(' ').Any(x.StartsWith));
                }

                return true;
            };
        }
    }
}