namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Swapchain implementation
/// </summary>
public class VulkanSwapchain
{
    private readonly VulkanContext _context;

    public VulkanSwapchain(VulkanContext context)
    {
        _context = context;
    }

    public void Create(int width, int height)
    {
        // Swapchain creation logic
    }

    public void Recreate(int width, int height)
    {
        // Swapchain recreation logic
    }

    public void Destroy()
    {
        // Swapchain destruction logic
    }
}

