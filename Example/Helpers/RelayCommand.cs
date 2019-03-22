// <copyright file="RelayCommand.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace Example.Helpers
{
    /// <summary>
    /// RelayCommand, used in WPD for as an ICommand for any control which will take one.
    /// </summary>
    public class RelayCommand : ICommand, INotifyPropertyChanged, IDisposable
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;
        private bool _isExecutable = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// Initializes a new instance of the RelayCommand class that
        /// can always execute.
        /// </summary>
        /// <param name="execute">The Action to execute</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// The INPC implementation event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object derived is executable.
        /// </summary>
        /// <value>a Brush</value>
        public bool IsExecutable
        {
            get
            {
                return _isExecutable;
            }

            set
            {
                _isExecutable = value;
                RaisePropertyChanged("IsExecutable");
            }
        }

        /// <summary>
        /// RunOnDisplayThread.  the funtion is used to run an action on the UI thread.
        /// </summary>
        /// <param name="actionToExecute">The supplied action will be executed on the UI thread</param>
        public static void RunOnDisplayThread(Action actionToExecute)
        {
            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;

            if (dispatcher != null && dispatcher.CheckAccess() == false)
            {
                dispatcher.BeginInvoke(actionToExecute);
            }
            else
            {
                actionToExecute();
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged()
        {
            RunOnDisplayThread(() =>
            {
                CommandManager.InvalidateRequerySuggested();
                RaisePropertyChanged("IsExecutable");
            });
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            IsExecutable = _canExecute == null ? true : _canExecute(parameter);
            return IsExecutable;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _execute = (parm) => { };
            _canExecute = (parm) => false;
        }

        /// <summary>
        /// The Property Changed Event
        /// </summary>
        /// <param name="name">the name of the property that changed</param>
        private void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
