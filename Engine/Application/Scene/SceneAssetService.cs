using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;
using Engine.Infrastructure.Resources;

namespace Engine.Application.Scene;

/// <summary>
/// Application service for managing assets within scene contexts.
/// Handles asset loading, caching, and lifecycle management for scenes.
/// </summary>
public class SceneAssetService
{
    private readonly AssetLoader _assetLoader;
    private readonly AssetCache _assetCache;
    private readonly Dictionary<string, HashSet<AssetGuid>> _sceneAssets;
    private readonly Dictionary<AssetGuid, int> _assetReferenceCounts;

    public SceneAssetService(AssetLoader assetLoader, AssetCache assetCache)
    {
        _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
        _assetCache = assetCache ?? throw new ArgumentNullException(nameof(assetCache));
        _sceneAssets = new Dictionary<string, HashSet<AssetGuid>>();
        _assetReferenceCounts = new Dictionary<AssetGuid, int>();
    }

    /// <summary>
    /// Load an asset and associate it with a scene
    /// </summary>
    /// <typeparam name="T">Type of asset to load</typeparam>
    /// <param name="assetId">Asset identifier</param>
    /// <param name="sceneId">Scene that will use this asset</param>
    /// <returns>The loaded asset</returns>
    public async Task<T> LoadAssetAsync<T>(AssetGuid assetId, string sceneId) where T : class
    {
        // Check cache first
        var cachedAsset = _assetCache.Get(assetId) as T;
        if (cachedAsset != null)
        {
            IncrementReference(assetId, sceneId);
            return cachedAsset;
        }

        // For Phase 4.5, we're focusing on the asset integration framework
        // Actual asset loading from files will be implemented in future phases
        throw new NotImplementedException("Asset loading from AssetGuid will be implemented in future phases. Use RegisterAsset for now.");
    }

    /// <summary>
    /// Register an asset with a scene without loading (for pre-loaded assets)
    /// </summary>
    /// <typeparam name="T">Type of asset</typeparam>
    /// <param name="assetId">Asset identifier</param>
    /// <param name="asset">Pre-loaded asset instance</param>
    /// <param name="sceneId">Scene that will use this asset</param>
    public void RegisterAsset<T>(AssetGuid assetId, T asset, string sceneId) where T : Asset
    {
        _assetCache.Add(asset);
        IncrementReference(assetId, sceneId);
    }

    /// <summary>
    /// Unload an asset from a specific scene context
    /// </summary>
    /// <param name="assetId">Asset to unload</param>
    /// <param name="sceneId">Scene releasing the asset</param>
    public void UnloadAsset(AssetGuid assetId, string sceneId)
    {
        DecrementReference(assetId, sceneId);
    }

    /// <summary>
    /// Unload all assets associated with a scene
    /// </summary>
    /// <param name="sceneId">Scene identifier</param>
    public void UnloadSceneAssets(string sceneId)
    {
        if (!_sceneAssets.TryGetValue(sceneId, out var assets))
            return;

        // Create copy of asset list to avoid modification during iteration
        var assetsToUnload = new List<AssetGuid>(assets);
        
        foreach (var assetId in assetsToUnload)
        {
            DecrementReference(assetId, sceneId);
        }

        _sceneAssets.Remove(sceneId);
    }

    /// <summary>
    /// Get all assets currently loaded for a scene
    /// </summary>
    /// <param name="sceneId">Scene identifier</param>
    /// <returns>Collection of asset identifiers</returns>
    public IReadOnlyCollection<AssetGuid> GetSceneAssets(string sceneId)
    {
        if (_sceneAssets.TryGetValue(sceneId, out var assets))
        {
            return assets;
        }
        
        return Array.Empty<AssetGuid>();
    }

    /// <summary>
    /// Get the current reference count for an asset
    /// </summary>
    /// <param name="assetId">Asset identifier</param>
    /// <returns>Number of scenes referencing this asset</returns>
    public int GetReferenceCount(AssetGuid assetId)
    {
        return _assetReferenceCounts.TryGetValue(assetId, out var count) ? count : 0;
    }

    /// <summary>
    /// Check if an asset is currently loaded
    /// </summary>
    /// <param name="assetId">Asset identifier</param>
    /// <returns>True if asset is in cache</returns>
    public bool IsAssetLoaded(AssetGuid assetId)
    {
        return _assetCache.Get(assetId) != null;
    }

    private void IncrementReference(AssetGuid assetId, string sceneId)
    {
        // Track scene -> assets mapping
        if (!_sceneAssets.TryGetValue(sceneId, out var sceneAssetSet))
        {
            sceneAssetSet = new HashSet<AssetGuid>();
            _sceneAssets[sceneId] = sceneAssetSet;
        }
        sceneAssetSet.Add(assetId);

        // Track asset reference count
        _assetReferenceCounts.TryGetValue(assetId, out var currentCount);
        _assetReferenceCounts[assetId] = currentCount + 1;
    }

    private void DecrementReference(AssetGuid assetId, string sceneId)
    {
        // Remove from scene assets
        if (_sceneAssets.TryGetValue(sceneId, out var sceneAssetSet))
        {
            sceneAssetSet.Remove(assetId);
        }

        // Decrement reference count
        if (_assetReferenceCounts.TryGetValue(assetId, out var currentCount))
        {
            var newCount = currentCount - 1;
            if (newCount <= 0)
            {
                // No more references, remove from cache
                _assetReferenceCounts.Remove(assetId);
                _assetCache.Remove(assetId);
            }
            else
            {
                _assetReferenceCounts[assetId] = newCount;
            }
        }
    }
}