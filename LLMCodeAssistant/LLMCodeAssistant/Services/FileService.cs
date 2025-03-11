using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LLMCodeAssistant.Services
{
    /// <summary>
    /// Service for handling file operations within Visual Studio.
    /// </summary>
    public class FileService
    {
        private readonly DTE2 _dte;

        /// <summary>
        /// Initializes a new instance of the FileService class.
        /// </summary>
        public FileService()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
        }

        /// <summary>
        /// Gets the content of the currently active document.
        /// </summary>
        /// <returns>The document content and language.</returns>
        public async Task<(string Content, string Language)> GetActiveDocumentContentAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            if (_dte.ActiveDocument == null)
            {
                return (string.Empty, string.Empty);
            }

            // Get text from document
            var textDocument = _dte.ActiveDocument.Object("TextDocument") as TextDocument;
            if (textDocument == null)
            {
                return (string.Empty, string.Empty);
            }

            EditPoint startPoint = textDocument.StartPoint.CreateEditPoint();
            EditPoint endPoint = textDocument.EndPoint.CreateEditPoint();
            string content = startPoint.GetText(endPoint);

            // Determine language from file extension
            string fileExtension = Path.GetExtension(_dte.ActiveDocument.FullName).ToLower();
            string language = DetermineLanguageFromExtension(fileExtension);

            return (content, language);
        }

        /// <summary>
        /// Gets the path of the currently active document.
        /// </summary>
        /// <returns>The full path of the active document.</returns>
        public string GetActiveDocumentPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (_dte.ActiveDocument == null)
            {
                return string.Empty;
            }

            return _dte.ActiveDocument.FullName;
        }

        /// <summary>
        /// Updates the content of the active document.
        /// </summary>
        /// <param name="newContent">The new content to write to the file.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        public async Task<bool> UpdateActiveDocumentAsync(string newContent)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            try
            {
                if (_dte.ActiveDocument == null)
                {
                    return false;
                }

                var textDocument = _dte.ActiveDocument.Object("TextDocument") as TextDocument;
                if (textDocument == null)
                {
                    return false;
                }

                // Replace entire document content
                EditPoint startPoint = textDocument.StartPoint.CreateEditPoint();
                EditPoint endPoint = textDocument.EndPoint.CreateEditPoint();
                startPoint.Delete(endPoint);
                startPoint.Insert(newContent);

                // Save the document
                _dte.ActiveDocument.Save();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new file with the specified content.
        /// </summary>
        /// <param name="filePath">The full path of the file to create.</param>
        /// <param name="content">The content to write to the file.</param>
        /// <returns>True if the file was created successfully, false otherwise.</returns>
        public async Task<bool> CreateFileAsync(string filePath, string content)
        {
            try
            {
                // Create directory if it doesn't exist
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write content to file
                await File.WriteAllTextAsync(filePath, content);

                // Add file to project if in a project
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                
                // Try to find the project containing this file
                foreach (Project project in _dte.Solution.Projects)
                {
                    if (IsFileInProject(project, filePath))
                    {
                        project.ProjectItems.AddFromFile(filePath);
                        break;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all files in the current solution or project.
        /// </summary>
        /// <returns>Dictionary of file paths and their content.</returns>
        public async Task<Dictionary<string, string>> GetAllFilesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            var files = new Dictionary<string, string>();

            if (_dte.Solution == null)
            {
                return files;
            }

            // Get all projects in the solution
            foreach (Project project in _dte.Solution.Projects)
            {
                GetFilesInProject(project, files);
            }

            return files;
        }

        /// <summary>
        /// Determines the programming language from the file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The programming language name.</returns>
        private string DetermineLanguageFromExtension(string extension)
        {
            return extension switch
            {
                ".cs" => "csharp",
                ".vb" => "visualbasic",
                ".js" => "javascript",
                ".ts" => "typescript",
                ".html" => "html",
                ".css" => "css",
                ".py" => "python",
                ".java" => "java",
                ".cpp" => "cpp",
                ".h" => "cpp",
                _ => "plaintext"
            };
        }

        /// <summary>
        /// Recursively gets all files in a project.
        /// </summary>
        /// <param name="project">The project to scan.</param>
        /// <param name="files">Dictionary to populate with file paths and content.</param>
        private void GetFilesInProject(Project project, Dictionary<string, string> files)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (project == null || project.ProjectItems == null)
            {
                return;
            }

            foreach (ProjectItem item in project.ProjectItems)
            {
                // Add file or recurse into folder
                if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
                {
                    string filePath = item.Properties?.Item("FullPath")?.Value?.ToString();
                    if (!string.IsNullOrEmpty(filePath) && !files.ContainsKey(filePath) && File.Exists(filePath))
                    {
                        try
                        {
                            files[filePath] = File.ReadAllText(filePath);
                        }
                        catch
                        {
                            // Skip files that can't be read
                        }
                    }
                }
                else if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder)
                {
                    // Recurse into folder
                    if (item.ProjectItems != null)
                    {
                        foreach (ProjectItem subItem in item.ProjectItems)
                        {
                            GetFilesInProject(subItem.SubProject, files);
                        }
                    }
                }
                else if (item.SubProject != null)
                {
                    // Recurse into sub-project
                    GetFilesInProject(item.SubProject, files);
                }
            }
        }

        /// <summary>
        /// Recursively gets all files in a project item.
        /// </summary>
        /// <param name="item">The project item to scan.</param>
        /// <param name="files">Dictionary to populate with file paths and content.</param>
        private void GetFilesInProject(ProjectItem item, Dictionary<string, string> files)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            if (item == null)
            {
                return;
            }

            if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
            {
                string filePath = item.Properties?.Item("FullPath")?.Value?.ToString();
                if (!string.IsNullOrEmpty(filePath) && !files.ContainsKey(filePath) && File.Exists(filePath))
                {
                    try
                    {
                        files[filePath] = File.ReadAllText(filePath);
                    }
                    catch
                    {
                        // Skip files that can't be read
                    }
                }
            }
            else if (item.ProjectItems != null)
            {
                foreach (ProjectItem subItem in item.ProjectItems)
                {
                    GetFilesInProject(subItem, files);
                }
            }
        }

        /// <summary>
        /// Checks if a file is in a project's directory.
        /// </summary>
        /// <param name="project">The project to check.</param>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>True if the file is in the project directory, false otherwise.</returns>
        private bool IsFileInProject(Project project, string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            try
            {
                // Get project directory
                string projectDir = Path.GetDirectoryName(project.FullName);
                return !string.IsNullOrEmpty(projectDir) && filePath.StartsWith(projectDir, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}