# Phase 2: Complete ECS System Implementation

## Overview

Phase 2 focuses on completing the Entity Component System (ECS) architecture that was started in Phase 1. While the basic foundation exists, we need to build out the complete ECS functionality with efficient component storage, world management, and system execution.

## Current State Analysis (Phase 1 Completed)

### ✅ Already Implemented:
- Basic Application class and GameLoop
- Core domain objects: Time, ApplicationConfig, LogLevel
- Basic Entity and Component base classes
- EntityId and ComponentType value objects
- Transform component (basic implementation)
- Skeleton components: Camera, Light, Renderable, PhysicsBody, ScriptComponent
- Basic System abstract class
- WorldService skeleton
- Test infrastructure setup with NUnit
- Basic test fixtures and domain tests

### ❌ Missing/Incomplete:
- Complete World entity management implementation
- Efficient component storage with archetypes
- Component lifecycle management (add/remove/query)
- System registration and execution pipeline
- Component events and event handling
- Advanced ECS queries and filtering
- Performance optimizations (memory pools, batch operations)
- Component serialization support

## Phase 2 Goals

### Primary Objectives:
1. **Complete World Implementation** - Full entity and component lifecycle management
2. **Implement Component Storage** - Efficient archetype-based storage system
3. **Build System Pipeline** - Registration, initialization, and execution of systems
4. **Add Component Events** - Domain events for component lifecycle
5. **Create ECS Query System** - Efficient component queries and filtering
6. **Implement Component Serialization** - Support for scene persistence

### Secondary Objectives:
1. **Performance Optimization** - Memory pooling and batch operations
2. **Advanced ECS Features** - Component dependencies and validation
3. **Comprehensive Testing** - Complete test coverage for all ECS functionality
4. **Documentation** - API documentation and usage examples

## Detailed Implementation Plan

### 1. Complete World Implementation (`Engine/Domain/ECS/World.cs`)

**Current Issues:**
- World class exists but lacks full implementation
- No entity creation/destruction methods
- No component management capabilities

**Required Implementation:**
```csharp
public class World : IDisposable
{
    // Entity management
    Entity CreateEntity(string name = "");
    void DestroyEntity(EntityId id);
    Entity? GetEntity(EntityId id);
    IEnumerable<Entity> GetAllEntities();
    
    // Component management
    T AddComponent<T>(EntityId entityId, T component) where T : Component;
    T? GetComponent<T>(EntityId entityId) where T : Component;
    bool HasComponent<T>(EntityId entityId) where T : Component;
    void RemoveComponent<T>(EntityId entityId) where T : Component;
    
    // Queries
    IEnumerable<Entity> Query<T>() where T : Component;
    IEnumerable<Entity> Query<T1, T2>() where T1 : Component where T2 : Component;
    
    // Events
    event Action<EntityId, Component> ComponentAdded;
    event Action<EntityId, Component> ComponentRemoved;
    event Action<EntityId> EntityDestroyed;
}
```

### 2. Implement Component Storage (`Engine/Infrastructure/ECS/ComponentStorage.cs`)

**Architecture Decision: Archetype-based Storage**

Components will be stored using an archetype system for maximum performance:

```csharp
public class ComponentStorage
{
    // Archetype management
    private Dictionary<ComponentSignature, Archetype> _archetypes;
    private Dictionary<EntityId, Archetype> _entityToArchetype;
    
    // Core operations
    void AddComponent<T>(EntityId entityId, T component) where T : Component;
    T? GetComponent<T>(EntityId entityId) where T : Component;
    void RemoveComponent<T>(EntityId entityId) where T : Component;
    
    // Queries
    IEnumerable<T> GetAllComponents<T>() where T : Component;
    IEnumerable<(EntityId, T)> GetEntitiesWithComponent<T>() where T : Component;
    
    // Archetype operations
    void MoveEntityToArchetype(EntityId entityId, ComponentSignature signature);
}
```

**Archetype Structure:**
```csharp
public class Archetype
{
    public ComponentSignature Signature { get; }
    private Dictionary<Type, Array> _componentArrays;
    private List<EntityId> _entities;
    
    public void AddEntity(EntityId entityId, params Component[] components);
    public void RemoveEntity(EntityId entityId);
    public T[] GetComponentArray<T>() where T : Component;
}
```

