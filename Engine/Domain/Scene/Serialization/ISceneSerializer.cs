using System.Threading.Tasks;

namespace Engine.Domain.Scene.Serialization;

/// <summary>
/// Interface for scene serialization operations
/// </summary>
public interface ISceneSerializer
{
    /// <summary>
    /// Load a scene from the specified file path
    /// </summary>
    /// <param name="filePath">Path to the scene file</param>
    /// <returns>The loaded scene</returns>
    Task<Scene> LoadSceneAsync(string filePath);

    /// <summary>
    /// Save a scene to the specified file path
    /// </summary>
    /// <param name="scene">Scene to save</param>
    /// <param name="filePath">Path where the scene should be saved</param>
    Task SaveSceneAsync(Scene scene, string filePath);

    /// <summary>
    /// Check if the serializer can handle the specified file format
    /// </summary>
    /// <param name="filePath">Path to check</param>
    /// <returns>True if the format is supported</returns>
    bool SupportsFormat(string filePath);

    /// <summary>
    /// Get the supported file extensions
    /// </summary>
    string[] SupportedExtensions { get; }
}