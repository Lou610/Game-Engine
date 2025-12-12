using Engine.Application.ECS;
using Engine.Domain.ECS;
using Engine.Domain.ECS.Components;
using Engine.Domain.Rendering;
using System.Numerics;
using ECSSystem = Engine.Application.ECS.System;

namespace Engine.Application.Rendering;

/// <summary>
/// System responsible for rendering entities with mesh and transform components.
/// Collects all renderable entities and submits them to the rendering service.
/// </summary>
public class MeshRenderSystem : ECSSystem
{
    private readonly IRenderer _renderer;
    
    public MeshRenderSystem(IRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }
    
    public override void Update(float deltaTime)
    {
        if (World == null) return;
        
        // Get the main camera (for now, just take the first one we find)
        Domain.ECS.Components.Camera? mainCamera = null;
        Transform? cameraTransform = null;
        
        foreach (var entity in World.Query<Domain.ECS.Components.Camera, Transform>())
        {
            var camera = World.GetComponent<Domain.ECS.Components.Camera>(entity.Id);
            var transform = World.GetComponent<Transform>(entity.Id);
            
            if (camera?.IsEnabled == true)
            {
                mainCamera = camera;
                cameraTransform = transform;
                break;
            }
        }
        
        if (mainCamera == null || cameraTransform == null)
        {
            // No active camera found, skip rendering
            return;
        }
        
        // Begin frame
        _renderer.BeginFrame();
        
        // Set camera matrices
        var viewMatrix = cameraTransform.GetViewMatrix();
        var projectionMatrix = mainCamera.GetProjectionMatrix(16.0f / 9.0f); // Default 16:9 aspect ratio
        _renderer.SetCamera(mainCamera, viewMatrix, projectionMatrix);
        
        // Render all entities with MeshRenderer and Transform components
        foreach (var entity in World.Query<MeshRenderer, Transform>())
        {
            var meshRenderer = World.GetComponent<MeshRenderer>(entity.Id);
            var transform = World.GetComponent<Transform>(entity.Id);
            
            if (meshRenderer?.Mesh == null || meshRenderer?.Material == null || transform == null) continue;
            
            var worldMatrix = transform.GetWorldMatrix();
            _renderer.DrawMesh(meshRenderer.Mesh, meshRenderer.Material, worldMatrix);
        }
        
        // End frame
        _renderer.EndFrame();
    }
}