### 3. Build System Pipeline (`Engine/Application/ECS/SystemManager.cs`)

Create a new SystemManager to handle system registration and execution:

```csharp
public class SystemManager
{
    private readonly List<System> _systems;
    private readonly World _world;
    
    // System lifecycle
    void RegisterSystem(System system);
    void UnregisterSystem(System system);
    void InitializeSystems();
    void ShutdownSystems();
    
    // Update loops
    void UpdateSystems(float deltaTime);
    void FixedUpdateSystems(float fixedDeltaTime);
    
    // System queries
    T? GetSystem<T>() where T : System;
    IEnumerable<System> GetAllSystems();
}
```

**Enhanced System Base Class:**
```csharp
public abstract class System
{
    protected World World { get; private set; }
    
    public virtual void Initialize(World world) { World = world; }
    public virtual void Update(float deltaTime) { }
    public virtual void FixedUpdate(float fixedDeltaTime) { }
    public virtual void Shutdown() { }
    
    // System priority for execution order
    public virtual int Priority => 0;
    
    // Component queries helper methods
    protected IEnumerable<Entity> Query<T>() where T : Component;
    protected IEnumerable<Entity> Query<T1, T2>() where T1 : Component where T2 : Component;
}
```

### 4. Add Component Events (`Engine/Domain/ECS/Events/`)

**Domain Events to Implement:**
- `EntityCreated.cs`
- `EntityDestroyed.cs`
- `ComponentAdded.cs`
- `ComponentRemoved.cs`
- `ComponentChanged.cs`

**Event Infrastructure:**
```csharp
public interface IDomainEventPublisher
{
    void Publish<T>(T domainEvent) where T : IDomainEvent;
    void Subscribe<T>(Action<T> handler) where T : IDomainEvent;
    void Unsubscribe<T>(Action<T> handler) where T : IDomainEvent;
}
```

### 5. Create ECS Query System (`Engine/Domain/ECS/Queries/`)

**Query Builder Pattern:**
```csharp
public class EntityQuery
{
    private readonly World _world;
    private readonly HashSet<Type> _withComponents = new();
    private readonly HashSet<Type> _withoutComponents = new();
    
    public EntityQuery With<T>() where T : Component;
    public EntityQuery Without<T>() where T : Component;
    public IEnumerable<Entity> Execute();
    
    // Performance optimization: cached queries
    public EntityQuery Cache();
}
```

**Usage Examples:**
```csharp
// Find all entities with Transform and Renderable components
var renderableEntities = world.Query()
    .With<Transform>()
    .With<Renderable>()
    .Execute();

// Find entities with Transform but without PhysicsBody
var staticEntities = world.Query()
    .With<Transform>()
    .Without<PhysicsBody>()
    .Execute();
```

### 6. Implement Component Serialization (`Engine/Infrastructure/ECS/ComponentSerializer.cs`)

**JSON-based Serialization:**
```csharp
public class ComponentSerializer
{
    public string SerializeEntity(Entity entity, World world);
    public Entity DeserializeEntity(string json, World world);
    
    public string SerializeComponent(Component component);
    public T DeserializeComponent<T>(string json) where T : Component;
    
    // Bulk operations for scenes
    public string SerializeWorld(World world);
    public void DeserializeWorld(string json, World world);
}
```

### 7. Performance Optimizations

**Memory Pool for Entities:**
```csharp
public class EntityPool
{
    private readonly Stack<EntityId> _availableIds = new();
    private ulong _nextId = 1;
    
    public EntityId AllocateId();
    public void ReleaseId(EntityId id);
}
```

**Component Array Pooling:**
```csharp
public class ComponentArrayPool<T> where T : Component
{
    private readonly Stack<T[]> _arrays = new();
    
    public T[] Rent(int size);
    public void Return(T[] array);
}
```

## Required Files to Create/Modify

