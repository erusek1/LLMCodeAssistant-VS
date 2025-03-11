using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLMCodeAssistant.Utilities;
using Microsoft.VisualStudio.Shell;

namespace LLMCodeAssistant.Services
{
    /// <summary>
    /// Service for analyzing code and generating reports.
    /// </summary>
    public class AnalysisService
    {
        private readonly LLMService _llmService;
        private readonly FileService _fileService;

        /// <summary>
        /// Initializes a new instance of the AnalysisService class.
        /// </summary>
        /// <param name="llmService">The LLM service to use.</param>
        /// <param name="fileService">The file service to use.</param>
        public AnalysisService(LLMService llmService, FileService fileService)
        {
            _llmService = llmService;
            _fileService = fileService;
        }

        /// <summary>
        /// Analyzes the currently active document for issues.
        /// </summary>
        /// <returns>The analysis report.</returns>
        public async Task<string> AnalyzeActiveDocumentAsync()
        {
            try
            {
                // Get the content of the active document
                var (content, language) = await _fileService.GetActiveDocumentContentAsync();

                if (string.IsNullOrEmpty(content))
                {
                    return "No active document found.";
                }

                // Get the path of the active document
                var path = _fileService.GetActiveDocumentPath();

                // Analyze the code
                var analysisResult = await _llmService.AnalyzeCodeAsync(content, language);

                // Format the report
                return $"Analysis of {path}:\n\n{analysisResult}";
            }
            catch (Exception ex)
            {
                return $"Error analyzing document: {ex.Message}";
            }
        }

        /// <summary>
        /// Analyzes all files in the current solution or project.
        /// </summary>
        /// <returns>The analysis report for all files.</returns>
        public async Task<string> AnalyzeAllFilesAsync()
        {
            try
            {
                // Get all files in the solution
                var files = await _fileService.GetAllFilesAsync();

                if (files.Count == 0)
                {
                    return "No files found in the solution.";
                }

                // Analyze each file
                var analysisResults = new Dictionary<string, string>();
                foreach (var (path, content) in files)
                {
                    // Determine language from file extension
                    string language = Path.GetExtension(path).ToLower().TrimStart('.');
                    
                    // Skip binary files and too large files
                    if (IsBinaryOrLargeFile(content))
                    {
                        continue;
                    }

                    // Analyze the code
                    var analysisResult = await _llmService.AnalyzeCodeAsync(content, language);
                    analysisResults[path] = analysisResult;
                }

                // Format the report
                var report = new StringBuilder();
                report.AppendLine($"Analysis of {analysisResults.Count} files:");
                report.AppendLine();

                foreach (var (path, result) in analysisResults)
                {
                    report.AppendLine($"File: {path}");
                    report.AppendLine(new string('-', path.Length + 6));
                    report.AppendLine(result);
                    report.AppendLine();
                    report.AppendLine();
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                return $"Error analyzing files: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates fixes for issues in the currently active document.
        /// </summary>
        /// <param name="analysisReport">The analysis report containing issues.</param>
        /// <returns>The fixed code.</returns>
        public async Task<string> GenerateFixesForActiveDocumentAsync(string analysisReport)
        {
            try
            {
                // Get the content of the active document
                var (content, language) = await _fileService.GetActiveDocumentContentAsync();

                if (string.IsNullOrEmpty(content))
                {
                    return "No active document found.";
                }

                // Generate fixes
                var fixedCode = await _llmService.GenerateFixesAsync(content, analysisReport, language);

                return fixedCode;
            }
            catch (Exception ex)
            {
                return $"Error generating fixes: {ex.Message}";
            }
        }

        /// <summary>
        /// Applies fixes to the currently active document.
        /// </summary>
        /// <param name="fixedCode">The fixed code to apply.</param>
        /// <returns>Success message or error.</returns>
        public async Task<string> ApplyFixesToActiveDocumentAsync(string fixedCode)
        {
            try
            {
                // Extract code block if present
                string codeToApply = ExtractCodeBlock(fixedCode);
                
                // Update the active document with the fixed code
                bool success = await _fileService.UpdateActiveDocumentAsync(codeToApply);

                if (success)
                {
                    return "Fixes applied successfully.";
                }
                else
                {
                    return "Failed to apply fixes.";
                }
            }
            catch (Exception ex)
            {
                return $"Error applying fixes: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Extracts a code block from a string.
        /// </summary>
        /// <param name="text">The text containing a code block.</param>
        /// <returns>The extracted code block, or the original text if no code block is found.</returns>
        private string ExtractCodeBlock(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            
            // Check if the text contains a code block
            if (text.Contains("```"))
            {
                // Find start and end of the first code block
                int start = text.IndexOf("```");
                start = text.IndexOf('\n', start) + 1;
                int end = text.IndexOf("```", start);
                
                if (start > 0 && end > start)
                {
                    return text.Substring(start, end - start).Trim();
                }
            }
            
            return text;
        }

        /// <summary>
        /// Checks if a file is binary or too large to analyze.
        /// </summary>
        /// <param name="content">The file content.</param>
        /// <returns>True if the file is binary or too large, false otherwise.</returns>
        private bool IsBinaryOrLargeFile(string content)
        {
            // Skip files larger than 100 KB
            if (content.Length > 100 * 1024)
            {
                return true;
            }

            // Check for binary content (contains many null or non-printable characters)
            int nonPrintableCount = content.Count(c => c < 32 && c != 9 && c != 10 && c != 13);
            return nonPrintableCount > content.Length * 0.1;
        }
    }
}