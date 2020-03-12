// <copyright file="SmartCollection.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Example.Helpers
{
    /// <summary>
    /// Generic Observable collection that implements new methods to reduce CollectionChanged events
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    public class SmartCollection<T> : ObservableCollection<T>
    {
        private bool _suspendCollectionChangeNotification;
        private object _collectionLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartCollection{T}"/> class.
        /// Constructor
        /// </summary>
        public SmartCollection()
            : base()
        {
            _suspendCollectionChangeNotification = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartCollection{T}"/> class from an existing collection.
        /// Constructor
        /// </summary>
        /// <param name="items">The list of items to initialize the collection with</param>
        public SmartCollection(IEnumerable<T> items)
            : base()
        {
            AddRange(items);
        }

        /// <summary>
        /// Turns on the ability to protect/synchronize the list
        /// </summary>
        public void EnableSynchronization()
        {
            BindingOperations.EnableCollectionSynchronization(this, _collectionLock);
        }

        /// <summary>
        /// disable the Collection Changed event
        /// </summary>
        public void SuspendCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = true;
        }

        /// <summary>
        /// Enable the collection Changed event
        /// </summary>
        public void ResumeCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = false;
        }

        /// <summary>
        /// Add the specified range to the collection without a CollectionChanged event for every add
        /// </summary>
        /// <param name="items">The list of items to add</param>
        public void AddRange(IEnumerable<T> items)
        {
            try
            {
                if (items == null)
                {
                    return;
                }

                var inflatedItems = items.ToList();  // incase this is a linq statement and the defered execution would result in a null.

                if (inflatedItems == null || inflatedItems.Count == 0)
                {
                    return;
                }

                SuspendCollectionChangeNotification();

                foreach (var i in inflatedItems)
                {
                    base.Add(i);
                }
            }
            finally
            {
                ResumeCollectionChangeNotification();
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnCollectionChanged(arg);
            }
        }

        /// <summary>
        /// Clears and then add the list of items to the collection.
        /// </summary>
        /// <param name="items">The list of items to add</param>
        public void Repopulate(IEnumerable<T> items)
        {
            if (items != null)
            {
                try
                {
                    SuspendCollectionChangeNotification();
                    Clear();
                    AddRange(items);
                }
                finally
                {
                    ResumeCollectionChangeNotification();
                }
            }
        }

        /// <summary>
        /// causes the Reset event to be raised.
        /// </summary>
        public void Reset()
        {
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(arg);
        }

        /// <summary>
        /// Event raised when the collection changed
        /// </summary>
        /// <param name="e">The event to send</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suspendCollectionChangeNotification)
            {
                {
                    var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;

                    if (dispatcher != null && dispatcher.CheckAccess() == false)
                    {
                        dispatcher.BeginInvoke((Action)(() => OnCollectionChanged(e)));
                    }
                    else
                    {
                        base.OnCollectionChanged(e);
                    }
                }
            }
        }
    }
}