### New Files:
```
Engine/Domain/ECS/
├── Events/
│   ├── EntityCreated.cs
│   ├── EntityDestroyed.cs
│   ├── ComponentAdded.cs
│   ├── ComponentRemoved.cs
│   └── ComponentChanged.cs
├── Services/
│   ├── ComponentArchetypeService.cs
│   ├── EntityLifecycleService.cs
│   └── EntityQueryService.cs
├── Queries/
│   ├── EntityQuery.cs
│   ├── ComponentSignature.cs
│   └── QueryCache.cs
└── Interfaces/
    ├── IDomainEvent.cs
    └── IDomainEventPublisher.cs

Engine/Application/ECS/
├── SystemManager.cs
├── DomainEventPublisher.cs
└── ECSApplicationService.cs

Engine/Infrastructure/ECS/
├── Archetype.cs
├── ComponentArrayPool.cs
├── EntityPool.cs
└── ComponentSerializer.cs
```

### Files to Modify:
```
Engine/Domain/ECS/World.cs          # Complete implementation
Engine/Application/ECS/System.cs    # Enhanced with World access and queries
Engine/Infrastructure/ECS/ComponentStorage.cs  # Complete archetype system
Engine/Application/ECS/WorldService.cs  # Integration with SystemManager
```

## Testing Requirements

### New Test Files:
```
Tests/GameEngine.Domain.Tests/ECS/
├── Events/
├── Services/
├── Queries/
│   ├── EntityQueryTests.cs
│   └── ComponentSignatureTests.cs
├── ArchetypeTests.cs
└── WorldIntegrationTests.cs

Tests/GameEngine.Application.Tests/ECS/
├── SystemManagerTests.cs
└── DomainEventPublisherTests.cs

Tests/GameEngine.Infrastructure.Tests/ECS/
├── ComponentStorageTests.cs
├── ComponentSerializerTests.cs
├── EntityPoolTests.cs
└── ComponentArrayPoolTests.cs
```

### Test Coverage Goals:
- **World operations**: 95% (critical path)
- **Component Storage**: 90% (performance critical)
- **System Management**: 85%
- **Serialization**: 90% (data integrity critical)
- **Event System**: 85%

## Performance Benchmarks

### Target Performance Metrics:
- **Entity Creation**: < 1μs per entity
- **Component Addition**: < 500ns per component
- **Component Query**: < 100μs for 10,000 entities
- **System Update**: < 16ms for 60 FPS (all systems combined)

### Profiling Points:
- Component storage access patterns
- Memory allocation in hot paths
- Query execution time
- Event dispatch overhead

## Integration Points

### With Phase 3 (Vulkan Rendering):
- Rendering systems will use Transform and Renderable components
- Camera component integration with rendering pipeline
- Efficient queries for renderable entities

### With Future Phases:
- Physics systems will query Transform and PhysicsBody components
- Script systems will access and modify components
- Scene serialization will use component serialization

## Dependencies

### External Libraries:
- **Newtonsoft.Json** (or System.Text.Json) for serialization
- **NUnit** for testing
- **NSubstitute** for mocking in tests

### Internal Dependencies:
- Core foundation from Phase 1
- Domain events infrastructure
- Platform abstraction layer

## Success Criteria

Phase 2 is considered complete when:

1. ✅ All entities can be created, destroyed, and managed through World
2. ✅ Components can be added, removed, and queried efficiently
3. ✅ Systems can be registered and execute in proper order
4. ✅ Component archetype storage provides expected performance
5. ✅ Entity queries work for complex component combinations
6. ✅ Component serialization supports scene save/load
7. ✅ All tests pass with target coverage metrics
8. ✅ Performance benchmarks meet target metrics
9. ✅ Domain events fire correctly for ECS operations
10. ✅ Memory usage remains stable during entity/component churn

## Risk Assessment

### High Risk:
- **Performance of archetype system** - Complex implementation with high performance requirements
- **Memory management** - Risk of leaks with component pooling

### Medium Risk:
- **Query system complexity** - Risk of over-engineering vs. performance needs
- **Serialization compatibility** - Component versioning and migration

### Low Risk:
- **Event system** - Well-understood domain event patterns
- **System management** - Straightforward orchestration layer

## Next Phase Preparation

Phase 2 completion enables Phase 3 (Vulkan Rendering) by providing:
- Efficient component queries for rendering systems
- Transform component for spatial data
- Camera and Light components for rendering
- Event system for render state changes
- System pipeline for render system integration

The ECS system built in Phase 2 forms the foundation for all subsequent phases and must be robust, performant, and well-tested.