using Engine.Application.Rendering;
using Engine.Domain.Rendering.Services;
using Engine.Infrastructure.Logging;

namespace Engine.Application.Rendering;

/// <summary>
/// Application service for rendering orchestration
/// </summary>
public class Renderer
{
    private readonly RenderingPipeline _pipeline;
    private readonly Logger _logger;

    public Renderer(RenderingPipeline pipeline, Logger logger)
    {
        _pipeline = pipeline;
        _logger = logger;
    }

    public void SubmitRenderCommand(RenderCommand command)
    {
        var domainEvent = new Engine.Domain.Rendering.Events.RenderCommandQueued(
            command.MeshId,
            command.MaterialId,
            command.InstanceCount
        );
        _pipeline.QueueRenderCommand(domainEvent);
    }

    public void Render()
    {
        var commands = _pipeline.GetRenderCommands();
        foreach (var command in commands)
        {
            // Render logic would go here
            _logger.Debug($"Rendering mesh: {command.MeshId} with material: {command.MaterialId}");
        }
        _pipeline.ClearCommands();
    }
}

