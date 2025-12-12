using System;
using Engine.Domain.ECS;
using Engine.Domain.Rendering.ValueObjects;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Quaternion = System.Numerics.Quaternion;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Camera component for rendering
/// </summary>
public class Camera : Component
{
    public ProjectionType ProjectionType { get; set; } = ProjectionType.Perspective;
    public float FieldOfView { get; set; } = 60.0f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000.0f;
    public float OrthographicSize { get; set; } = 5.0f;
    public bool IsMainCamera { get; set; } = false;
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Get view matrix from transform component
    /// </summary>
    public Matrix4x4 GetViewMatrix(Transform transform)
    {
        var position = transform.Position;
        var rotation = transform.Rotation;
        
        // Create view matrix from transform
        var worldMatrix = Matrix4x4.CreateFromQuaternion(rotation) * 
                         Matrix4x4.CreateTranslation(position);
        
        return Matrix4x4.Invert(worldMatrix, out var viewMatrix) ? viewMatrix : Matrix4x4.Identity;
    }
    
    /// <summary>
    /// Get projection matrix based on aspect ratio
    /// </summary>
    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return ProjectionType switch
        {
            ProjectionType.Perspective => Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI / 180.0f * FieldOfView, aspectRatio, NearPlane, FarPlane),
            
            ProjectionType.Orthographic => Matrix4x4.CreateOrthographic(
                OrthographicSize * aspectRatio, OrthographicSize, NearPlane, FarPlane),
            
            _ => Matrix4x4.Identity
        };
    }
    
    /// <summary>
    /// Convert screen point to world ray
    /// </summary>
    public Ray ScreenPointToRay(Vector2 screenPoint, Vector2 screenSize, Transform transform)
    {
        var aspectRatio = screenSize.X / screenSize.Y;
        var projMatrix = GetProjectionMatrix(aspectRatio);
        var viewMatrix = GetViewMatrix(transform);
        
        // Convert screen coordinates to normalized device coordinates
        var ndc = new Vector2(
            (2.0f * screenPoint.X) / screenSize.X - 1.0f,
            1.0f - (2.0f * screenPoint.Y) / screenSize.Y
        );
        
        // Create ray in clip space
        var clipNear = new Vector4(ndc.X, ndc.Y, -1.0f, 1.0f);
        var clipFar = new Vector4(ndc.X, ndc.Y, 1.0f, 1.0f);
        
        // Transform to world space
        var invViewProj = Matrix4x4.Invert(viewMatrix * projMatrix, out var invMatrix) ? invMatrix : Matrix4x4.Identity;
        
        var worldNear = Vector4.Transform(clipNear, invViewProj);
        var worldFar = Vector4.Transform(clipFar, invViewProj);
        
        worldNear /= worldNear.W;
        worldFar /= worldFar.W;
        
        var origin = new Vector3(worldNear.X, worldNear.Y, worldNear.Z);
        var direction = Vector3.Normalize(new Vector3(worldFar.X, worldFar.Y, worldFar.Z) - origin);
        
        return new Ray(origin, direction);
    }
}

/// <summary>
/// Ray structure for camera calculations
/// </summary>
public readonly record struct Ray(Vector3 Origin, Vector3 Direction)
{
    public Vector3 GetPoint(float distance) => Origin + Direction * distance;
}

