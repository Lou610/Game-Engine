using System;
using System.Collections.Generic;

namespace Engine.Infrastructure.Memory;

/// <summary>
/// Infrastructure service for memory management
/// </summary>
public class MemoryManager
{
    private readonly Dictionary<Type, Queue<object>> _objectPools = new();

    public T GetPooled<T>() where T : class, new()
    {
        var type = typeof(T);
        if (!_objectPools.TryGetValue(type, out var pool))
        {
            pool = new Queue<object>();
            _objectPools[type] = pool;
        }

        if (pool.Count > 0)
        {
            return (T)pool.Dequeue();
        }

        return new T();
    }

    public void ReturnPooled<T>(T item) where T : class
    {
        var type = typeof(T);
        if (!_objectPools.TryGetValue(type, out var pool))
        {
            pool = new Queue<object>();
            _objectPools[type] = pool;
        }

        pool.Enqueue(item);
    }

    public void ClearPools()
    {
        _objectPools.Clear();
    }
}

