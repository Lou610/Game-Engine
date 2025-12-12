using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.Rendering;
using Engine.Domain.Scene;
using Engine.Domain.Scene.Prefabs;
using Engine.Domain.Scene.ValueObjects;
using Engine.Infrastructure.Logging;

namespace Engine.Application.Scene;

/// <summary>
/// Application service for instantiating prefabs into scenes
/// </summary>
public class PrefabInstantiator
{
    private readonly PrefabFactory _prefabFactory;
    private readonly Logger _logger;
    private readonly Dictionary<Guid, int> _usageStats = new();
    private int _totalInstantiations = 0;
    private double _totalInstantiationTime = 0;

    public PrefabInstantiator(PrefabFactory prefabFactory, Logger logger)
    {
        _prefabFactory = prefabFactory ?? throw new ArgumentNullException(nameof(prefabFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Instantiate a prefab into a scene
    /// </summary>
    public async Task<PrefabInstantiationResult> InstantiatePrefabAsync(PrefabInstantiationRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.TargetScene == null)
            throw new ArgumentException("Target scene cannot be null", nameof(request));

        var stopwatch = Stopwatch.StartNew();
        var result = new PrefabInstantiationResult();

        try
        {
            var prefab = _prefabFactory.GetPrefab(new PrefabId(request.PrefabId.ToString()));
            if (prefab == null)
            {
                result.Success = false;
                result.ErrorMessage = $"Prefab with ID {request.PrefabId} not found";
                return result;
            }

            // Create the root entity
            var rootEntity = await CreateEntityFromPrefab(prefab, request);
            result.RootEntity = rootEntity;
            result.CreatedEntities.Add(rootEntity);

            // Create scene node for the entity
            var sceneNode = CreateSceneNodeForEntity(rootEntity, request);
            result.CreatedNodes.Add(sceneNode);

            // Instantiate children recursively if requested
            if (request.InstantiateChildren && request.MaxDepth > 0)
            {
                await InstantiateChildPrefabs(prefab, sceneNode, request, result, 1);
            }

            result.Success = true;
            UpdateStatistics(request.PrefabId, stopwatch.Elapsed.TotalMilliseconds);
            
            _logger.Info($"Successfully instantiated prefab '{prefab.Name}' with {result.CreatedEntities.Count} entities");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.Error($"Failed to instantiate prefab {request.PrefabId}: {ex.Message}");
        }
        finally
        {
            stopwatch.Stop();
            result.Metadata["InstantiationTimeMs"] = stopwatch.Elapsed.TotalMilliseconds;
        }

        return result;
    }

    /// <summary>
    /// Instantiate a prefab by name
    /// </summary>
    public async Task<PrefabInstantiationResult> InstantiatePrefabAsync(string prefabName, Engine.Domain.Scene.Scene targetScene, Transform? worldTransform = null, SceneNode? parentNode = null)
    {
        var prefab = _prefabFactory.GetPrefab(prefabName);
        if (prefab == null)
        {
            return new PrefabInstantiationResult
            {
                Success = false,
                ErrorMessage = $"Prefab with name '{prefabName}' not found"
            };
        }

        var request = new PrefabInstantiationRequest
        {
            PrefabId = Guid.Parse(prefab.Id.Value),
            TargetScene = targetScene,
            WorldTransform = worldTransform,
            ParentNode = parentNode
        };

        return await InstantiatePrefabAsync(request);
    }

    /// <summary>
    /// Create multiple instances of a prefab
    /// </summary>
    public async Task<List<PrefabInstantiationResult>> InstantiateMultipleAsync(PrefabInstantiationRequest template, int count, Func<int, Transform>? positionGenerator = null)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));
        if (count <= 0)
            throw new ArgumentException("Count must be positive", nameof(count));

        var results = new List<PrefabInstantiationResult>();
        var tasks = new List<Task<PrefabInstantiationResult>>();

        for (int i = 0; i < count; i++)
        {
            var instanceRequest = new PrefabInstantiationRequest
            {
                PrefabId = template.PrefabId,
                TargetScene = template.TargetScene,
                ParentNode = template.ParentNode,
                WorldTransform = positionGenerator?.Invoke(i) ?? template.WorldTransform,
                ComponentOverrides = new Dictionary<Type, object>(template.ComponentOverrides),
                CustomName = template.CustomName != null ? $"{template.CustomName}_{i}" : null,
                InstantiateChildren = template.InstantiateChildren,
                MaxDepth = template.MaxDepth
            };

            tasks.Add(InstantiatePrefabAsync(instanceRequest));
        }

        var taskResults = await Task.WhenAll(tasks);
        results.AddRange(taskResults);

