using System.Collections.Generic;
using Engine.Domain.Rendering.Events;

namespace Engine.Domain.Rendering.Services;

/// <summary>
/// Domain service orchestrating render passes
/// </summary>
public class RenderingPipeline
{
    private readonly List<RenderCommandQueued> _renderCommands = new();

    public void QueueRenderCommand(RenderCommandQueued command)
    {
        _renderCommands.Add(command);
    }

    public IEnumerable<RenderCommandQueued> GetRenderCommands() => _renderCommands;

    public void ClearCommands()
    {
        _renderCommands.Clear();
    }
}

