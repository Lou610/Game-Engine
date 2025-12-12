using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.ECS;

namespace Engine.Application.ECS;

/// <summary>
/// Manages system registration, initialization, and execution
/// </summary>
public class SystemManager : IDisposable
{
    private readonly List<System> _systems;
    private readonly World _world;
    private bool _initialized;
    private bool _disposed;

    public SystemManager(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _systems = new List<System>();
    }

    /// <summary>
    /// Register a system for execution
    /// </summary>
    public void RegisterSystem(System system)
    {
        ThrowIfDisposed();
        
        if (system == null)
            throw new ArgumentNullException(nameof(system));

        if (_systems.Contains(system))
            throw new InvalidOperationException($"System {system.GetType().Name} is already registered");

        _systems.Add(system);
        
        // Sort by priority after adding
        _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        // If we're already initialized, initialize the new system
        if (_initialized)
        {
            system.Initialize(_world);
        }
    }

    /// <summary>
    /// Unregister a system
    /// </summary>
    public void UnregisterSystem(System system)
    {
        ThrowIfDisposed();
        
        if (system == null)
            return;

        if (_systems.Remove(system))
        {
            if (_initialized)
            {
                system.Shutdown();
            }
        }
    }

    /// <summary>
    /// Register a system by type (creates instance)
    /// </summary>
    public T RegisterSystem<T>() where T : System, new()
    {
        var system = new T();
        RegisterSystem(system);
        return system;
    }

    /// <summary>
    /// Get a registered system by type
    /// </summary>
    public T? GetSystem<T>() where T : System
    {
        ThrowIfDisposed();
        return _systems.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Get all registered systems
    /// </summary>
    public IReadOnlyList<System> GetAllSystems()
    {
        ThrowIfDisposed();
        return _systems.AsReadOnly();
    }

    /// <summary>
    /// Initialize all systems
    /// </summary>
    public void InitializeSystems()
    {
        ThrowIfDisposed();
        
        if (_initialized)
            return;

        foreach (var system in _systems)
        {
            try
            {
                system.Initialize(_world);
            }
            catch (Exception ex)
            {
                // Log error and continue with other systems
                Console.WriteLine($"Error initializing system {system.GetType().Name}: {ex.Message}");
            }
        }

        _initialized = true;
    }

    /// <summary>
    /// Update all enabled systems
    /// </summary>
    public void UpdateSystems(float deltaTime)
    {
        ThrowIfDisposed();
        
        if (!_initialized)
            throw new InvalidOperationException("Systems must be initialized before updating");

        foreach (var system in _systems.Where(s => s.IsEnabled))
        {
            try
            {
                system.Update(deltaTime);
            }
            catch (Exception ex)
            {
                // Log error and continue with other systems
                Console.WriteLine($"Error updating system {system.GetType().Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Fixed update all enabled systems
    /// </summary>
    public void FixedUpdateSystems(float fixedDeltaTime)
    {
        ThrowIfDisposed();
        
        if (!_initialized)
            throw new InvalidOperationException("Systems must be initialized before fixed updating");

        foreach (var system in _systems.Where(s => s.IsEnabled))
        {
            try
            {
                system.FixedUpdate(fixedDeltaTime);
            }
            catch (Exception ex)
            {
                // Log error and continue with other systems
                Console.WriteLine($"Error fixed updating system {system.GetType().Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Shutdown all systems
    /// </summary>
    public void ShutdownSystems()
    {
        ThrowIfDisposed();
        
        if (!_initialized)
            return;

        // Shutdown in reverse order
        for (int i = _systems.Count - 1; i >= 0; i--)
        {
            try
            {
                _systems[i].Shutdown();
            }
            catch (Exception ex)
            {
                // Log error and continue with other systems
                Console.WriteLine($"Error shutting down system {_systems[i].GetType().Name}: {ex.Message}");
            }
        }

        _initialized = false;
    }

    /// <summary>
    /// Enable or disable a system by type
    /// </summary>
    public void SetSystemEnabled<T>(bool enabled) where T : System
    {
        var system = GetSystem<T>();
        if (system != null)
        {
            system.IsEnabled = enabled;
        }
    }

    /// <summary>
    /// Check if a system is registered
    /// </summary>
    public bool IsSystemRegistered<T>() where T : System
    {
        return GetSystem<T>() != null;
    }

    /// <summary>
    /// Get the number of registered systems
    /// </summary>
    public int SystemCount => _systems.Count;

    /// <summary>
    /// Get the number of enabled systems
    /// </summary>
    public int EnabledSystemCount => _systems.Count(s => s.IsEnabled);

    /// <summary>
    /// Clear all systems
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        
        ShutdownSystems();
        _systems.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SystemManager));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ShutdownSystems();
            _systems.Clear();
            _disposed = true;
        }
    }
}