using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LLMCodeAssistant.Utilities;
using Microsoft.VisualStudio.Shell;

namespace LLMCodeAssistant.Services
{
    /// <summary>
    /// Service for interacting with the LLM via Ollama local API.
    /// </summary>
    public class LLMService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaEndpoint;
        private readonly string _modelName;
        private readonly PromptBuilder _promptBuilder;

        /// <summary>
        /// Initializes a new instance of the LLMService class.
        /// </summary>
        public LLMService()
        {
            _httpClient = new HttpClient();
            
            // Get Ollama settings - default to localhost:11434 if not configured
            _ollamaEndpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? "http://localhost:11434";
            _modelName = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "codellama:34b";
            
            _promptBuilder = new PromptBuilder();

            // Set up HTTP client
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        /// <summary>
        /// Analyzes code for issues.
        /// </summary>
        /// <param name="code">The code to analyze.</param>
        /// <param name="language">The programming language.</param>
        /// <returns>Analysis results as a string.</returns>
        public async Task<string> AnalyzeCodeAsync(string code, string language)
        {
            string prompt = _promptBuilder.BuildAnalysisPrompt(code, language);
            return await SendRequestAsync(prompt);
        }

        /// <summary>
        /// Generates fixes for identified issues.
        /// </summary>
        /// <param name="code">The code to fix.</param>
        /// <param name="issues">Identified issues.</param>
        /// <param name="language">The programming language.</param>
        /// <returns>Fixed code as a string.</returns>
        public async Task<string> GenerateFixesAsync(string code, string issues, string language)
        {
            string prompt = _promptBuilder.BuildFixPrompt(code, issues, language);
            return await SendRequestAsync(prompt);
        }

        /// <summary>
        /// Generates new code based on a description.
        /// </summary>
        /// <param name="description">Description of the code to generate.</param>
        /// <param name="language">The programming language.</param>
        /// <returns>Generated code as a string.</returns>
        public async Task<string> GenerateCodeAsync(string description, string language)
        {
            string prompt = _promptBuilder.BuildGenerationPrompt(description, language);
            return await SendRequestAsync(prompt);
        }

        /// <summary>
        /// Continues a conversation with the LLM.
        /// </summary>
        /// <param name="messages">Previous messages in the conversation.</param>
        /// <param name="newMessage">New message to send.</param>
        /// <returns>LLM response as a string.</returns>
        public async Task<string> ContinueConversationAsync(List<(string Role, string Content)> messages, string newMessage)
        {
            // Format messages for Ollama API
            var ollamaMessages = new List<object>();
            foreach (var (role, content) in messages)
            {
                // Convert role from OpenAI format to Ollama format
                string ollamaRole = role;
                if (role == "assistant") ollamaRole = "assistant";
                if (role == "user") ollamaRole = "user";
                if (role == "system") ollamaRole = "system";
                
                ollamaMessages.Add(new { role = ollamaRole, content });
            }
            
            // Add new message if provided
            if (!string.IsNullOrEmpty(newMessage))
            {
                ollamaMessages.Add(new { role = "user", content = newMessage });
            }

            // Create request payload for Ollama
            var requestPayload = new
            {
                model = _modelName,
                messages = ollamaMessages,
                stream = false,
                options = new
                {
                    temperature = 0.2,
                    top_p = 0.95,
                    num_predict = 4000
                }
            };

            // Send request to Ollama
            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            
            try
            {
                var response = await _httpClient.PostAsync($"{_ollamaEndpoint}/api/chat", content);
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                
                var messageContent = responseObject.GetProperty("message").GetProperty("content").GetString();
                
                // Add assistant response to conversation history if valid
                if (!string.IsNullOrEmpty(messageContent))
                {
                    messages.Add(("assistant", messageContent));
                }
                
                return messageContent ?? "No response from model";
            }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                // Log error in VS output window
                var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow));
                // Handle error
                return $"Error communicating with local LLM: {ex.Message}\n\nPlease ensure Ollama is running with the codellama:34b model installed.\nYou can install it with: ollama pull codellama:34b";
            }
        }

        /// <summary>
        /// Sends a request to the Ollama API.
        /// </summary>
        /// <param name="prompt">The prompt to send.</param>
        /// <returns>LLM response as a string.</returns>
        private async Task<string> SendRequestAsync(string prompt)
        {
            // Create a simple conversation with just this prompt
            var messages = new List<(string, string)>
            {
                ("system", "You are a helpful AI assistant for analyzing and generating code."),
                ("user", prompt)
            };
            
            return await ContinueConversationAsync(messages, "");
        }

        /// <summary>
        /// Verifies the Ollama connection and model availability.
        /// </summary>
        /// <returns>True if Ollama is available and the model is loaded, false otherwise.</returns>
        public async Task<(bool Success, string Message)> VerifyOllamaConnectionAsync()
        {
            try
            {
                // Check if Ollama is running
                var response = await _httpClient.GetAsync($"{_ollamaEndpoint}/api/tags");
                if (!response.IsSuccessStatusCode)
                {
                    return (false, $"Ollama server is not reachable at {_ollamaEndpoint}");
                }

                // Parse response to check for the model
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

                // Verify if our model exists in the list
                var models = responseObject.GetProperty("models");
                bool modelFound = false;

                foreach (var model in models.EnumerateArray())
                {
                    string name = model.GetProperty("name").GetString();
                    if (name != null && name.Contains("codellama"))
                    {
                        modelFound = true;
                        break;
                    }
                }

                if (!modelFound)
                {
                    return (false, "CodeLlama model not found. Please install with 'ollama pull codellama:34b'");
                }

                return (true, "Ollama is running and CodeLlama model is available");
            }
            catch (Exception ex)
            {
                return (false, $"Error connecting to Ollama: {ex.Message}");
            }
        }
    }
}