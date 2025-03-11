using System;
using System.Threading.Tasks;
using System.Windows;
using LLMCodeAssistant.Services;
using Microsoft.VisualStudio.Shell;

namespace LLMCodeAssistant.Utilities
{
    /// <summary>
    /// Utility class to check Ollama connection status.
    /// </summary>
    public static class OllamaStatusChecker
    {
        private static bool _initialCheckPerformed = false;
        
        /// <summary>
        /// Checks the Ollama connection status and shows a warning if there are issues.
        /// </summary>
        public static async Task CheckConnectionAsync()
        {
            try
            {
                // Only perform this check once per session
                if (_initialCheckPerformed)
                {
                    return;
                }
                
                _initialCheckPerformed = true;
                
                // Create service to check connection
                var llmService = new LLMService();
                
                // Check connection
                var (success, message) = await llmService.VerifyOllamaConnectionAsync();
                
                if (!success)
                {
                    // Switch to UI thread for showing message box
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    
                    MessageBox.Show(
                        $"Could not connect to Ollama LLM:\n\n{message}\n\nPlease check your settings and ensure Ollama is running with CodeLlama model installed.\n\nYou can install it with: ollama pull codellama:34b",
                        "LLM Connection Issue",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                // Silently fail - don't want to block VS startup with exceptions
            }
        }
    }
}