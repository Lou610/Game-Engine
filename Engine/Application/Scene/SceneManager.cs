using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Engine.Domain.Scene;
using Engine.Domain.Scene.ValueObjects;
using Engine.Domain.Scene.Serialization;
using Engine.Domain.ECS;
using Engine.Domain.Core.Services;
using Engine.Infrastructure.Logging;
using SceneEntity = Engine.Domain.Scene.Scene;

namespace Engine.Application.Scene;

/// <summary>
/// Service for managing scene lifecycle and operations
/// </summary>
public class SceneManager : IService, IDisposable
{
    private readonly Dictionary<string, SceneEntity> _loadedScenes;
    private SceneEntity? _activeScene;
    private readonly Logger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISceneSerializer _serializer;
    private readonly object _lock = new object();
    private bool _disposed = false;

    /// <summary>
    /// Event raised when the active scene changes
    /// </summary>
    public event Action<SceneEntity?, SceneEntity?>? ActiveSceneChanged;

    /// <summary>
    /// Event raised when a scene is loaded
    /// </summary>
    public event Action<SceneEntity>? SceneLoaded;

    /// <summary>
    /// Event raised when a scene is unloaded
    /// </summary>
    public event Action<SceneEntity>? SceneUnloaded;

    /// <summary>
    /// Gets the currently active scene
    /// </summary>
    public SceneEntity? ActiveScene 
    { 
        get 
        { 
            lock (_lock) 
            { 
                return _activeScene; 
            } 
        } 
    }

    /// <summary>
    /// Gets all loaded scenes
    /// </summary>
    public IReadOnlyCollection<SceneEntity> LoadedScenes 
    { 
        get 
        { 
            lock (_lock) 
            { 
                return _loadedScenes.Values.ToList(); 
            } 
        } 
    }

    /// <summary>
    /// Gets the number of loaded scenes
    /// </summary>
    public int LoadedSceneCount 
    { 
        get 
        { 
            lock (_lock) 
            { 
                return _loadedScenes.Count; 
            } 
        } 
    }

    public SceneManager(Logger logger, IServiceProvider serviceProvider, ISceneSerializer? serializer = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _serializer = serializer ?? new FileSceneSerializer();
        _loadedScenes = new Dictionary<string, SceneEntity>();
    }

    #region Scene Creation

    /// <summary>
    /// Creates a new scene with the specified name and settings
    /// </summary>
    /// <param name="name">Scene name</param>
    /// <param name="settings">Scene settings (optional)</param>
    /// <returns>The created scene</returns>
    public SceneEntity CreateScene(string name, SceneSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Scene name cannot be null or empty", nameof(name));

        lock (_lock)
        {
            if (_loadedScenes.ContainsKey(name))
                throw new InvalidOperationException($"Scene with name '{name}' already exists");

            var world = new World();
            var scene = new SceneEntity(name, world, settings ?? new SceneSettings());
            
            _loadedScenes[name] = scene;
            _logger.Info($"Scene created: {name}");
            SceneLoaded?.Invoke(scene);

            return scene;
        }
    }

    /// <summary>
    /// Creates a new scene and sets it as active
    /// </summary>
    /// <param name="name">Scene name</param>
    /// <param name="settings">Scene settings (optional)</param>
    /// <returns>The created scene</returns>
    public SceneEntity CreateAndActivateScene(string name, SceneSettings? settings = null)
    {
        var scene = CreateScene(name, settings);
        SetActiveScene(scene);
        return scene;
    }

    #endregion

    #region Scene Loading/Unloading (Enhanced from original)

