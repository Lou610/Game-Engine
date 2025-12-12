using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Engine.Domain.ECS;

namespace Engine.Infrastructure.ECS;

/// <summary>
/// Memory pool for component arrays to reduce allocation overhead
/// Provides efficient reuse of component storage arrays
/// </summary>
public class ComponentArrayPool<T> : IDisposable where T : Component
{
    private readonly ConcurrentDictionary<int, ConcurrentQueue<T[]>> _pools;
    private readonly int _maxPoolSize;
    private readonly int[] _standardSizes = { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };
    private bool _disposed;

    public ComponentArrayPool(int maxPoolSize = 100)
    {
        _maxPoolSize = maxPoolSize;
        _pools = new ConcurrentDictionary<int, ConcurrentQueue<T[]>>();
        
        // Initialize pools for standard sizes
        foreach (var size in _standardSizes)
        {
            _pools[size] = new ConcurrentQueue<T[]>();
        }
    }

    /// <summary>
    /// Rent an array of the specified size (or larger)
    /// </summary>
    public T[] Rent(int minimumSize)
    {
        ThrowIfDisposed();

        if (minimumSize <= 0)
            throw new ArgumentException("Size must be positive", nameof(minimumSize));

        // Find the smallest standard size that fits
        var targetSize = GetTargetSize(minimumSize);
        
        // Try to get from pool
        if (_pools.TryGetValue(targetSize, out var pool) && pool.TryDequeue(out var array))
        {
            // Clear the array to ensure clean state
            Array.Clear(array, 0, array.Length);
            return array;
        }

        // Create new array if none available
        return new T[targetSize];
    }

    /// <summary>
    /// Return an array to the pool
    /// </summary>
    public void Return(T[] array)
    {
        if (array == null || _disposed)
            return;

        var size = array.Length;
        
        // Only pool arrays of standard sizes
        if (!IsStandardSize(size))
            return;

        // Get or create pool for this size
        var pool = _pools.GetOrAdd(size, _ => new ConcurrentQueue<T[]>());
        
        // Only return if pool isn't full
        if (pool.Count < _maxPoolSize)
        {
            // Clear references to avoid memory leaks
            Array.Clear(array, 0, array.Length);
            pool.Enqueue(array);
        }
    }

    /// <summary>
    /// Get statistics about the pool
    /// </summary>
    public PoolStatistics GetStatistics()
    {
        ThrowIfDisposed();

        var stats = new PoolStatistics();
        
        foreach (var (size, pool) in _pools)
        {
            var count = pool.Count;
            stats.PoolSizes[size] = count;
            stats.TotalPooledArrays += count;
        }

        return stats;
    }

    /// <summary>
    /// Clear all pooled arrays
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();

        foreach (var pool in _pools.Values)
        {
            while (pool.TryDequeue(out _)) { }
        }
    }

    /// <summary>
    /// Trim pools to remove excess arrays
    /// </summary>
    public void TrimExcess(int targetPoolSize = 10)
    {
        ThrowIfDisposed();

        foreach (var pool in _pools.Values)
        {
            while (pool.Count > targetPoolSize && pool.TryDequeue(out _)) { }
        }
    }

    private int GetTargetSize(int minimumSize)
    {
        // Find the smallest standard size that fits
        foreach (var size in _standardSizes)
        {
            if (size >= minimumSize)
                return size;
        }

        // If larger than largest standard size, round up to next power of 2
        var targetSize = 1;
        while (targetSize < minimumSize)
        {
            targetSize <<= 1;
        }

        return targetSize;
    }

    private bool IsStandardSize(int size)
    {
        return Array.IndexOf(_standardSizes, size) >= 0 || IsPowerOfTwo(size);
    }

    private static bool IsPowerOfTwo(int value)
    {
        return value > 0 && (value & (value - 1)) == 0;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ComponentArrayPool<T>));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _pools.Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Statistics about pool usage
    /// </summary>
    public class PoolStatistics
    {
        public Dictionary<int, int> PoolSizes { get; } = new Dictionary<int, int>();
        public int TotalPooledArrays { get; set; }
        
        public override string ToString()
        {
            return $"ComponentArrayPool<{typeof(T).Name}>: {TotalPooledArrays} total arrays across {PoolSizes.Count} size pools";
        }
    }
}

/// <summary>
/// Static component array pool manager for global access
/// </summary>
public static class ComponentArrayPools
{
    private static readonly ConcurrentDictionary<Type, object> _pools = new();

    /// <summary>
    /// Get the pool for a specific component type
    /// </summary>
    public static ComponentArrayPool<T> Get<T>() where T : Component
    {
        return (ComponentArrayPool<T>)_pools.GetOrAdd(typeof(T), _ => new ComponentArrayPool<T>());
    }

    /// <summary>
    /// Clear all pools
    /// </summary>
    public static void ClearAll()
    {
        foreach (var pool in _pools.Values)
        {
            if (pool is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _pools.Clear();
    }

    /// <summary>
    /// Trim excess from all pools
    /// </summary>
    public static void TrimExcessAll(int targetPoolSize = 10)
    {
        foreach (var pool in _pools.Values)
        {
            if (pool is ComponentArrayPool<Component> componentPool)
            {
                // Use reflection to call TrimExcess
                var method = pool.GetType().GetMethod("TrimExcess");
                method?.Invoke(pool, new object[] { targetPoolSize });
            }
        }
    }
}