using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;

public class MartFilePlugin
{
    private readonly string _baseFolder;

    /// <summary>
    /// Initialize the plugin with the secure base folder.
    /// </summary>
    /// <param name="baseFolder">The base folder where file operations are allowed.</param>
    public MartFilePlugin(string baseFolder = @"C:\Git\Prive_GH\kql-demo")
    {
        // Normalize the base folder path
        _baseFolder = Path.GetFullPath(baseFolder);
    }

    /// <summary>
    /// Combines the base folder with a relative path and ensures the resulting absolute path is within the base folder.
    /// </summary>
    /// <param name="relativePath">The user-supplied relative file path.</param>
    /// <returns>The safe, combined absolute path.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the resolved path is outside the base folder.</exception>
    private string GetSafePath(string relativePath)
    {
        // Combine the base folder with the relative path and resolve it to an absolute path.
        var fullPath = Path.GetFullPath(Path.Combine(_baseFolder, relativePath));
        // Check if the resolved path starts with the base folder's full path.
        if (!fullPath.StartsWith(_baseFolder, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Access denied: invalid file path.");
        }
        return fullPath;
    }

    /// <summary>
    /// Reads the content of a file within the allowed folder.
    /// </summary>
    /// <param name="relativePath">The relative path to the file.</param>
    /// <returns>The file contents or an error message if file not found.</returns>
    [KernelFunction, Description("Reads the contents of a file within the allowed base folder.")]
    public string ReadFile(string relativePath)
    {
        var safePath = GetSafePath(relativePath);
        if (!File.Exists(safePath))
        {
            return $"Error: File '{relativePath}' not found.";
        }
        return File.ReadAllText(safePath);
    }

    /// <summary>
    /// Writes content to a file in the allowed folder. Creates the file if it doesn't exist.
    /// </summary>
    /// <param name="relativePath">The relative file path within the allowed folder.</param>
    /// <param name="content">The content to write to the file.</param>
    /// <returns>A success message with the file name.</returns>
    [KernelFunction, Description("Writes text content to a file in the allowed folder. Creates the file if it does not exist.")]
    public string WriteFile(string relativePath, string content)
    {
        var safePath = GetSafePath(relativePath);
        File.WriteAllText(safePath, content);
        return $"Successfully wrote to '{relativePath}'.";
    }

    /// <summary>
    /// Lists files in the allowed folder that match the specified search pattern.
    /// </summary>
    /// <param name="searchPattern">Optional search pattern. Defaults to "*.*" (all files).</param>
    /// <returns>A list of files (with their absolute paths) located in the allowed folder.</returns>
    [KernelFunction, Description("Lists files in the allowed folder matching the search pattern.")]
    public string ListFiles(string searchPattern = "*.*")
    {
        // Get files only in the base folder (top-level only)
        var files = Directory.GetFiles(_baseFolder, searchPattern, SearchOption.AllDirectories);
        return string.Join(Environment.NewLine, files);
    }

    /// <summary>
    /// Searches for files by file name in the allowed folder.
    /// </summary>
    /// <param name="fileName">The file name (or partial name) to search for.</param>
    /// <returns>A newline-separated list of matching files.</returns>
    [KernelFunction, Description("Searches for a file by name in the allowed folder.")]
    public string SearchFile(string fileName)
    {
        // This method will search only the top level of _baseFolder
        var files = Directory.GetFiles(_baseFolder, $"*{fileName}*", SearchOption.TopDirectoryOnly)
                             .Select(file => Path.GetFileName(file));
        if (!files.Any())
        {
            return $"No files matching '{fileName}' were found.";
        }
        return string.Join(Environment.NewLine, files);
    }
}
