using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LLMCodeAssistant.UI.ViewModel;
using Microsoft.VisualStudio.Shell;

namespace LLMCodeAssistant.UI
{
    /// <summary>
    /// Assistant window for code analysis, fixing, and generation.
    /// </summary>
    [Guid("1c5de4e7-7a3e-4b13-9f39-c97a1c7b44d5")]
    public partial class AssistantWindow : UserControl
    {
        /// <summary>
        /// Reference to the view model.
        /// </summary>
        public AssistantViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the AssistantWindow class.
        /// </summary>
        public AssistantWindow()
        {
            InitializeComponent();
            
            // Create and set the view model
            ViewModel = new AssistantViewModel();
            DataContext = ViewModel;

            // Initialize value converters
            Resources.Add("MessageBackgroundConverter", new MessageBackgroundConverter());
        }

        /// <summary>
        /// Focuses on the chat input textbox.
        /// </summary>
        public void FocusOnInput()
        {
            ChatInputTextBox.Focus();
        }

        /// <summary>
        /// Handles the click event for the Analyze button.
        /// </summary>
        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetMode(AssistantMode.Analysis);
            _ = Task.Run(async () => await ViewModel.AnalyzeActiveDocumentAsync());
        }

        /// <summary>
        /// Handles the click event for the Fix button.
        /// </summary>
        private void FixButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if we have analysis results
            if (string.IsNullOrEmpty(ViewModel.AnalysisResult))
            {
                MessageBox.Show("Please analyze the code first.", "Fix Code", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ViewModel.SetMode(AssistantMode.Fix);
            _ = Task.Run(async () => await ViewModel.GenerateFixesAsync());
        }

        /// <summary>
        /// Handles the click event for the Generate button.
        /// </summary>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetMode(AssistantMode.Generate);
            FocusOnInput();
        }

        /// <summary>
        /// Handles the click event for the Settings button.
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        /// <summary>
        /// Handles the click event for the Apply Fixes button.
        /// </summary>
        private void ApplyFixesButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () => await ViewModel.ApplyFixesAsync());
        }

        /// <summary>
        /// Handles the click event for the Send button.
        /// </summary>
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(async () => await ViewModel.SendChatMessageAsync());
        }

        /// <summary>
        /// Handles the key down event for the chat input textbox.
        /// </summary>
        private void ChatInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Send message on Ctrl+Enter
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _ = Task.Run(async () => await ViewModel.SendChatMessageAsync());
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Converter to set the background color of chat messages based on sender.
    /// </summary>
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isUser)
            {
                return isUser ? new SolidColorBrush(Color.FromRgb(230, 230, 255)) : new SolidColorBrush(Color.FromRgb(240, 240, 240));
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}