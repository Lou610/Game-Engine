namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Texture implementation
/// </summary>
public class VulkanTexture
{
    private readonly VulkanContext _context;

    public VulkanTexture(VulkanContext context)
    {
        _context = context;
    }

    public void Create(int width, int height, byte[] data)
    {
        // Texture creation logic
    }

    public void Destroy()
    {
        // Texture destruction logic
    }
}

