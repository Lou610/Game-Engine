# Unity-Type Game Engine

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

