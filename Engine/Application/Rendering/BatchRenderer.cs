using System.Collections.Generic;
using Engine.Application.Rendering;

namespace Engine.Application.Rendering;

/// <summary>
/// Application service for batching
/// </summary>
public class BatchRenderer
{
    private readonly List<RenderCommand> _batch = new();

    public void AddToBatch(RenderCommand command)
    {
        _batch.Add(command);
    }

    public void RenderBatch()
    {
        // Batch rendering logic
        _batch.Clear();
    }
}