    /// <summary>
    /// Loads a scene from the specified path
    /// </summary>
    /// <param name="scenePath">Path to the scene file</param>
    /// <returns>The loaded scene</returns>
    public async Task<SceneEntity> LoadSceneAsync(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
            throw new ArgumentException("Scene path cannot be null or empty", nameof(scenePath));

        try
        {
            var scene = await _serializer.LoadSceneAsync(scenePath);
            
            lock (_lock)
            {
                _loadedScenes[scene.Name] = scene;
            }
            
            _logger.Info($"Scene loaded from path: {scenePath}");
            SceneLoaded?.Invoke(scene);
            return scene;
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to load scene from '{scenePath}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads a scene (backward compatibility with original method)
    /// </summary>
    public void LoadScene(SceneEntity scene)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        lock (_lock)
        {
            _loadedScenes[scene.Name] = scene;
            _activeScene = scene;
            _logger.Info($"Scene loaded: {scene.Name}");
            SceneLoaded?.Invoke(scene);
        }
    }

    /// <summary>
    /// Loads a scene additively (keeping existing scenes loaded)
    /// </summary>
    /// <param name="scenePath">Path to the scene file</param>
    /// <returns>The loaded scene</returns>
    public async Task<SceneEntity> LoadSceneAdditiveAsync(string scenePath)
    {
        var scene = await LoadSceneAsync(scenePath);
        // Scene is already loaded by LoadSceneAsync, no need to set as active
        return scene;
    }

    /// <summary>
    /// Unloads the specified scene
    /// </summary>
    /// <param name="scene">Scene to unload</param>
    public void UnloadScene(SceneEntity scene)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        lock (_lock)
        {
            if (!_loadedScenes.ContainsValue(scene))
                return;

            var sceneEntry = _loadedScenes.FirstOrDefault(kvp => kvp.Value == scene);
            if (sceneEntry.Key == null)
                return;

            // If this is the active scene, clear it
            if (_activeScene == scene)
            {
                var previousScene = _activeScene;
                _activeScene = null;
                ActiveSceneChanged?.Invoke(previousScene, null);
            }

            _loadedScenes.Remove(sceneEntry.Key);
            scene.Dispose();
            _logger.Info($"Scene unloaded: {scene.Name}");
            SceneUnloaded?.Invoke(scene);
        }
    }

    /// <summary>
    /// Unloads the scene with the specified ID (backward compatibility)
    /// </summary>
    /// <param name="sceneId">ID of the scene to unload</param>
    public void UnloadScene(SceneId sceneId)
    {
        lock (_lock)
        {
            var scene = _loadedScenes.Values.FirstOrDefault(s => s.Id.Value == sceneId.Value);
            if (scene != null)
            {
                UnloadScene(scene);
            }
        }
    }

