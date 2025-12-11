using Engine.Application.Rendering;
using Engine.Domain.Rendering.Services;

namespace Engine.Application.Rendering;

/// <summary>
/// Application service coordinating rendering
/// </summary>
public class RenderingService
{
    private readonly Renderer _renderer;
    private readonly MaterialService _materialService;
    private readonly CameraService _cameraService;

    public RenderingService(Renderer renderer, MaterialService materialService, CameraService cameraService)
    {
        _renderer = renderer;
        _materialService = materialService;
        _cameraService = cameraService;
    }

    public void RenderFrame()
    {
        _renderer.Render();
    }
}

