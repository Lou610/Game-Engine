using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Engine.Domain.ECS.ValueObjects;

namespace Engine.Infrastructure.ECS;

/// <summary>
/// Memory pool for entity IDs to avoid allocation overhead
/// Provides efficient reuse of entity identifiers
/// </summary>
public class EntityPool : IDisposable
{
    private readonly ConcurrentQueue<EntityId> _availableIds;
    private readonly HashSet<EntityId> _usedIds;
    private readonly object _lock = new object();
    private ulong _nextId = 1;
    private bool _disposed;

    public EntityPool()
    {
        _availableIds = new ConcurrentQueue<EntityId>();
        _usedIds = new HashSet<EntityId>();
    }

    /// <summary>
    /// Allocate a new entity ID
    /// </summary>
    public EntityId AllocateId()
    {
        ThrowIfDisposed();

        // Try to reuse an available ID
        if (_availableIds.TryDequeue(out var reusedId))
        {
            lock (_lock)
            {
                _usedIds.Add(reusedId);
            }
            return reusedId;
        }

        // Create a new ID
        EntityId newId;
        lock (_lock)
        {
            newId = new EntityId(_nextId++);
            _usedIds.Add(newId);
        }

        return newId;
    }

    /// <summary>
    /// Release an entity ID back to the pool
    /// </summary>
    public void ReleaseId(EntityId id)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (_usedIds.Remove(id))
            {
                _availableIds.Enqueue(id);
            }
        }
    }

    /// <summary>
    /// Check if an ID is currently allocated
    /// </summary>
    public bool IsAllocated(EntityId id)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            return _usedIds.Contains(id);
        }
    }

    /// <summary>
    /// Get the number of allocated IDs
    /// </summary>
    public int AllocatedCount
    {
        get
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                return _usedIds.Count;
            }
        }
    }

    /// <summary>
    /// Get the number of available IDs in the pool
    /// </summary>
    public int AvailableCount
    {
        get
        {
            ThrowIfDisposed();
            return _availableIds.Count;
        }
    }

    /// <summary>
    /// Clear all allocated and available IDs
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            _usedIds.Clear();
            while (_availableIds.TryDequeue(out _)) { }
            _nextId = 1;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EntityPool));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _disposed = true;
        }
    }
}