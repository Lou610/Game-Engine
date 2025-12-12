using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Domain.Scene.Serialization;

/// <summary>
/// File-based scene serializer that manages multiple formats and provides file system operations
/// </summary>
public class FileSceneSerializer : ISceneSerializer
{
    private readonly Dictionary<string, ISceneSerializer> _formatSerializers;
    private readonly List<string> _supportedExtensions;

    /// <summary>
    /// All supported file extensions from registered serializers
    /// </summary>
    public string[] SupportedExtensions => _supportedExtensions.ToArray();

    public FileSceneSerializer()
    {
        _formatSerializers = new Dictionary<string, ISceneSerializer>();
        _supportedExtensions = new List<string>();
        
        // Register default serializers
        RegisterSerializer(new JsonSceneSerializer());
    }

    /// <summary>
    /// Register a serializer for specific formats
    /// </summary>
    /// <param name="serializer">Serializer to register</param>
    public void RegisterSerializer(ISceneSerializer serializer)
    {
        foreach (var extension in serializer.SupportedExtensions)
        {
            _formatSerializers[extension.ToLowerInvariant()] = serializer;
            if (!_supportedExtensions.Contains(extension))
            {
                _supportedExtensions.Add(extension);
            }
        }
    }

    /// <summary>
    /// Load scene from file using appropriate serializer
    /// </summary>
    public async Task<Scene> LoadSceneAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Scene file not found: {filePath}");

        var serializer = GetSerializerForFile(filePath);
        if (serializer == null)
            throw new NotSupportedException($"No serializer found for file format: {Path.GetExtension(filePath)}");

        try
        {
            var scene = await serializer.LoadSceneAsync(filePath);
            
            // Store file path metadata (Scene.Metadata needs to be mutable)
            // This will need to be handled at the Scene level with proper setter methods
            
            return scene;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load scene from '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Save scene to file using appropriate serializer
    /// </summary>
    public async Task SaveSceneAsync(Scene scene, string filePath)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var serializer = GetSerializerForFile(filePath);
        if (serializer == null)
            throw new NotSupportedException($"No serializer found for file format: {Path.GetExtension(filePath)}");

        try
        {
            // Create backup if file exists
            await CreateBackupIfNeeded(filePath);
            
            // Update scene metadata (handled by Scene class internally)
            // The Scene class should handle metadata updates and ModifiedAt internally

            await serializer.SaveSceneAsync(scene, filePath);
        }
        catch (Exception ex)
        {
            // Attempt to restore backup on failure
            await RestoreBackupIfNeeded(filePath);
            throw new InvalidOperationException($"Failed to save scene to '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Check if format is supported
    /// </summary>
    public bool SupportsFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        return !string.IsNullOrEmpty(extension) && _formatSerializers.ContainsKey(extension);
    }

    /// <summary>
    /// Get scene metadata without fully loading the scene
    /// </summary>
    public async Task<SceneMetadata> GetSceneMetadataAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Scene file not found: {filePath}");

        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        if (extension == ".scene" || extension == ".json")
        {
            // For JSON-based scenes, we can read just the metadata section
            return await GetJsonSceneMetadataAsync(filePath);
        }

        // For other formats, we need to load the full scene
        var scene = await LoadSceneAsync(filePath);
        return new SceneMetadata
        {
            Id = scene.Id.Value,
            Name = scene.Name,
            Description = scene.Metadata.TryGetValue("Description", out var desc) ? desc.ToString() ?? "" : "",
            Author = scene.Metadata.TryGetValue("Author", out var author) ? author.ToString() ?? "" : "",
            Tags = scene.Metadata.TryGetValue("Tags", out var tags) && tags is List<string> tagList 
                ? tagList 
                : new List<string>()
        };
    }

    /// <summary>
    /// List all scene files in a directory
    /// </summary>
    public async Task<List<SceneFileInfo>> ListScenesInDirectoryAsync(string directoryPath, bool recursive = false)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var sceneFiles = new List<SceneFileInfo>();
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var extension in SupportedExtensions)
        {
            var pattern = $"*{extension}";
            var files = Directory.GetFiles(directoryPath, pattern, searchOption);
            
            foreach (var file in files)
            {
                try
                {
                    var metadata = await GetSceneMetadataAsync(file);
                    var fileInfo = new FileInfo(file);
                    
                    sceneFiles.Add(new SceneFileInfo
                    {
                        FilePath = file,
                        Metadata = metadata,
                        FileSize = fileInfo.Length,
                        CreatedAt = fileInfo.CreationTimeUtc,
                        ModifiedAt = fileInfo.LastWriteTimeUtc
                    });
                }
                catch
                {
                    // Skip files that can't be read
                }
            }
        }