    /// <summary>
    /// Unloads the scene with the specified name
    /// </summary>
    /// <param name="sceneName">Name of the scene to unload</param>
    public void UnloadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        lock (_lock)
        {
            if (_loadedScenes.TryGetValue(sceneName, out var scene))
            {
                UnloadScene(scene);
            }
        }
    }

    /// <summary>
    /// Unloads all scenes except the active one
    /// </summary>
    public void UnloadAllScenesExceptActive()
    {
        lock (_lock)
        {
            var scenesToUnload = _loadedScenes.Values
                .Where(s => s != _activeScene)
                .ToList();

            foreach (var scene in scenesToUnload)
            {
                UnloadScene(scene);
            }
        }
    }

    /// <summary>
    /// Unloads all scenes
    /// </summary>
    public void UnloadAllScenes()
    {
        lock (_lock)
        {
            var scenesToUnload = _loadedScenes.Values.ToList();

            foreach (var scene in scenesToUnload)
            {
                UnloadScene(scene);
            }

            _activeScene = null;
        }
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// Sets the active scene
    /// </summary>
    /// <param name="scene">Scene to set as active (can be null)</param>
    public void SetActiveScene(SceneEntity? scene)
    {
        SceneEntity? previousScene;

        lock (_lock)
        {
            if (scene != null && !_loadedScenes.ContainsValue(scene))
                throw new ArgumentException("Scene must be loaded before it can be set as active", nameof(scene));

            previousScene = _activeScene;
            _activeScene = scene;
        }

        if (previousScene != _activeScene)
        {
            _logger.Info($"Active scene changed from '{previousScene?.Name}' to '{_activeScene?.Name}'");
            ActiveSceneChanged?.Invoke(previousScene, _activeScene);
        }
    }

    /// <summary>
    /// Sets the active scene by name
    /// </summary>
    /// <param name="sceneName">Name of the scene to set as active</param>
    public void SetActiveScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            SetActiveScene((SceneEntity?)null);
            return;
        }

        lock (_lock)
        {
            if (_loadedScenes.TryGetValue(sceneName, out var scene))
            {
                SetActiveScene(scene);
            }
            else
            {
                throw new ArgumentException($"No loaded scene found with name '{sceneName}'", nameof(sceneName));
            }
        }
    }

    /// <summary>
    /// Gets the currently active scene (backward compatibility)
    /// </summary>
    /// <returns>The active scene</returns>
    public SceneEntity? GetActiveScene() => ActiveScene;

    /// <summary>
    /// Gets a loaded scene by name
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    /// <returns>The scene, or null if not found</returns>
    public SceneEntity? GetScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return null;

        lock (_lock)
        {
            _loadedScenes.TryGetValue(sceneName, out var scene);
            return scene;
        }
    }

    /// <summary>
    /// Checks if a scene with the specified name is loaded
    /// </summary>
    /// <param name="sceneName">Name of the scene</param>
    /// <returns>True if the scene is loaded</returns>
    public bool IsSceneLoaded(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        lock (_lock)
        {
            return _loadedScenes.ContainsKey(sceneName);
        }
    }

    #endregion

    #region Scene Operations

    /// <summary>
    /// Updates all loaded scenes
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update</param>
    public void Update(float deltaTime)
    {
        List<SceneEntity> scenesToUpdate;

        lock (_lock)
        {
            scenesToUpdate = _loadedScenes.Values.ToList();
        }

        // Update scenes outside of lock to avoid blocking
        foreach (var scene in scenesToUpdate)
        {
            try
            {
                scene.Update(deltaTime);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating scene '{scene.Name}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Saves the specified scene to disk
    /// </summary>
    /// <param name="scene">Scene to save</param>
    /// <param name="path">Path to save the scene to</param>
    public async Task SaveSceneAsync(SceneEntity scene, string path)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        try
        {
            await _serializer.SaveSceneAsync(scene, path);
            _logger.Info($"Scene '{scene.Name}' saved to '{path}'");
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to save scene '{scene.Name}' to '{path}': {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Saves the active scene to disk
    /// </summary>
    /// <param name="path">Path to save the scene to</param>
    public async Task SaveActiveSceneAsync(string path)
    {
        var activeScene = ActiveScene;
        if (activeScene == null)
            throw new InvalidOperationException("No active scene to save");

        await SaveSceneAsync(activeScene, path);
    }

    /// <summary>
    /// Duplicates a scene with a new name
    /// </summary>
    /// <param name="sourceScene">Scene to duplicate</param>
    /// <param name="newName">Name for the duplicated scene</param>
    /// <returns>The duplicated scene</returns>
    public async Task<SceneEntity> DuplicateScene(SceneEntity sourceScene, string newName)
    {
        if (sourceScene == null)
            throw new ArgumentNullException(nameof(sourceScene));
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New scene name cannot be null or empty", nameof(newName));

        // Check if name already exists
        lock (_lock)
        {
            if (_loadedScenes.ContainsKey(newName))
                throw new InvalidOperationException($"Scene with name '{newName}' already exists");
        }

        // Create a deep copy by serializing and deserializing (outside lock)
        SceneEntity duplicatedScene;
        try
        {
            var tempPath = System.IO.Path.GetTempFileName() + ".scene";
            
            // Async operations outside of lock
            await _serializer.SaveSceneAsync(sourceScene, tempPath);
            duplicatedScene = await _serializer.LoadSceneAsync(tempPath);
            
            // Clean up temp file
            if (System.IO.File.Exists(tempPath))
            {
                System.IO.File.Delete(tempPath);
            }
            
            // Update the duplicated scene's name and ID
            duplicatedScene.Name = newName;
            duplicatedScene.Id = new SceneId(newName);
            // ModifiedAt will be set internally by the Scene class
            
            // Add to loaded scenes (with lock)
            lock (_lock)
            {
                _loadedScenes[newName] = duplicatedScene;
            }
            
            _logger.Info($"Scene '{sourceScene.Name}' duplicated as '{newName}'");
            return duplicatedScene;
        }
        catch (Exception ex)
            {
                _logger.Error($"Failed to duplicate scene '{sourceScene.Name}': {ex.Message}");
                
                // Fallback to simple settings copy
                var newSettings = new SceneSettings
                {
                    BackgroundColor = sourceScene.Settings.BackgroundColor,
                    AmbientLight = sourceScene.Settings.AmbientLight,
                    Fog = sourceScene.Settings.Fog,
                    Physics = sourceScene.Settings.Physics,
                    Rendering = sourceScene.Settings.Rendering,
                    Audio = sourceScene.Settings.Audio,
                    AutoSave = sourceScene.Settings.AutoSave,
                    AutoSaveInterval = sourceScene.Settings.AutoSaveInterval,
                    MaxEntities = sourceScene.Settings.MaxEntities,
                    UpdateFrequency = sourceScene.Settings.UpdateFrequency
                };
                
                var fallbackScene = CreateScene(newName, newSettings);
                _logger.Info($"Scene '{sourceScene.Name}' duplicated as '{newName}' (fallback method)");
                return fallbackScene;
            }
    }

    #endregion

    #region Service Implementation

    public void Initialize()
    {
        _logger.Info("SceneManager initialized");
    }

    public void Shutdown()
    {
        _logger.Info("SceneManager shutting down");
        UnloadAllScenes();
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            Shutdown();
            _disposed = true;
        }
    }

    /// <summary>
    /// Export scene metadata without loading the full scene
    /// </summary>
    /// <param name="scenePath">Path to the scene file</param>
    /// <returns>Scene metadata</returns>
    public async Task<SceneMetadata> GetSceneMetadataAsync(string scenePath)
    {
        if (_serializer is FileSceneSerializer fileSerializer)
        {
            return await fileSerializer.GetSceneMetadataAsync(scenePath);
        }
        
        // Fallback: load the full scene to get metadata
        var scene = await LoadSceneAsync(scenePath);
        return new SceneMetadata
        {
            Id = scene.Id.Value,
            Name = scene.Name,
            Description = scene.Metadata.TryGetValue("Description", out var desc) ? desc.ToString() ?? "" : "",
            Author = scene.Metadata.TryGetValue("Author", out var author) ? author.ToString() ?? "" : ""
        };
    }

    /// <summary>
    /// Validate a scene file
    /// </summary>
    /// <param name="scenePath">Path to the scene file</param>
    /// <returns>Validation result</returns>
    public async Task<SceneValidationResult> ValidateSceneAsync(string scenePath)
    {
        if (_serializer is FileSceneSerializer fileSerializer)
        {
            return await fileSerializer.ValidateSceneAsync(scenePath);
        }

        // Basic validation
        var result = new SceneValidationResult { IsValid = true, Errors = new List<string>() };
        
        try
        {
            var scene = await LoadSceneAsync(scenePath);
            if (string.IsNullOrEmpty(scene.Name))
            {
                result.Errors.Add("Scene name is empty");
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    /// <summary>
    /// Auto-save the active scene if auto-save is enabled
    /// </summary>
    public async Task AutoSaveActiveSceneAsync()
    {
        var activeScene = ActiveScene;
        if (activeScene?.Settings.AutoSave == true && 
            activeScene.Metadata.TryGetValue("FilePath", out var filePathObj) &&
            filePathObj is string filePath &&
            !string.IsNullOrEmpty(filePath))
        {
            try
            {
                var lastSaved = activeScene.Metadata.TryGetValue("LastSaved", out var lastSavedObj) && 
                               lastSavedObj is DateTime lastSavedTime ? lastSavedTime : DateTime.MinValue;
                
                var timeSinceLastSave = DateTime.UtcNow - lastSaved;
                if (timeSinceLastSave.TotalSeconds >= activeScene.Settings.AutoSaveInterval)
                {
                    await SaveSceneAsync(activeScene, filePath);
                    _logger.Info($"Auto-saved scene '{activeScene.Name}'");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Auto-save failed for scene '{activeScene.Name}': {ex.Message}");
            }
        }
    }

    #endregion
}

/// <summary>
/// Extension methods for SceneManager
/// </summary>
public static class SceneManagerExtensions
{
    /// <summary>
    /// Creates a new empty scene for testing or prototyping
    /// </summary>
    /// <param name="sceneManager">Scene manager instance</param>
    /// <param name="name">Scene name (optional, will generate if not provided)</param>
    /// <returns>The created scene</returns>
    public static SceneEntity CreateEmptyScene(this SceneManager sceneManager, string? name = null)
    {
        name ??= $"EmptyScene_{Guid.NewGuid():N}";
        return sceneManager.CreateScene(name);
    }

    /// <summary>
    /// Gets the active scene or throws if none is active
    /// </summary>
    /// <param name="sceneManager">Scene manager instance</param>
    /// <returns>The active scene</returns>
    /// <exception cref="InvalidOperationException">No active scene</exception>
    public static SceneEntity GetActiveSceneOrThrow(this SceneManager sceneManager)
    {
        return sceneManager.ActiveScene ?? throw new InvalidOperationException("No active scene");
    }

    /// <summary>
    /// Switches to a scene, creating it if it doesn't exist
    /// </summary>
    /// <param name="sceneManager">Scene manager instance</param>
    /// <param name="sceneName">Name of the scene</param>
    /// <param name="settings">Settings for the scene if it needs to be created</param>
    /// <returns>The scene that was switched to</returns>
    public static SceneEntity SwitchToScene(this SceneManager sceneManager, string sceneName, SceneSettings? settings = null)
    {
        var scene = sceneManager.GetScene(sceneName);
        if (scene == null)
        {
            scene = sceneManager.CreateScene(sceneName, settings);
        }
        
        sceneManager.SetActiveScene(scene);
        return scene;
    }
}

