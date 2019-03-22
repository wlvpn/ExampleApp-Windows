// <copyright file="AsyncCommand.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Example.Helpers
{
    /// <summary>
    /// Class AsyncCommand.
    /// </summary>
    public class AsyncCommand : IAsyncCommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private readonly Action<Exception> _errorHandler;
        private bool _isExecuting;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand"/> class.
        /// </summary>
        /// <param name="execute">Task to execute.</param>
        /// <param name="canExecute">Indicates if the task can be executed or not.</param>
        /// <param name="errorHandler">Error handler.</param>
        public AsyncCommand(
            Func<Task> execute,
            Func<bool> canExecute = null,
            Action<Exception> errorHandler = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Event CanExecuteChanged.
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
        /// Run action on UI thread.
        /// </summary>
        /// <param name="actionToExecute">Action.</param>
        public static void RunOnDisplayThread(Action actionToExecute)
        {
            var dispatcher = Application.Current?.Dispatcher;

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
        /// CanExecute.
        /// </summary>
        /// <returns>True if command can be executed, false if not.</returns>
        public bool CanExecute()
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// ExecuteAsync.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            if (CanExecute())
            {
                try
                {
                    _isExecuting = true;
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Raise <see cref="CommandManager.RequerySuggested"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            RunOnDisplayThread(CommandManager.InvalidateRequerySuggested);
        }

        /// <summary>
        /// Explicit implementation of <see cref="ICommand.CanExecute"/>.
        /// </summary>
        /// <param name="parameter">Object.</param>
        /// <returns>True if command can be executed, false if not.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        /// <summary>
        /// Explicit implementation of <see cref="ICommand.Execute"/>.
        /// </summary>
        /// <param name="parameter">Object.</param>
        void ICommand.Execute(object parameter)
        {
            ExecuteAsync().FireAndForgetSafeAsync(_errorHandler);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    /// <summary>
    /// Class AsyncCommand&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">Type.</typeparam>
    public class AsyncCommand<T> : IAsyncCommand<T>
#pragma warning restore SA1402 // File may only contain a single class
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private readonly Action<Exception> _errorHandler;
        private bool _isExecuting;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">Task to execute.</param>
        /// <param name="canExecute">Indicates if the task can be executed or not.</param>
        /// <param name="errorHandler">Error handler.</param>
        public AsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, Action<Exception> errorHandler = null)
        {
            _execute = execute;
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Event CanExecuteChanged.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// CanExecute.
        /// </summary>
        /// <param name="parameter">Command's parameter.</param>
        /// <returns>True if command can be executed, false if not.</returns>
        public bool CanExecute(T parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
        }

        /// <summary>
        /// Execute command asynchronously.
        /// </summary>
        /// <param name="parameter">Command's parameter.</param>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync(T parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    await _execute(parameter);
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Raise <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Explicit implementation of <see cref="ICommand.CanExecute"/>.
        /// </summary>
        /// <param name="parameter">Object.</param>
        /// <returns>True if command can be executed, false if not.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        /// <summary>
        /// Explicit implementation of <see cref="ICommand.Execute"/>.
        /// </summary>
        /// <param name="parameter">Object.</param>
        void ICommand.Execute(object parameter)
        {
            ExecuteAsync((T)parameter).FireAndForgetSafeAsync(_errorHandler);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    /// <summary>
    /// TaskUtilities class.
    /// </summary>
    internal static class TaskUtilities
#pragma warning restore SA1402 // File may only contain a single class
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

        public static async void FireAndForgetSafeAsync(this Task task, Action<Exception> handler = null)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                handler?.Invoke(ex);
            }
        }
    }
}