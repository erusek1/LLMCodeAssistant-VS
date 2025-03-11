using System;
using System.Text;

namespace LLMCodeAssistant.Utilities
{
    /// <summary>
    /// Utility class for building prompts for the LLM.
    /// </summary>
    public class PromptBuilder
    {
        /// <summary>
        /// Builds a prompt for analyzing code.
        /// </summary>
        /// <param name="code">The code to analyze.</param>
        /// <param name="language">The programming language.</param>
        /// <returns>A prompt for code analysis.</returns>
        public string BuildAnalysisPrompt(string code, string language)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are an expert code reviewer specialized in identifying issues, bugs, and optimization opportunities. Analyze the following code and provide detailed feedback.");
            prompt.AppendLine();
            prompt.AppendLine($"Programming Language: {language}");
            prompt.AppendLine();
            prompt.AppendLine("Code to analyze:");
            prompt.AppendLine("```");
            prompt.AppendLine(code);
            prompt.AppendLine("```");
            prompt.AppendLine();
            prompt.AppendLine("Provide your analysis in this structured format:");
            prompt.AppendLine("## Summary\n[Brief overview of the code and its quality]");
            prompt.AppendLine("## Critical Issues\n[List any bugs, errors, or critical problems]");
            prompt.AppendLine("## Performance Concerns\n[Identify performance bottlenecks or inefficient code]");
            prompt.AppendLine("## Readability & Maintainability\n[Suggestions to improve code structure and readability]");
            prompt.AppendLine("## Security Considerations\n[Highlight any security vulnerabilities or risks]");
            prompt.AppendLine("## Improvement Recommendations\n[Specific actionable suggestions for improvement]");
            
            return prompt.ToString();
        }

        /// <summary>
        /// Builds a prompt for fixing code issues.
        /// </summary>
        /// <param name="code">The original code.</param>
        /// <param name="issues">The identified issues.</param>
        /// <param name="language">The programming language.</param>
        /// <returns>A prompt for generating code fixes.</returns>
        public string BuildFixPrompt(string code, string issues, string language)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are an expert programmer tasked with improving and fixing the following code based on identified issues. Provide the complete fixed code.");
            prompt.AppendLine();
            prompt.AppendLine($"Programming Language: {language}");
            prompt.AppendLine();
            prompt.AppendLine("Original code:");
            prompt.AppendLine("```");
            prompt.AppendLine(code);
            prompt.AppendLine("```");
            prompt.AppendLine();
            prompt.AppendLine("Issues to address:");
            prompt.AppendLine(issues);
            prompt.AppendLine();
            prompt.AppendLine("Please provide:");
            prompt.AppendLine("1. A summary of changes you're making to address the issues");
            prompt.AppendLine("2. The complete fixed code (not just the changes)");
            prompt.AppendLine("3. Comment your fixes within the code to explain important changes");
            prompt.AppendLine();
            prompt.AppendLine("Present the full fixed code inside a single code block marked with triple backticks.");
            
            return prompt.ToString();
        }

        /// <summary>
        /// Builds a prompt for generating code from a description.
        /// </summary>
        /// <param name="description">Description of the code to generate.</param>
        /// <param name="language">The programming language.</param>
        /// <returns>A prompt for code generation.</returns>
        public string BuildGenerationPrompt(string description, string language)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are an expert developer tasked with generating high-quality, production-ready code based on the following requirements.");
            prompt.AppendLine();
            prompt.AppendLine($"Programming Language: {language}");
            prompt.AppendLine();
            prompt.AppendLine("Requirements:");
            prompt.AppendLine(description);
            prompt.AppendLine();
            prompt.AppendLine("Please generate complete, well-structured code with the following characteristics:");
            prompt.AppendLine("- Include proper error handling");
            prompt.AppendLine("- Add comprehensive comments explaining complex sections");
            prompt.AppendLine("- Follow best practices and design patterns for this language");
            prompt.AppendLine("- Optimize for readability and maintainability");
            prompt.AppendLine("- Include necessary imports/dependencies");
            prompt.AppendLine();
            prompt.AppendLine("For each file, start with the file path, followed by the code in a code block with triple backticks.");
            
            return prompt.ToString();
        }

        /// <summary>
        /// Builds a prompt for generating a file structure from a description.
        /// </summary>
        /// <param name="description">Description of the program to generate.</param>
        /// <returns>A prompt for file structure generation.</returns>
        public string BuildFileStructurePrompt(string description)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are a software architecture expert tasked with designing a file structure for a program based on the following requirements.");
            prompt.AppendLine();
            prompt.AppendLine("Program Requirements:");
            prompt.AppendLine(description);
            prompt.AppendLine();
            prompt.AppendLine("Please provide a directory structure showing all necessary files with the following format:");
            prompt.AppendLine("```");
            prompt.AppendLine("project_root/");
            prompt.AppendLine("  ├── file1.ext         # Description of file1's purpose");
            prompt.AppendLine("  ├── directory/");
            prompt.AppendLine("  │   ├── file2.ext     # Description of file2's purpose");
            prompt.AppendLine("  │   └── file3.ext     # Description of file3's purpose");
            prompt.AppendLine("  └── file4.ext         # Description of file4's purpose");
            prompt.AppendLine("```");
            prompt.AppendLine();
            prompt.AppendLine("After the directory structure, provide a brief explanation of the overall architecture, including:");
            prompt.AppendLine("1. How components interact with each other");
            prompt.AppendLine("2. Key architectural patterns used");
            prompt.AppendLine("3. Data flow through the system");
            prompt.AppendLine("4. Any external dependencies required");
            
            return prompt.ToString();
        }
    }
}