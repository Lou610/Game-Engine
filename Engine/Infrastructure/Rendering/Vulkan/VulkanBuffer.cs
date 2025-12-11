namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Buffer implementation
/// </summary>
public class VulkanBuffer
{
    private readonly VulkanContext _context;
    public BufferType Type { get; private set; }

    public VulkanBuffer(VulkanContext context)
    {
        _context = context;
    }

    public void Create(BufferType type, ulong size)
    {
        Type = type;
        // Buffer creation logic
    }

    public void UploadData<T>(T[] data) where T : struct
    {
        // Data upload logic
    }

    public void Destroy()
    {
        // Buffer destruction logic
    }
}

public enum BufferType
{
    Vertex,
    Index,
    Uniform
}

