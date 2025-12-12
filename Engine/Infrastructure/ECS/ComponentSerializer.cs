using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Infrastructure.ECS;

/// <summary>
/// Handles serialization and deserialization of entities and components
/// Supports JSON format for scene persistence
/// </summary>
public class ComponentSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly Dictionary<string, Type> _componentTypeRegistry;

    public ComponentSerializer()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new ComponentConverter() },
            IncludeFields = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _componentTypeRegistry = new Dictionary<string, Type>();
        RegisterBuiltInComponentTypes();
    }

    /// <summary>
    /// Register a component type for serialization
    /// </summary>
    public void RegisterComponentType<T>() where T : Component
    {
        var type = typeof(T);
        _componentTypeRegistry[type.Name] = type;
        _componentTypeRegistry[type.FullName ?? type.Name] = type;
    }

    /// <summary>
    /// Serialize an entity with all its components
    /// </summary>
    public string SerializeEntity(Entity entity, World world)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        if (world == null)
            throw new ArgumentNullException(nameof(world));

        var entityData = new SerializedEntity
        {
            Id = entity.Id.Value,
            Name = entity.Name,
            Components = SerializeEntityComponents(entity, world)
        };

        return JsonSerializer.Serialize(entityData, _options);
    }

    /// <summary>
    /// Deserialize an entity from JSON
    /// </summary>
    public Entity DeserializeEntity(string json, World world)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentNullException(nameof(json));
        if (world == null)
            throw new ArgumentNullException(nameof(world));

        var entityData = JsonSerializer.Deserialize<SerializedEntity>(json, _options);
        if (entityData == null)
            throw new InvalidOperationException("Failed to deserialize entity data");

        // Create entity with specific ID (we'll need to modify World to support this)
        var entity = CreateEntityWithId(world, entityData.Id, entityData.Name);
        
        // Add components
        foreach (var componentData in entityData.Components)
        {
            DeserializeAndAddComponent(entity, world, componentData);
        }

        return entity;
    }

    /// <summary>
    /// Serialize a single component
    /// </summary>
    public string SerializeComponent(Component component)
    {
        if (component == null)
            throw new ArgumentNullException(nameof(component));

        var componentData = new SerializedComponent
        {
            TypeName = component.GetType().Name,
            Data = JsonSerializer.SerializeToElement(component, component.GetType(), _options)
        };

        return JsonSerializer.Serialize(componentData, _options);
    }

    /// <summary>
    /// Deserialize a component from JSON
    /// </summary>
    public T DeserializeComponent<T>(string json) where T : Component
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentNullException(nameof(json));

        var componentData = JsonSerializer.Deserialize<SerializedComponent>(json, _options);
        if (componentData == null)
            throw new InvalidOperationException("Failed to deserialize component data");

        if (!_componentTypeRegistry.TryGetValue(componentData.TypeName, out var componentType))
            throw new InvalidOperationException($"Component type '{componentData.TypeName}' is not registered");

        if (!typeof(T).IsAssignableFrom(componentType))
            throw new InvalidOperationException($"Component type '{componentData.TypeName}' is not assignable to '{typeof(T).Name}'");

        var component = JsonSerializer.Deserialize(componentData.Data, componentType, _options);
        return (T)component!;
    }

    /// <summary>
    /// Serialize multiple entities (for scene serialization)
    /// </summary>
    public string SerializeWorld(World world)
    {
        if (world == null)
            throw new ArgumentNullException(nameof(world));

        var entities = world.GetAllEntities()
            .Select(entity => new SerializedEntity
            {
                Id = entity.Id.Value,
                Name = entity.Name,
                Components = SerializeEntityComponents(entity, world)
            })
            .ToArray();

        var worldData = new SerializedWorld
        {
            Entities = entities,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(worldData, _options);
    }

    /// <summary>
    /// Deserialize multiple entities into a world
    /// </summary>
    public void DeserializeWorld(string json, World world, bool clearWorld = true)
    {
        if (string.IsNullOrEmpty(json))
            throw new ArgumentNullException(nameof(json));
        if (world == null)
            throw new ArgumentNullException(nameof(world));

        if (clearWorld)
        {
            world.Clear();
        }

        var worldData = JsonSerializer.Deserialize<SerializedWorld>(json, _options);
        if (worldData == null)
            throw new InvalidOperationException("Failed to deserialize world data");

        foreach (var entityData in worldData.Entities)
        {
            var entity = CreateEntityWithId(world, entityData.Id, entityData.Name);
            
            foreach (var componentData in entityData.Components)
            {
                DeserializeAndAddComponent(entity, world, componentData);
            }
        }
    }

    private SerializedComponent[] SerializeEntityComponents(Entity entity, World world)
    {
        var components = new List<SerializedComponent>();

        // We need to get all components for this entity
        // This would require extending the World or ComponentStorage interface
        // For now, we'll use reflection to get common component types
        var componentTypes = GetKnownComponentTypes();

        foreach (var componentType in componentTypes)
        {
            var component = GetComponentOfType(world, entity.Id, componentType);
            if (component != null)
            {
                components.Add(new SerializedComponent
                {
                    TypeName = componentType.Name,
                    Data = JsonSerializer.SerializeToElement(component, componentType, _options)
                });
            }
        }

        return components.ToArray();
    }

    private Component? GetComponentOfType(World world, EntityId entityId, Type componentType)
    {
        var method = world.GetType().GetMethod("GetComponent");
        if (method != null)
        {
            var genericMethod = method.MakeGenericMethod(componentType);
            return genericMethod.Invoke(world, new object[] { entityId }) as Component;
        }

        return null;
    }

    private void DeserializeAndAddComponent(Entity entity, World world, SerializedComponent componentData)
    {
        if (!_componentTypeRegistry.TryGetValue(componentData.TypeName, out var componentType))
        {
            // Log warning and skip unknown components
            Console.WriteLine($"Warning: Unknown component type '{componentData.TypeName}' skipped");
            return;
        }

        var component = JsonSerializer.Deserialize(componentData.Data, componentType, _options) as Component;
        if (component != null)
        {
            AddComponentOfType(world, entity.Id, component, componentType);
        }
    }

    private void AddComponentOfType(World world, EntityId entityId, Component component, Type componentType)
    {
        var method = world.GetType().GetMethod("AddComponent", new[] { typeof(EntityId), componentType });
        if (method != null)
        {
            var genericMethod = method.MakeGenericMethod(componentType);
            genericMethod.Invoke(world, new object[] { entityId, component });
        }
    }

    private Entity CreateEntityWithId(World world, ulong id, string name)
    {
        // For now, create a regular entity and hope the IDs don't conflict
        // In a full implementation, we'd need World.CreateEntityWithId method
        return world.CreateEntity(name);
    }

    private void RegisterBuiltInComponentTypes()
    {
        // Register common component types
        // In a real implementation, this could be done via reflection
        RegisterComponentType<Engine.Domain.ECS.Components.Transform>();
        RegisterComponentType<Engine.Domain.ECS.Components.Camera>();
        RegisterComponentType<Engine.Domain.ECS.Components.Light>();
        RegisterComponentType<Engine.Domain.ECS.Components.Renderable>();
        RegisterComponentType<Engine.Domain.ECS.Components.PhysicsBody>();
        RegisterComponentType<Engine.Domain.ECS.Components.ScriptComponent>();
    }

    private IEnumerable<Type> GetKnownComponentTypes()
    {
        return _componentTypeRegistry.Values.Distinct();
    }

    // Data classes for serialization
    private class SerializedEntity
    {
        public ulong Id { get; set; }
        public string Name { get; set; } = "";
        public SerializedComponent[] Components { get; set; } = Array.Empty<SerializedComponent>();
    }

    private class SerializedComponent
    {
        public string TypeName { get; set; } = "";
        public JsonElement Data { get; set; }
    }

    private class SerializedWorld
    {
        public SerializedEntity[] Entities { get; set; } = Array.Empty<SerializedEntity>();
        public DateTime Timestamp { get; set; }
    }
}

/// <summary>
/// Custom JSON converter for Component polymorphism
/// </summary>
internal class ComponentConverter : JsonConverter<Component>
{
    public override Component? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // This would need more sophisticated handling for polymorphic deserialization
        throw new NotSupportedException("Use ComponentSerializer.DeserializeComponent instead");
    }

    public override void Write(Utf8JsonWriter writer, Component value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}