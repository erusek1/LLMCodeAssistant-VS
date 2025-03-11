using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LLMCodeAssistant.Commands
{
    /// <summary>
    /// Base class for all commands in the extension.
    /// </summary>
    public abstract class CommandBase
    {
        /// <summary>
        /// VS Package that provides this command.
        /// </summary>
        protected readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the CommandBase class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        protected CommandBase(AsyncPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Shows a message box in Visual Studio.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="title">The title of the message box.</param>
        protected void ShowMessage(string message, string title = "LLM Code Assistant")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// Shows an error message box in Visual Studio.
        /// </summary>
        /// <param name="message">The error message to show.</param>
        /// <param name="title">The title of the message box.</param>
        protected void ShowError(string message, string title = "LLM Code Assistant - Error")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// Shows the Assistant tool window.
        /// </summary>
        protected void ShowAssistantWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var commandService = this.package as IServiceProvider;
            if (commandService != null)
            {
                var windowFrame = package.FindToolWindow(typeof(UI.AssistantWindow), 0, true);
                if (windowFrame?.Frame == null)
                {
                    throw new NotSupportedException("Cannot create tool window");
                }

                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(((IVsWindowFrame)windowFrame.Frame).Show());
            }
        }
    }
}