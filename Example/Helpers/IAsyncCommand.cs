// <copyright file="IAsyncCommand.cs" company="NetProtect, LLC">
// Copyright (c) NetProtect, LLC. All Rights Reserved.
// </copyright>

using System.Threading.Tasks;
using System.Windows.Input;

namespace Example.Helpers
{
    /// <summary>
    /// Interface IAsyncCommand
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Execute a command.
        /// </summary>
        /// <returns>Task.</returns>
        Task ExecuteAsync();

        /// <summary>
        /// Checks if the command can be executed.
        /// </summary>
        /// <returns>True if command can be executed, false if not.</returns>
        bool CanExecute();
    }

    /// <summary>
    /// Interface IAsyncCommand&lt;T&gt;
    /// </summary>
    /// <typeparam name="T">Parameter's type.</typeparam>
    public interface IAsyncCommand<T> : ICommand
    {
        /// <summary>
        /// Execute a command asynchronously.
        /// </summary>
        /// <param name="parameter">Command's parameter.</param>
        /// <returns>A task.</returns>
        Task ExecuteAsync(T parameter);

        /// <summary>
        /// CanExecute.
        /// </summary>
        /// <param name="parameter">Command's parameter.</param>
        /// <returns>True if command can be executed, false if not.</returns>
        bool CanExecute(T parameter);
    }
}