        _logger.Info($"Instantiated {count} instances of prefab {template.PrefabId}");
        return results;
    }

    /// <summary>
    /// Get instantiation statistics
    /// </summary>
    public PrefabStatistics GetStatistics()
    {
        return new PrefabStatistics
        {
            TotalInstantiations = _totalInstantiations,
            AverageInstantiationTime = _totalInstantiations > 0 ? _totalInstantiationTime / _totalInstantiations : 0,
            UsageCount = new Dictionary<Guid, int>(_usageStats)
        };
    }

    /// <summary>
    /// Clear instantiation statistics
    /// </summary>
    public void ClearStatistics()
    {
        _usageStats.Clear();
        _totalInstantiations = 0;
        _totalInstantiationTime = 0;
    }

    /// <summary>
    /// Create an entity from a prefab
    /// </summary>
    private async Task<Entity> CreateEntityFromPrefab(Prefab prefab, PrefabInstantiationRequest request)
    {
        // Create the entity
        var entityName = request.CustomName ?? prefab.Name;
        var entityId = request.TargetScene.CreateEntity(entityName);
        var entity = request.TargetScene.Entities.GetEntity(entityId);
        if (entity == null)
            throw new InvalidOperationException($"Failed to create entity '{entityName}'");

        // Get all components including inherited ones
        var allComponents = prefab.GetAllComponents();

        // Apply components to the entity
        foreach (var (componentType, template) in allComponents)
        {
            // Check for component overrides
            if (request.ComponentOverrides.TryGetValue(componentType, out var overrideComponent))
            {
                if (overrideComponent is Component component)
                {
                    request.TargetScene.Entities.AddComponent(entity.Id, component);
                }
            }
            else
            {
                // Clone the template component
                var clonedComponent = CloneComponent(template);
                
                // Apply world transform if it's a Transform component and we have a world transform
                if (clonedComponent is Transform transform && request.WorldTransform != null)
                {
                    transform.Position = request.WorldTransform.Position;
                    transform.Rotation = request.WorldTransform.Rotation;
                    transform.Scale = request.WorldTransform.Scale;
                }
                
                request.TargetScene.Entities.AddComponent(entity.Id, clonedComponent);
            }
        }

        // Simulate async operation for complex prefabs
        if (allComponents.Count > 5)
        {
            await Task.Delay(1); // Small delay for complex prefabs
        }

        return entity;
    }

    /// <summary>
    /// Create a scene node for an entity
    /// </summary>
    private SceneNode CreateSceneNodeForEntity(Entity entity, PrefabInstantiationRequest request)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "Entity cannot be null when creating scene node");
            
        // Get the entity's transform
        var transform = request.TargetScene.Entities.HasComponent<Transform>(entity.Id)
            ? request.TargetScene.Entities.GetComponent<Transform>(entity.Id)
            : Transform.Identity;

        // Create scene node
        var sceneNode = request.TargetScene.SceneGraph.CreateNode(entity.Id, request.ParentNode, entity.Name);

        // Add to parent or root
        if (request.ParentNode != null)
        {
            request.ParentNode.AddChild(sceneNode);
        }
        else
        {
            request.TargetScene.SceneGraph.Root.AddChild(sceneNode);
        }

        return sceneNode ?? throw new InvalidOperationException("Failed to create scene node");
    }

    /// <summary>
    /// Recursively instantiate child prefabs
    /// </summary>
    private async Task InstantiateChildPrefabs(Prefab prefab, SceneNode parentNode, PrefabInstantiationRequest originalRequest, PrefabInstantiationResult result, int currentDepth)
    {
        if (currentDepth >= originalRequest.MaxDepth)
        {
            _logger.Warning($"Reached maximum depth {originalRequest.MaxDepth} while instantiating child prefabs");
            return;
        }

        foreach (var childPrefabInfo in prefab.Children)
        {
            try
            {
                var childRequest = new PrefabInstantiationRequest
                {
                    PrefabId = Guid.Parse(childPrefabInfo.Prefab.Id.Value),
                    TargetScene = originalRequest.TargetScene,
                    ParentNode = parentNode,
                    WorldTransform = childPrefabInfo.RelativeTransform,
                    CustomName = childPrefabInfo.NameOverride ?? childPrefabInfo.Prefab.Name,
                    InstantiateChildren = originalRequest.InstantiateChildren,
                    MaxDepth = originalRequest.MaxDepth
                };

                var childEntity = await CreateEntityFromPrefab(childPrefabInfo.Prefab, childRequest);
                var childNode = CreateSceneNodeForEntity(childEntity, childRequest);

                result.CreatedEntities.Add(childEntity);
                result.CreatedNodes.Add(childNode);

                // Recursively instantiate grandchildren
                await InstantiateChildPrefabs(childPrefabInfo.Prefab, childNode, originalRequest, result, currentDepth + 1);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to instantiate child prefab '{childPrefabInfo.Prefab.Name}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Clone a component for entity instantiation
    /// </summary>
    private Component CloneComponent(Component original)
    {
        return original switch
        {
            Transform transform => new Transform
            {
                Position = transform.Position,
                Rotation = transform.Rotation,
                Scale = transform.Scale
            },
            _ => original // In practice, all components should be properly cloneable
        };
    }

    /// <summary>
    /// Update usage statistics
    /// </summary>
    private void UpdateStatistics(Guid prefabId, double instantiationTimeMs)
    {
        _totalInstantiations++;
        _totalInstantiationTime += instantiationTimeMs;
        
        if (_usageStats.ContainsKey(prefabId))
            _usageStats[prefabId]++;
        else
            _usageStats[prefabId] = 1;
    }
}