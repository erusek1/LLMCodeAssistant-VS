using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace LLMCodeAssistant.Utilities
{
    /// <summary>
    /// Configuration for Ollama LLM integration.
    /// </summary>
    public class OllamaConfig
    {
        // Default configuration values
        private const string DEFAULT_ENDPOINT = "http://localhost:11434";
        private const string DEFAULT_MODEL = "codellama:34b";
        
        // Configuration properties
        public string Endpoint { get; set; } = DEFAULT_ENDPOINT;
        public string Model { get; set; } = DEFAULT_MODEL;
        
        // Singleton instance
        private static OllamaConfig _instance;
        
        // Configuration file path
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LLMCodeAssistant",
            "config.json");
            
        /// <summary>
        /// Gets the singleton instance of the OllamaConfig.
        /// </summary>
        public static OllamaConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = LoadConfig().GetAwaiter().GetResult();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Loads the configuration from file or creates a default one.
        /// </summary>
        private static async Task<OllamaConfig> LoadConfig()
        {
            try
            {
                // Create directory if it doesn't exist
                string configDir = Path.GetDirectoryName(ConfigFilePath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // If config file exists, load it
                if (File.Exists(ConfigFilePath))
                {
                    string json = await File.ReadAllTextAsync(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<OllamaConfig>(json);
                    if (config != null)
                    {
                        return config;
                    }
                }
                
                // Create default config
                var defaultConfig = new OllamaConfig();
                
                // Save it
                await SaveConfigAsync(defaultConfig);
                
                return defaultConfig;
            }
            catch (Exception)
            {
                // Return default config on error
                return new OllamaConfig();
            }
        }
        
        /// <summary>
        /// Saves the configuration to file.
        /// </summary>
        public static async Task SaveConfigAsync(OllamaConfig config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error saving Ollama config: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Updates the configuration and saves it to file.
        /// </summary>
        public static async Task UpdateConfigAsync(string endpoint, string model)
        {
            var config = Instance;
            config.Endpoint = endpoint;
            config.Model = model;
            
            await SaveConfigAsync(config);
        }
    }
}