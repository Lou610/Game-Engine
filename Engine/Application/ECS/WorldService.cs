using Engine.Domain.ECS;
using Engine.Domain.ECS.Services;
using Engine.Infrastructure.Logging;

namespace Engine.Application.ECS;

/// <summary>
/// Application service orchestrating world operations
/// </summary>
public class WorldService
{
    private readonly World _world;
    private readonly EntityLifecycleService _lifecycleService;
    private readonly ComponentArchetypeService _archetypeService;
    private readonly Logger _logger;

    public WorldService(World world, EntityLifecycleService lifecycleService, ComponentArchetypeService archetypeService, Logger logger)
    {
        _world = world;
        _lifecycleService = lifecycleService;
        _archetypeService = archetypeService;
        _logger = logger;
    }

    public Entity CreateEntity(string name = "")
    {
        var entity = _world.CreateEntity(name);
        _lifecycleService.CreateEntity(entity.Id, entity.Name);
        _logger.Debug($"Created entity: {entity.Name} (ID: {entity.Id.Value})");
        return entity;
    }

    public void DestroyEntity(Engine.Domain.ECS.ValueObjects.EntityId id)
    {
        _lifecycleService.DestroyEntity(id);
        _world.DestroyEntity(id);
        _logger.Debug($"Destroyed entity: {id.Value}");
    }
}

