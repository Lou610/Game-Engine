namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Pipeline implementation
/// </summary>
public class VulkanPipeline
{
    private readonly VulkanContext _context;

    public VulkanPipeline(VulkanContext context)
    {
        _context = context;
    }

    public void Create(string vertexShader, string fragmentShader)
    {
        // Pipeline creation logic
    }

    public void Destroy()
    {
        // Pipeline destruction logic
    }
}

