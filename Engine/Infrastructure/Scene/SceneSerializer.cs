using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Engine.Domain.Scene;
using Engine.Domain.Scene.Serialization;
using SceneEntity = Engine.Domain.Scene.Scene;

namespace Engine.Infrastructure.Scene;

/// <summary>
/// JSON/YAML serialization implementation
/// </summary>
public class SceneSerializer : ISceneSerializer
{
    public string[] SupportedExtensions => new[] { ".scene", ".json" };

    public async Task<SceneEntity> LoadSceneAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Scene file not found: {filePath}");
        }

        var json = await File.ReadAllTextAsync(filePath);
        var scene = JsonConvert.DeserializeObject<SceneEntity>(json);
        
        if (scene == null)
        {
            throw new InvalidDataException($"Failed to deserialize scene from: {filePath}");
        }

        return scene;
    }

    public async Task SaveSceneAsync(SceneEntity scene, string filePath)
    {
        var json = JsonConvert.SerializeObject(scene, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json);
    }

    public bool SupportsFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension == ".scene" || extension == ".json";
    }

    // Legacy synchronous methods for backward compatibility
    public void Serialize(SceneEntity scene, string filePath)
    {
        var json = JsonConvert.SerializeObject(scene, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public SceneEntity? Deserialize(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<SceneEntity>(json);
    }

    // Additional methods that SceneManager expects
    public async Task<Engine.Domain.Scene.Serialization.SceneMetadata> GetSceneMetadataAsync(string filePath)
    {
        // For now, we need to load the scene to get metadata
        // In a more optimized implementation, we'd parse just the header
        var scene = await LoadSceneAsync(filePath);
        return new Engine.Domain.Scene.Serialization.SceneMetadata
        {
            Name = scene.Name,
            Id = scene.Id.Value,
            Description = scene.Metadata.ContainsKey("description") ? scene.Metadata["description"].ToString() ?? "" : ""
        };
    }

    public async Task<Engine.Domain.Scene.Serialization.SceneValidationResult> ValidateSceneAsync(string filePath)
    {
        var result = new Engine.Domain.Scene.Serialization.SceneValidationResult 
        { 
            IsValid = true, 
            Errors = new List<string>() 
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.IsValid = false;
                result.Errors.Add($"Scene file does not exist: {filePath}");
                return result;
            }

            var scene = await LoadSceneAsync(filePath);
            
            // Basic validation
            if (string.IsNullOrEmpty(scene.Name))
            {
                result.IsValid = false;
                result.Errors.Add("Scene name is required");
            }

            // Additional validation can be added here
        }
        catch (System.Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Failed to load scene: {ex.Message}");
        }

        return result;
    }
}

