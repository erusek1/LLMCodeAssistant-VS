using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LLMCodeAssistant.Services;
using LLMCodeAssistant.Utilities;
using Microsoft.VisualStudio.Shell;

namespace LLMCodeAssistant.UI.ViewModel
{
    /// <summary>
    /// Assistant modes.
    /// </summary>
    public enum AssistantMode
    {
        Analysis,
        Fix,
        Generate
    }

    /// <summary>
    /// Chat message model.
    /// </summary>
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public bool IsUser { get; set; }
    }

    /// <summary>
    /// View model for the Assistant window.
    /// </summary>
    public class AssistantViewModel : INotifyPropertyChanged
    {
        private readonly LLMService _llmService;
        private readonly FileService _fileService;
        private readonly AnalysisService _analysisService;
        private AssistantMode _currentMode;
        private string _analysisResult;
        private string _fixedCode;
        private string _chatInput;
        private string _statusMessage;
        private bool _isProcessing;
        private ObservableCollection<ChatMessage> _chatMessages;
        private string _modelName;

        /// <summary>
        /// Event handler for property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the current assistant mode.
        /// </summary>
        public AssistantMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                OnPropertyChanged(nameof(CurrentMode));
                OnPropertyChanged(nameof(AnalysisMode));
                OnPropertyChanged(nameof(FixMode));
                OnPropertyChanged(nameof(GenerateMode));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the assistant is in analysis mode.
        /// </summary>
        public Visibility AnalysisMode => CurrentMode == AssistantMode.Analysis ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets a value indicating whether the assistant is in fix mode.
        /// </summary>
        public Visibility FixMode => CurrentMode == AssistantMode.Fix ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets a value indicating whether the assistant is in generate mode.
        /// </summary>
        public Visibility GenerateMode => CurrentMode == AssistantMode.Generate ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Gets or sets the analysis result.
        /// </summary>
        public string AnalysisResult
        {
            get => _analysisResult;
            set
            {
                _analysisResult = value;
                OnPropertyChanged(nameof(AnalysisResult));
            }
        }

        /// <summary>
        /// Gets or sets the fixed code.
        /// </summary>
        public string FixedCode
        {
            get => _fixedCode;
            set
            {
                _fixedCode = value;
                OnPropertyChanged(nameof(FixedCode));
                OnPropertyChanged(nameof(HasFixedCode));
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is fixed code available.
        /// </summary>
        public bool HasFixedCode => !string.IsNullOrEmpty(FixedCode);

        /// <summary>
        /// Gets or sets the chat input.
        /// </summary>
        public string ChatInput
        {
            get => _chatInput;
            set
            {
                _chatInput = value;
                OnPropertyChanged(nameof(ChatInput));
                OnPropertyChanged(nameof(HasChatInput));
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is chat input available.
        /// </summary>
        public bool HasChatInput => !string.IsNullOrEmpty(ChatInput);

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the assistant is processing.
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        /// <summary>
        /// Gets the chat messages.
        /// </summary>
        public ObservableCollection<ChatMessage> ChatMessages
        {
            get => _chatMessages;
            private set
            {
                _chatMessages = value;
                OnPropertyChanged(nameof(ChatMessages));
            }
        }
        
        /// <summary>
        /// Gets the current LLM model name.
        /// </summary>
        public string ModelName
        {
            get => _modelName;
            private set
            {
                _modelName = value;
                OnPropertyChanged(nameof(ModelName));
            }
        }

        /// <summary>
        /// Initializes a new instance of the AssistantViewModel class.
        /// </summary>
        public AssistantViewModel()
        {
            // Initialize services
            _llmService = new LLMService();
            _fileService = new FileService();
            _analysisService = new AnalysisService(_llmService, _fileService);

            // Initialize properties
            CurrentMode = AssistantMode.Analysis;
            AnalysisResult = string.Empty;
            FixedCode = string.Empty;
            ChatInput = string.Empty;
            StatusMessage = "Ready";
            IsProcessing = false;
            ChatMessages = new ObservableCollection<ChatMessage>();
            ModelName = OllamaConfig.Instance.Model;

            // Add initial welcome message
            ChatMessages.Add(new ChatMessage
            {
                Sender = "Assistant",
                Content = $"Hello! I'm your Local LLM Code Assistant using {OllamaConfig.Instance.Model}. How can I help you today? You can ask me to analyze your code, suggest fixes, or help you generate new code.",
                IsUser = false
            });
            
            // Check Ollama connection status
            _ = Task.Run(async () => await CheckOllamaStatusAsync());
        }

        /// <summary>
        /// Checks Ollama connection status.
        /// </summary>
        private async Task CheckOllamaStatusAsync()
        {
            try
            {
                await OllamaStatusChecker.CheckConnectionAsync();
            }
            catch (Exception)
            {
                // Silently fail - don't want to block VS startup with exceptions
            }
        }

        /// <summary>
        /// Sets the assistant mode.
        /// </summary>
        /// <param name="mode">The mode to set.</param>
        public void SetMode(AssistantMode mode)
        {
            CurrentMode = mode;
        }

        /// <summary>
        /// Analyzes the currently active document.
        /// </summary>
        public async Task AnalyzeActiveDocumentAsync()
        {
            IsProcessing = true;
            StatusMessage = "Analyzing code...";

            try
            {
                AnalysisResult = await _analysisService.AnalyzeActiveDocumentAsync();
                StatusMessage = "Analysis complete";
            }
            catch (Exception ex)
            {
                AnalysisResult = $"Error analyzing document: {ex.Message}";
                StatusMessage = "Analysis failed";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Generates fixes for the analyzed code.
        /// </summary>
        public async Task GenerateFixesAsync()
        {
            IsProcessing = true;
            StatusMessage = "Generating fixes...";

            try
            {
                FixedCode = await _analysisService.GenerateFixesForActiveDocumentAsync(AnalysisResult);
                StatusMessage = "Fixes generated";
            }
            catch (Exception ex)
            {
                FixedCode = $"Error generating fixes: {ex.Message}";
                StatusMessage = "Fix generation failed";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Applies the generated fixes to the document.
        /// </summary>
        public async Task ApplyFixesAsync()
        {
            IsProcessing = true;
            StatusMessage = "Applying fixes...";

            try
            {
                string result = await _analysisService.ApplyFixesToActiveDocumentAsync(FixedCode);
                StatusMessage = result;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error applying fixes: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Sends a chat message to the LLM.
        /// </summary>
        public async Task SendChatMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(ChatInput))
            {
                return;
            }

            IsProcessing = true;
            StatusMessage = "Processing message...";

            try
            {
                // Add user message to chat
                string userMessage = ChatInput;
                ChatMessages.Add(new ChatMessage
                {
                    Sender = "You",
                    Content = userMessage,
                    IsUser = true
                });

                // Clear input
                ChatInput = string.Empty;

                // Get messages for the conversation
                var messages = ChatMessages.Select(m => (m.IsUser ? "user" : "assistant", m.Content)).ToList();

                // Get response from LLM
                string response = await _llmService.ContinueConversationAsync(messages, string.Empty);

                // Check for code generation intent
                if (IsCodeGenerationRequest(userMessage))
                {
                    // Parse for file paths and content
                    await HandleCodeGenerationAsync(response);
                }

                // Add assistant message to chat
                ChatMessages.Add(new ChatMessage
                {
                    Sender = "Assistant",
                    Content = response,
                    IsUser = false
                });

                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                ChatMessages.Add(new ChatMessage
                {
                    Sender = "System",
                    Content = $"Error: {ex.Message}",
                    IsUser = false
                });
                StatusMessage = "Error processing message";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Opens the settings window.
        /// </summary>
        public void OpenSettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Checks if the message is a code generation request.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns>True if the message is a code generation request, false otherwise.</returns>
        private bool IsCodeGenerationRequest(string message)
        {
            string lowerMessage = message.ToLower();
            return lowerMessage.Contains("create") || lowerMessage.Contains("generate") || 
                   lowerMessage.Contains("build") || lowerMessage.Contains("make") || 
                   lowerMessage.Contains("write") || lowerMessage.Contains("develop");
        }

        /// <summary>
        /// Handles code generation from LLM response.
        /// </summary>
        /// <param name="response">The LLM response.</param>
        private async Task HandleCodeGenerationAsync(string response)
        {
            try
            {
                // Look for file paths and code blocks
                var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    // Look for file path indicators
                    if (lines[i].Contains(".") && (lines[i].Contains("/") || lines[i].Contains("\\")))
                    {
                        string potentialPath = lines[i].Trim();
                        
                        // Look for code blocks following this line
                        if (i + 1 < lines.Length && lines[i + 1].Trim().StartsWith("```"))
                        {
                            // Find the end of the code block
                            int start = i + 2; // Skip the opening ```
                            int end = -1;
                            for (int j = start; j < lines.Length; j++)
                            {
                                if (lines[j].Trim().StartsWith("```"))
                                {
                                    end = j;
                                    break;
                                }
                            }

                            if (end > start)
                            {
                                // Extract the code
                                string code = string.Join(Environment.NewLine, lines.Skip(start).Take(end - start));
                                
                                // Create the file
                                bool success = await _fileService.CreateFileAsync(potentialPath, code);
                                
                                if (success)
                                {
                                    // Add message to chat
                                    ChatMessages.Add(new ChatMessage
                                    {
                                        Sender = "System",
                                        Content = $"Created file: {potentialPath}",
                                        IsUser = false
                                    });
                                }
                                
                                // Skip to after this code block
                                i = end;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Add error message to chat
                ChatMessages.Add(new ChatMessage
                {
                    Sender = "System",
                    Content = $"Error creating files: {ex.Message}",
                    IsUser = false
                });
            }
        }
    }
}