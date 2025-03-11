using System;
using System.IO;
using System.Text.RegularExpressions;
using EnvDTE;

namespace LLMCodeAssistant.Utilities
{
    /// <summary>
    /// Extension methods for various classes.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extracts code blocks from a string.
        /// </summary>
        /// <param name="text">The text to extract code blocks from.</param>
        /// <returns>The extracted code.</returns>
        public static string ExtractCodeBlocks(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Extract code blocks (between triple backticks)
            var codeBlockPattern = @"```(?:\w+)?\s*\n([\s\S]*?)\n```";
            var matches = Regex.Matches(text, codeBlockPattern);

            if (matches.Count == 0)
            {
                // If no code blocks found, return the original text
                return text;
            }

            // Return the content of the first code block
            return matches[0].Groups[1].Value;
        }

        /// <summary>
        /// Creates a new file in a project.
        /// </summary>
        /// <param name="project">The project to create the file in.</param>
        /// <param name="filePath">The relative path of the file within the project.</param>
        /// <param name="content">The file content.</param>
        /// <returns>The created project item.</returns>
        public static ProjectItem CreateFileInProject(this Project project, string filePath, string content)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // Get the full path
            string projectDir = Path.GetDirectoryName(project.FullName);
            string fullPath = Path.Combine(projectDir, filePath);
            
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Write the file
            File.WriteAllText(fullPath, content);
            
            // Add to project
            return project.ProjectItems.AddFromFile(fullPath);
        }

        /// <summary>
        /// Gets the file extension for a programming language.
        /// </summary>
        /// <param name="language">The programming language.</param>
        /// <returns>The file extension.</returns>
        public static string GetFileExtension(this string language)
        {
            return language.ToLower() switch
            {
                "csharp" => ".cs",
                "visualbasic" => ".vb",
                "javascript" => ".js",
                "typescript" => ".ts",
                "html" => ".html",
                "css" => ".css",
                "python" => ".py",
                "java" => ".java",
                "cpp" => ".cpp",
                "c" => ".c",
                "sql" => ".sql",
                "xml" => ".xml",
                "json" => ".json",
                "markdown" => ".md",
                _ => ".txt"
            };
        }
    }
}