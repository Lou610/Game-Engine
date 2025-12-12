using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Scene;

/// <summary>
/// Registry for tracking and managing assets within a scene context
/// </summary>
public class AssetRegistry
{
    private readonly Dictionary<AssetGuid, Asset> _assets;
    private readonly Dictionary<AssetType, HashSet<AssetGuid>> _assetsByType;
    private readonly HashSet<AssetGuid> _requiredAssets;

    public AssetRegistry()
    {
        _assets = new Dictionary<AssetGuid, Asset>();
        _assetsByType = new Dictionary<AssetType, HashSet<AssetGuid>>();
        _requiredAssets = new HashSet<AssetGuid>();
    }

    /// <summary>
    /// All assets in the registry
    /// </summary>
    public IReadOnlyCollection<Asset> Assets => _assets.Values;

    /// <summary>
    /// All asset IDs in the registry
    /// </summary>
    public IReadOnlyCollection<AssetGuid> AssetIds => _assets.Keys;

    /// <summary>
    /// Assets that are required for the scene to function properly
    /// </summary>
    public IReadOnlyCollection<AssetGuid> RequiredAssets => _requiredAssets;

    /// <summary>
    /// Register an asset with the scene
    /// </summary>
    /// <param name="asset">Asset to register</param>
    /// <param name="isRequired">Whether this asset is required for scene functionality</param>
    public void RegisterAsset(Asset asset, bool isRequired = false)
    {
        if (asset == null)
            throw new ArgumentNullException(nameof(asset));

        _assets[asset.Id] = asset;

        // Track by type
        if (!_assetsByType.TryGetValue(asset.Type, out var assetsOfType))
        {
            assetsOfType = new HashSet<AssetGuid>();
            _assetsByType[asset.Type] = assetsOfType;
        }
        assetsOfType.Add(asset.Id);

        // Track required status
        if (isRequired)
        {
            _requiredAssets.Add(asset.Id);
        }
    }

    /// <summary>
    /// Unregister an asset from the scene
    /// </summary>
    /// <param name="assetId">Asset ID to unregister</param>
    public bool UnregisterAsset(AssetGuid assetId)
    {
        if (!_assets.TryGetValue(assetId, out var asset))
            return false;

        _assets.Remove(assetId);

        // Remove from type tracking
        if (_assetsByType.TryGetValue(asset.Type, out var assetsOfType))
        {
            assetsOfType.Remove(assetId);
            if (assetsOfType.Count == 0)
            {
                _assetsByType.Remove(asset.Type);
            }
        }

        // Remove from required tracking
        _requiredAssets.Remove(assetId);

        return true;
    }

    /// <summary>
    /// Get an asset by ID
    /// </summary>
    /// <param name="assetId">Asset identifier</param>
    /// <returns>Asset if found, null otherwise</returns>
    public Asset? GetAsset(AssetGuid assetId)
    {
        return _assets.TryGetValue(assetId, out var asset) ? asset : null;
    }

    /// <summary>
    /// Get a typed asset by ID
    /// </summary>
    /// <typeparam name="T">Asset type</typeparam>
    /// <param name="assetId">Asset identifier</param>
    /// <returns>Typed asset if found and correct type, null otherwise</returns>
    public T? GetAsset<T>(AssetGuid assetId) where T : Asset
    {
        return GetAsset(assetId) as T;
    }

    /// <summary>
    /// Get all assets of a specific type
    /// </summary>
    /// <param name="assetType">Type of assets to retrieve</param>
    /// <returns>Collection of assets of the specified type</returns>
    public IReadOnlyCollection<Asset> GetAssetsByType(AssetType assetType)
    {
        if (_assetsByType.TryGetValue(assetType, out var assetIds))
        {
            return assetIds.Select(id => _assets[id]).ToList();
        }
        
        return Array.Empty<Asset>();
    }

    /// <summary>
    /// Get all assets of a specific type (strongly typed)
    /// </summary>
    /// <typeparam name="T">Asset type</typeparam>
    /// <returns>Collection of typed assets</returns>
    public IReadOnlyCollection<T> GetAssetsByType<T>() where T : Asset
    {
        return Assets.OfType<T>().ToList();
    }

    /// <summary>
    /// Check if an asset is registered
    /// </summary>
    /// <param name="assetId">Asset identifier</param>
    /// <returns>True if asset is registered</returns>
    public bool HasAsset(AssetGuid assetId)
    {
        return _assets.ContainsKey(assetId);
    }

    /// <summary>
    /// Check if an asset is marked as required
    /// </summary>
    /// <param name="assetId">Asset identifier</param>
    /// <returns>True if asset is required</returns>
    public bool IsRequired(AssetGuid assetId)
    {
        return _requiredAssets.Contains(assetId);
    }

    /// <summary>
    /// Mark an asset as required
    /// </summary>
    /// <param name="assetId">Asset identifier</param>
    public void SetRequired(AssetGuid assetId, bool required = true)
    {
        if (!_assets.ContainsKey(assetId))
            throw new ArgumentException($"Asset {assetId.Value} is not registered");

        if (required)
        {
            _requiredAssets.Add(assetId);
        }
        else
        {
            _requiredAssets.Remove(assetId);
        }
    }

    /// <summary>
    /// Get count of assets by type
    /// </summary>
    /// <param name="assetType">Asset type</param>
    /// <returns>Number of assets of the specified type</returns>
    public int GetAssetCount(AssetType assetType)
    {
        return _assetsByType.TryGetValue(assetType, out var assets) ? assets.Count : 0;
    }

    /// <summary>
    /// Get total number of registered assets
    /// </summary>
    public int TotalAssetCount => _assets.Count;

    /// <summary>
    /// Clear all assets from the registry
    /// </summary>
    public void Clear()
    {
        _assets.Clear();
        _assetsByType.Clear();
        _requiredAssets.Clear();
    }
}