        return sceneFiles.OrderBy(f => f.Metadata.Name).ToList();
    }

    /// <summary>
    /// Validate scene file integrity
    /// </summary>
    public async Task<SceneValidationResult> ValidateSceneAsync(string filePath)
    {
        var result = new SceneValidationResult { IsValid = true, Errors = new List<string>() };

        try
        {
            if (!File.Exists(filePath))
            {
                result.IsValid = false;
                result.Errors.Add("File not found");
                return result;
            }

            if (!SupportsFormat(filePath))
            {
                result.IsValid = false;
                result.Errors.Add($"Unsupported file format: {Path.GetExtension(filePath)}");
                return result;
            }

            // Try to load the scene
            var scene = await LoadSceneAsync(filePath);
            
            // Basic validation checks
            if (string.IsNullOrEmpty(scene.Name))
            {
                result.Errors.Add("Scene name is empty");
            }

            if (scene.Entities.EntityCount == 0)
            {
                result.Errors.Add("Scene contains no entities");
            }

            result.IsValid = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return result;
    }

    #region Private Methods

    private ISceneSerializer? GetSerializerForFile(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
            return null;

        _formatSerializers.TryGetValue(extension, out var serializer);
        return serializer;
    }

    private async Task<SceneMetadata> GetJsonSceneMetadataAsync(string filePath)
    {
        // Read only the first part of the file to extract metadata
        var json = await File.ReadAllTextAsync(filePath);
        
        // Simple approach: deserialize just enough to get metadata
        // In a production system, you'd use a streaming JSON parser
        try
        {
            var document = System.Text.Json.JsonDocument.Parse(json);
            var root = document.RootElement;
            
            if (root.TryGetProperty("metadata", out var metadataElement))
            {
                return System.Text.Json.JsonSerializer.Deserialize<SceneMetadata>(metadataElement.GetRawText()) 
                    ?? new SceneMetadata();
            }
        }
        catch
        {
            // Fall back to empty metadata if parsing fails
        }

        return new SceneMetadata();
    }

    private async Task CreateBackupIfNeeded(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var backupPath = filePath + ".backup";
        try
        {
            await File.WriteAllBytesAsync(backupPath, await File.ReadAllBytesAsync(filePath));
        }
        catch
        {
            // Backup creation failed, but continue with save operation
        }
    }

    private async Task RestoreBackupIfNeeded(string filePath)
    {
        var backupPath = filePath + ".backup";
        if (!File.Exists(backupPath))
            return;

        try
        {
            await File.WriteAllBytesAsync(filePath, await File.ReadAllBytesAsync(backupPath));
            File.Delete(backupPath);
        }
        catch
        {
            // Backup restoration failed
        }
    }

    #endregion
}

/// <summary>
/// Information about a scene file
/// </summary>
public class SceneFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public SceneMetadata Metadata { get; set; } = new();
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}

/// <summary>
/// Scene validation result
/// </summary>
public class SceneValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Extension methods for scene serialization
/// </summary>
public static class SceneSerializationExtensions
{
    /// <summary>
    /// Save scene with automatic format detection based on file extension
    /// </summary>
    public static async Task SaveAsync(this Scene scene, string filePath, ISceneSerializer? serializer = null)
    {
        serializer ??= new FileSceneSerializer();
        await serializer.SaveSceneAsync(scene, filePath);
    }

    /// <summary>
    /// Load scene with automatic format detection
    /// </summary>
    public static async Task<Scene> LoadAsync(string filePath, ISceneSerializer? serializer = null)
    {
        serializer ??= new FileSceneSerializer();
        return await serializer.LoadSceneAsync(filePath);
    }

    /// <summary>
    /// Get default scene file extension
    /// </summary>
    public static string GetDefaultExtension() => ".scene";

    /// <summary>
    /// Generate a unique scene file name
    /// </summary>
    public static string GenerateSceneFileName(string baseName, string directory = "")
    {
        var fileName = $"{baseName}{GetDefaultExtension()}";
        
        if (string.IsNullOrEmpty(directory))
            return fileName;

        var fullPath = Path.Combine(directory, fileName);
        var counter = 1;
        
        while (File.Exists(fullPath))
        {
            fileName = $"{baseName}_{counter}{GetDefaultExtension()}";
            fullPath = Path.Combine(directory, fileName);
            counter++;
        }

        return fullPath;
    }
}