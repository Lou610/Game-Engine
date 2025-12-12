using System;
using System.Numerics;

namespace Engine.Domain.Rendering;

/// <summary>
/// Core renderer interface for graphics abstraction
/// </summary>
public interface IRenderer : IDisposable
{
    /// <summary>
    /// Initialize the renderer with window handle and dimensions
    /// </summary>
    void Initialize(nint windowHandle, uint width, uint height);
    
    /// <summary>
    /// Begin a new frame for rendering
    /// </summary>
    void BeginFrame();
    
    /// <summary>
    /// End the current frame and present
    /// </summary>
    void EndFrame();
    
    /// <summary>
    /// Draw a mesh with material and transform
    /// </summary>
    void DrawMesh(Entities.Mesh mesh, Entities.Material material, Matrix4x4 transform);
    
    /// <summary>
    /// Set the active camera for rendering
    /// </summary>
    void SetCamera(ECS.Components.Camera camera, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix);
    
    /// <summary>
    /// Handle window resize
    /// </summary>
    void Resize(uint width, uint height);
    
    /// <summary>
    /// Check if renderer is initialized
    /// </summary>
    bool IsInitialized { get; }
}