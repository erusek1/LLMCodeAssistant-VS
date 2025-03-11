using System;
using System.Windows;
using System.Threading.Tasks;
using LLMCodeAssistant.Services;
using LLMCodeAssistant.Utilities;

namespace LLMCodeAssistant.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly LLMService _llmService;

        public SettingsWindow()
        {
            InitializeComponent();
            
            // Initialize service
            _llmService = new LLMService();
            
            // Load current settings
            EndpointTextBox.Text = OllamaConfig.Instance.Endpoint;
            ModelTextBox.Text = OllamaConfig.Instance.Model;
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusTextBlock.Text = "Testing connection...";
                
                var (success, message) = await _llmService.VerifyOllamaConnectionAsync();
                
                if (success)
                {
                    StatusTextBlock.Text = "✓ " + message;
                }
                else
                {
                    StatusTextBlock.Text = "❌ " + message;
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"❌ Error: {ex.Message}";
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate endpoint URL
                if (!Uri.TryCreate(EndpointTextBox.Text, UriKind.Absolute, out _))
                {
                    MessageBox.Show("Please enter a valid endpoint URL", "Invalid Endpoint", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Validate model name
                if (string.IsNullOrWhiteSpace(ModelTextBox.Text))
                {
                    MessageBox.Show("Please enter a valid model name", "Invalid Model", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Save settings
                await OllamaConfig.UpdateConfigAsync(EndpointTextBox.Text, ModelTextBox.Text);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}