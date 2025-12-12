using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Components;
using Engine.Domain.Scene.ValueObjects;

namespace Engine.Domain.Scene.Serialization;

/// <summary>
/// JSON-based scene serializer
/// </summary>
public class JsonSceneSerializer : ISceneSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly Dictionary<Type, IComponentSerializer> _componentSerializers;

    /// <summary>
    /// Supported file extensions
    /// </summary>
    public string[] SupportedExtensions => new[] { ".scene", ".json" };

    public JsonSceneSerializer()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(),
                new Vector3Converter(),
                new Vector4Converter(),
                new QuaternionConverter()
            }
        };

        _componentSerializers = new Dictionary<Type, IComponentSerializer>();
        RegisterDefaultComponentSerializers();
    }

    /// <summary>
    /// Load scene from JSON file
    /// </summary>
    public async Task<Scene> LoadSceneAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Scene file not found: {filePath}");

        if (!SupportsFormat(filePath))
            throw new NotSupportedException($"Unsupported file format: {Path.GetExtension(filePath)}");

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var serializedScene = JsonSerializer.Deserialize<SerializedScene>(json, _options);

            if (serializedScene == null)
                throw new InvalidOperationException("Failed to deserialize scene data");

            return DeserializeScene(serializedScene);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load scene from '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Save scene to JSON file
    /// </summary>
    public async Task SaveSceneAsync(Scene scene, string filePath)
    {
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));

        if (!SupportsFormat(filePath))
            throw new NotSupportedException($"Unsupported file format: {Path.GetExtension(filePath)}");

        try
        {
            var serializedScene = SerializeScene(scene);
            var json = JsonSerializer.Serialize(serializedScene, _options);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save scene to '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Check if the format is supported
    /// </summary>
    public bool SupportsFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    #region Scene Serialization

    private SerializedScene SerializeScene(Scene scene)
    {
        var serializedScene = new SerializedScene
        {
            Metadata = new SceneMetadata
            {
                Id = scene.Id.Value,
                Name = scene.Name,
                Description = scene.Metadata.TryGetValue("Description", out var desc) ? desc.ToString() ?? "" : "",
                Author = scene.Metadata.TryGetValue("Author", out var author) ? author.ToString() ?? "" : "",
                Tags = scene.Metadata.TryGetValue("Tags", out var tags) && tags is List<string> tagList 
                    ? tagList 
                    : new List<string>()
            },
            Settings = SerializeSettings(scene.Settings),
            Entities = SerializeEntities(scene.Entities),
            SceneGraph = SerializeSceneGraph(scene.SceneGraph),
            CustomProperties = new Dictionary<string, object>(scene.Metadata),
            CreatedAt = scene.CreatedAt,
            ModifiedAt = scene.ModifiedAt,
            Version = "1.0.0"
        };

        return serializedScene;
    }

    private SceneSettingsData SerializeSettings(SceneSettings settings)
    {
        return new SceneSettingsData
        {
            BackgroundColor = settings.BackgroundColor,
            AmbientLight = settings.AmbientLight,
            Fog = new FogSettingsData
            {
                Enabled = settings.Fog.Enabled,
                Color = settings.Fog.Color,
                Density = settings.Fog.Density,
                Start = settings.Fog.Start,
                End = settings.Fog.End,
                Mode = settings.Fog.Mode.ToString()
            },
            Physics = new PhysicsSettingsData
            {
                Enabled = settings.Physics.Enabled,
                Gravity = settings.Physics.Gravity,
                TimeStep = settings.Physics.TimeStep,
                MaxSubSteps = settings.Physics.MaxSubSteps,
                SolverIterations = settings.Physics.SolverIterations,
                ContinuousCollisionDetection = settings.Physics.ContinuousCollisionDetection
            },
            Rendering = new RenderingSettingsData
            {
                ShadowsEnabled = settings.Rendering.ShadowsEnabled,
                ShadowQuality = settings.Rendering.ShadowQuality,
                ShadowDistance = settings.Rendering.ShadowDistance,
                PostProcessingEnabled = settings.Rendering.PostProcessingEnabled,
                AntiAliasing = settings.Rendering.AntiAliasing.ToString(),
                VSync = settings.Rendering.VSync,
                TargetFrameRate = settings.Rendering.TargetFrameRate,
                LodEnabled = settings.Rendering.LodEnabled,
                FrustumCullingEnabled = settings.Rendering.FrustumCullingEnabled,
                OcclusionCullingEnabled = settings.Rendering.OcclusionCullingEnabled
            },
            Audio = new AudioSettingsData
            {
                MasterVolume = settings.Audio.MasterVolume,
                SpatialAudioEnabled = settings.Audio.SpatialAudioEnabled,
                DopplerEnabled = settings.Audio.DopplerEnabled,
                SpeedOfSound = settings.Audio.SpeedOfSound,
                MaxAudioSources = settings.Audio.MaxAudioSources,
                RolloffMode = settings.Audio.RolloffMode.ToString()
            },
            AutoSave = settings.AutoSave,
            AutoSaveInterval = settings.AutoSaveInterval,
            MaxEntities = settings.MaxEntities,
            UpdateFrequency = settings.UpdateFrequency
        };
    }

    private List<SerializedEntity> SerializeEntities(World world)
    {
        var serializedEntities = new List<SerializedEntity>();

        foreach (var entity in world.GetAllEntities())
        {
            var serializedEntity = new SerializedEntity
            {
                Id = entity.Id.Value,
                Name = entity.Name,
                IsActive = true, // Assuming all entities are active for now
                Components = SerializeEntityComponents(entity, world)
            };

            serializedEntities.Add(serializedEntity);
        }

        return serializedEntities;
    }

    private List<SerializedComponent> SerializeEntityComponents(Entity entity, World world)
    {
        var components = new List<SerializedComponent>();

        // Get all component types for this entity (this would require extending World to track component types per entity)
        // For now, check for known component types
        var knownComponentTypes = new[]
        {
            typeof(Transform),
            // Add other known component types here
        };

        foreach (var componentType in knownComponentTypes)
        {
            var component = GetComponentOfType(world, entity.Id, componentType);
            if (component != null)
            {
                components.Add(SerializeComponent(component));
            }
        }

        return components;
    }

    private SerializedComponent SerializeComponent(Component component)
    {
        var componentType = component.GetType();
        
        if (_componentSerializers.TryGetValue(componentType, out var serializer))
        {
            return serializer.Serialize(component);
        }

        // Default serialization using reflection
        return SerializeComponentDefault(component);
    }

    private SerializedComponent SerializeComponentDefault(Component component)
    {
        var componentType = component.GetType();
        var data = new Dictionary<string, object>();

        // Use reflection to serialize public properties
        var properties = componentType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.CanRead && property.Name != "EntityId") // Skip EntityId as it's handled separately
            {
                var value = property.GetValue(component);
                if (value != null)
                {
                    data[property.Name] = value;
                }
            }
        }

        return new SerializedComponent
        {
            TypeName = componentType.FullName ?? componentType.Name,
            Data = data
        };
    }

    private List<SerializedSceneNode> SerializeSceneGraph(SceneGraph sceneGraph)
    {
        var serializedNodes = new List<SerializedSceneNode>();

        foreach (var node in sceneGraph.GetAllNodes())
        {
            var serializedNode = new SerializedSceneNode
            {
                EntityId = node.EntityId.Value,
                Name = node.Name,
                ParentEntityId = node.Parent?.EntityId.Value,
                ChildEntityIds = node.Children.Select(c => c.EntityId.Value).ToList(),
                LocalTransform = SerializeTransform(node.LocalTransform),
                WorldTransform = SerializeTransform(node.WorldTransform),
                IsActive = node.IsActive
            };

            serializedNodes.Add(serializedNode);
        }

        return serializedNodes;
    }

    private TransformData SerializeTransform(Transform transform)
    {
        return new TransformData
        {
            Position = transform.Position,
            Rotation = new Vector4(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, transform.Rotation.W),
            Scale = transform.Scale
        };
    }

    #endregion

    #region Scene Deserialization

    private Scene DeserializeScene(SerializedScene serializedScene)
    {
        // Create scene settings
        var settings = DeserializeSettings(serializedScene.Settings);

        // Create scene
        var scene = new Scene(serializedScene.Metadata.Name, new World(), settings);

        // Set metadata
        scene.Id = new SceneId(serializedScene.Metadata.Id);
        scene.Name = serializedScene.Metadata.Name;

        // Restore metadata (Scene needs to provide metadata setting methods)
        // This will be handled when Scene class provides proper metadata management

        // Deserialize entities
        var entityMap = DeserializeEntities(serializedScene.Entities, scene.Entities);

        // Reconstruct scene graph
        DeserializeSceneGraph(serializedScene.SceneGraph, scene.SceneGraph, entityMap);

        return scene;
    }

    private SceneSettings DeserializeSettings(SceneSettingsData data)
    {
        var settings = new SceneSettings
        {
            BackgroundColor = data.BackgroundColor,
            AmbientLight = data.AmbientLight,
            AutoSave = data.AutoSave,
            AutoSaveInterval = data.AutoSaveInterval,
            MaxEntities = data.MaxEntities,
            UpdateFrequency = data.UpdateFrequency
        };

        // Deserialize fog settings
        settings.Fog.Enabled = data.Fog.Enabled;
        settings.Fog.Color = data.Fog.Color;
        settings.Fog.Density = data.Fog.Density;
        settings.Fog.Start = data.Fog.Start;
        settings.Fog.End = data.Fog.End;
        if (Enum.TryParse<FogMode>(data.Fog.Mode, out var fogMode))
        {
            settings.Fog.Mode = fogMode;
        }

        // Deserialize physics settings
        settings.Physics.Enabled = data.Physics.Enabled;
        settings.Physics.Gravity = data.Physics.Gravity;
        settings.Physics.TimeStep = data.Physics.TimeStep;
        settings.Physics.MaxSubSteps = data.Physics.MaxSubSteps;
        settings.Physics.SolverIterations = data.Physics.SolverIterations;
        settings.Physics.ContinuousCollisionDetection = data.Physics.ContinuousCollisionDetection;

        // Deserialize rendering settings
        settings.Rendering.ShadowsEnabled = data.Rendering.ShadowsEnabled;
        settings.Rendering.ShadowQuality = data.Rendering.ShadowQuality;
        settings.Rendering.ShadowDistance = data.Rendering.ShadowDistance;
        settings.Rendering.PostProcessingEnabled = data.Rendering.PostProcessingEnabled;
        if (Enum.TryParse<AntiAliasingMode>(data.Rendering.AntiAliasing, out var aaMode))
        {
            settings.Rendering.AntiAliasing = aaMode;
        }
        settings.Rendering.VSync = data.Rendering.VSync;
        settings.Rendering.TargetFrameRate = data.Rendering.TargetFrameRate;
        settings.Rendering.LodEnabled = data.Rendering.LodEnabled;
        settings.Rendering.FrustumCullingEnabled = data.Rendering.FrustumCullingEnabled;
        settings.Rendering.OcclusionCullingEnabled = data.Rendering.OcclusionCullingEnabled;

        // Deserialize audio settings
        settings.Audio.MasterVolume = data.Audio.MasterVolume;
        settings.Audio.SpatialAudioEnabled = data.Audio.SpatialAudioEnabled;
        settings.Audio.DopplerEnabled = data.Audio.DopplerEnabled;
        settings.Audio.SpeedOfSound = data.Audio.SpeedOfSound;
        settings.Audio.MaxAudioSources = data.Audio.MaxAudioSources;
        if (Enum.TryParse<AudioRolloffMode>(data.Audio.RolloffMode, out var rolloffMode))
        {
            settings.Audio.RolloffMode = rolloffMode;
        }

        return settings;
    }

    private Dictionary<ulong, Entity> DeserializeEntities(List<SerializedEntity> serializedEntities, World world)
    {
        var entityMap = new Dictionary<ulong, Entity>();

        foreach (var serializedEntity in serializedEntities)
        {
            var entity = world.CreateEntity(serializedEntity.Name);
            entityMap[serializedEntity.Id] = entity;

            // Deserialize components
            foreach (var serializedComponent in serializedEntity.Components)
            {
                DeserializeComponent(serializedComponent, entity, world);
            }
        }

        return entityMap;
    }

    private void DeserializeComponent(SerializedComponent serializedComponent, Entity entity, World world)
    {
        var componentType = Type.GetType(serializedComponent.TypeName);
        if (componentType == null)
        {
            // Component type not found, skip
            return;
        }

        if (_componentSerializers.TryGetValue(componentType, out var serializer))
        {
            var component = serializer.Deserialize(serializedComponent, entity.Id);
            if (component != null)
            {
                world.AddComponent(entity.Id, component);
            }
        }
        else
        {
            // Default deserialization
            var component = DeserializeComponentDefault(serializedComponent, componentType, entity.Id);
            if (component != null)
            {
                world.AddComponent(entity.Id, component);
            }
        }
    }

    private Component? DeserializeComponentDefault(SerializedComponent serializedComponent, Type componentType, EntityId entityId)
    {
        try
        {
            var component = Activator.CreateInstance(componentType) as Component;
            if (component == null) return null;

            component.EntityId = entityId;

            // Set properties using reflection
            var properties = componentType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.CanWrite && property.Name != "EntityId" && 
                    serializedComponent.Data.TryGetValue(property.Name, out var value))
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(component, convertedValue);
                    }
                    catch
                    {
                        // Skip properties that can't be converted
                    }
                }
            }

            return component;
        }
        catch
        {
            return null;
        }
    }

    private void DeserializeSceneGraph(List<SerializedSceneNode> serializedNodes, SceneGraph sceneGraph, Dictionary<ulong, Entity> entityMap)
    {
        var nodeMap = new Dictionary<ulong, SceneNode>();

        // First pass: create all nodes
        foreach (var serializedNode in serializedNodes)
        {
            if (entityMap.TryGetValue(serializedNode.EntityId, out var entity))
            {
                var transform = DeserializeTransform(serializedNode.LocalTransform);
                var node = sceneGraph.CreateNode(entity.Id, null, serializedNode.Name);
                
                // Set transform properties
                if (transform != null)
                {
                    node.LocalTransform = transform;
                }
                
                node.IsActive = serializedNode.IsActive;
                nodeMap[serializedNode.EntityId] = node;
            }
        }

        // Second pass: establish parent-child relationships
        foreach (var serializedNode in serializedNodes)
        {
            if (nodeMap.TryGetValue(serializedNode.EntityId, out var node) && 
                serializedNode.ParentEntityId.HasValue &&
                nodeMap.TryGetValue(serializedNode.ParentEntityId.Value, out var parentNode))
            {
                sceneGraph.ReparentNode(node, parentNode);
            }
        }
    }

    private Transform DeserializeTransform(TransformData data)
    {
        return new Transform
        {
            Position = data.Position,
            Rotation = new Quaternion(data.Rotation.X, data.Rotation.Y, data.Rotation.Z, data.Rotation.W),
            Scale = data.Scale
        };
    }

    #endregion

    #region Helper Methods

    private Component? GetComponentOfType(World world, EntityId entityId, Type componentType)
    {
        // This is a simplified approach - in a real implementation, World would need to support
        // getting components by type directly
        if (componentType == typeof(Transform))
        {
            return world.GetComponent<Transform>(entityId);
        }

        // Add other component type checks here
        return null;
    }

    private void RegisterDefaultComponentSerializers()
    {
        // Register built-in component serializers
        _componentSerializers[typeof(Transform)] = new TransformComponentSerializer();
        // Add other component serializers here
    }

    #endregion
}

