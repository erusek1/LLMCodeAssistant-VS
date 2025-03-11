# LLM Code Assistant for Visual Studio

A Visual Studio extension that leverages local LLM capabilities through Ollama to provide code analysis, fixing, and generation features.

## Features

- **Code Analysis**: Analyze your code for issues, bugs, and improvement opportunities
- **Code Fixing**: Generate and apply fixes for identified issues
- **Code Generation**: Generate new code based on descriptions through a chat interface
- **Local LLM Integration**: Uses Ollama with Llama Code 34B for privacy and performance

## Prerequisites

1. **Ollama**: You need to have [Ollama](https://ollama.ai/) installed and running on your machine
   - Download from: https://ollama.ai/download

2. **Llama Code 34B model**: Install the model with:
   ```
   ollama pull codellama:34b
   ```

3. **Visual Studio**: This extension is compatible with Visual Studio 2022

## Installation

1. Download the VSIX package from the Releases page
2. Double-click the VSIX file to install it in Visual Studio
3. Restart Visual Studio

## Usage

### Configuration

1. Open Visual Studio
2. Go to Tools > LLM Assistant Settings
3. Configure the Ollama endpoint (default: http://localhost:11434)
4. Configure the model name (default: codellama:34b)
5. Click "Test Connection" to verify the setup
6. Click "Save" to apply the settings

### Analyze Code

1. Open a code file in Visual Studio
2. Go to Tools > Analyze Code
3. The extension will analyze the code and display the results

### Fix Code

1. After analyzing code, go to Tools > Fix Code
2. The extension will generate fixes based on the analysis
3. Review the fixes and click "Apply Fixes" to apply them

### Generate Code

1. Go to Tools > Generate Code
2. Enter a description of the code you want to generate
3. Chat with the assistant to refine your requirements
4. The generated code will be displayed and can be saved to files

## Building from Source

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the solution
5. The VSIX package will be in the bin/Debug or bin/Release folder

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgements

- [CodeLlama](https://github.com/facebookresearch/codellama) - The underlying LLM model
- [Ollama](https://ollama.ai/) - Local LLM runtime
- Visual Studio SDK
