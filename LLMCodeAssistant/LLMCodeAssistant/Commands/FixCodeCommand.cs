using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using LLMCodeAssistant.UI;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LLMCodeAssistant.Commands
{
    /// <summary>
    /// Command handler for fixing code.
    /// </summary>
    internal sealed class FixCodeCommand : CommandBase
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3b59944b-1b3c-4e40-8e4b-c5f19a8f5c8f");

        /// <summary>
        /// Initializes a new instance of the FixCodeCommand class.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private FixCodeCommand(AsyncPackage package) : base(package)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FixCodeCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new FixCodeCommand(package);
            
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(Instance.Execute, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            // Show the assistant window
            ShowAssistantWindow();

            // Get the window and set its mode to fix
            var window = package.FindToolWindow(typeof(AssistantWindow), 0, false) as AssistantWindow;
            if (window != null)
            {
                // Check if we have analysis results
                if (string.IsNullOrEmpty(window.ViewModel.AnalysisResult))
                {
                    ShowMessage("Please analyze the code first.", "Fix Code");
                    return;
                }

                window.ViewModel.SetMode(AssistantMode.Fix);
                
                // Generate fixes
                _ = Task.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    await window.ViewModel.GenerateFixesAsync();
                });
            }
            else
            {
                ShowError("Could not find the Assistant window.");
            }
        }
    }
}