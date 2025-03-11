using System;
using System.Runtime.InteropServices;
using System.Threading;
using LLMCodeAssistant.Commands;
using LLMCodeAssistant.UI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace LLMCodeAssistant
{
    /// <summary>
    /// This is the main package class for the LLM Code Assistant extension.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(AssistantWindow))]
    public sealed class LLMCodeAssistantPackage : AsyncPackage
    {
        /// <summary>
        /// LLMCodeAssistantPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "b25d2a99-8a77-4eb3-a56e-0d1ef89ff6d4";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Register commands
            await AnalyzeCodeCommand.InitializeAsync(this);
            await FixCodeCommand.InitializeAsync(this);
            await GenerateCodeCommand.InitializeAsync(this);
            await SettingsCommand.InitializeAsync(this);

            // Register tool window
            await this.RegisterToolWindowsAsync();
        }

        /// <summary>
        /// Registers the tool windows with Visual Studio.
        /// </summary>
        private async Task RegisterToolWindowsAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            
            // Register the assistant window
            var windowPane = CreateToolWindow<AssistantWindow>(id: 0);
        }

        #endregion
    }
}