/// <summary>
/// Interface for component-specific serialization
/// </summary>
public interface IComponentSerializer
{
    SerializedComponent Serialize(Component component);
    Component? Deserialize(SerializedComponent data, EntityId entityId);
}

/// <summary>
/// Transform component serializer
/// </summary>
public class TransformComponentSerializer : IComponentSerializer
{
    public SerializedComponent Serialize(Component component)
    {
        if (component is not Transform transform)
            throw new ArgumentException("Component must be a Transform", nameof(component));

        return new SerializedComponent
        {
            TypeName = typeof(Transform).FullName ?? nameof(Transform),
            Data = new Dictionary<string, object>
            {
                ["Position"] = transform.Position,
                ["Rotation"] = new { transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, transform.Rotation.W },
                ["Scale"] = transform.Scale
            }
        };
    }

    public Component? Deserialize(SerializedComponent data, EntityId entityId)
    {
        try
        {
            var transform = new Transform { EntityId = entityId };

            if (data.Data.TryGetValue("Position", out var posObj) && posObj is JsonElement posElement)
            {
                transform.Position = JsonSerializer.Deserialize<Vector3>(posElement.GetRawText());
            }

            if (data.Data.TryGetValue("Rotation", out var rotObj) && rotObj is JsonElement rotElement)
            {
                var rotData = JsonSerializer.Deserialize<dynamic>(rotElement.GetRawText());
                // Handle rotation deserialization
            }

            if (data.Data.TryGetValue("Scale", out var scaleObj) && scaleObj is JsonElement scaleElement)
            {
                transform.Scale = JsonSerializer.Deserialize<Vector3>(scaleElement.GetRawText());
            }

            return transform;
        }
        catch
        {
            return null;
        }
    }
}

#region JSON Converters

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new Vector3(
            root.GetProperty("x").GetSingle(),
            root.GetProperty("y").GetSingle(),
            root.GetProperty("z").GetSingle()
        );
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}

public class Vector4Converter : JsonConverter<Vector4>
{
    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new Vector4(
            root.GetProperty("x").GetSingle(),
            root.GetProperty("y").GetSingle(),
            root.GetProperty("z").GetSingle(),
            root.GetProperty("w").GetSingle()
        );
    }

    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteNumber("w", value.W);
        writer.WriteEndObject();
    }
}

public class QuaternionConverter : JsonConverter<Quaternion>
{
    public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        return new Quaternion(
            root.GetProperty("x").GetSingle(),
            root.GetProperty("y").GetSingle(),
            root.GetProperty("z").GetSingle(),
            root.GetProperty("w").GetSingle()
        );
    }

    public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteNumber("w", value.W);
        writer.WriteEndObject();
    }
}

#endregion