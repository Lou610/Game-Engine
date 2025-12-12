# LAG Game Engine

A full-featured C# game engine built with Domain-Driven Design (DDD) principles, featuring Vulkan rendering, ECS architecture, and support for both C# and Lag scripting languages.

## Architecture

The engine follows a layered DDD architecture:

- **Domain Layer**: Core business logic, entities, value objects, domain services, and domain events
- **Application Layer**: Use cases, application services, DTOs, and orchestration
- **Infrastructure Layer**: Technical implementations (Vulkan, file I/O, platform abstraction)
- **Presentation Layer**: Editor UI and user interfaces

## Features

### Core Systems
- Entity Component System (ECS)
- Vulkan-based rendering (2D and 3D)
- Scene management with prefabs
- Physics system (2D and 3D)
- Audio system
- Input management
- Resource management
- Scripting system with hot-reload

### Scripting
- **C# Scripting**: Full C# script compilation and execution
- **Lag Scripting**: Custom TypeScript-style language that transpiles to C#

### Editor
- Scene viewport
- Hierarchy view
- Inspector for component editing
- Project asset browser
- Console for logging

### Testing
- NUnit test framework
- Unit tests for Domain, Application, and Infrastructure layers
- Integration tests
- Test fixtures and helpers

## Project Structure

```
GameEngine/
├── Engine/
│   ├── Domain/          # Domain layer (business logic)
│   ├── Application/     # Application layer (orchestration)
│   └── Infrastructure/  # Infrastructure layer (technical)
├── Editor/              # Editor application
├── Tests/               # Test projects
└── Shaders/             # GLSL shader files
```

## Development Status

### ✅ Completed Phases
- **Phase 4.1: Scene Foundation** - Basic scene, entity, and component structures
- **Phase 4.2: Scene Graph System** - Hierarchical scene management with transforms  
- **Phase 4.3: Scene Serialization System** - JSON-based scene save/load functionality
- **Phase 4.4: Prefab System** - Reusable entity templates with inheritance support

### 🚧 Current Development Phase
**Phase 4.5: Scene Optimization** - *Ready to start*

Next tasks to implement:
- Scene culling system for performance optimization
- Level-of-Detail (LOD) systems for rendering efficiency  
- Performance optimizations for rendering and update loops
- Frustum culling and occlusion culling
- Spatial partitioning (quadtree/octree) for scene queries

### 📋 Outstanding Work
- **Phase 4.6: Scene Testing** - Comprehensive unit tests and performance benchmarks
- **Phase 5+: Additional Systems** - Physics integration, advanced rendering features, etc.

## Building

```bash
dotnet build
```

## Running Tests

```bash
dotnet test
```

## Dependencies

- Vulkan.NET
- GLFW.NET
- Newtonsoft.Json
- Roslyn (C# compilation)
- ANTLR4 (Lag scripting parser)
- NUnit (testing)
- NSubstitute (mocking)

## License

MIT License

