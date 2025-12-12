using Engine.Domain.ECS;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Transform component - value object (position, rotation, scale)
/// </summary>
public class Transform : Component
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
    }

    public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
    
    /// <summary>
    /// Get world transformation matrix
    /// </summary>
    public Matrix4x4 GetWorldMatrix()
    {
        return Matrix4x4.CreateScale(Scale) *
               Matrix4x4.CreateFromQuaternion(Rotation) *
               Matrix4x4.CreateTranslation(Position);
    }
    
    /// <summary>
    /// Get view matrix (inverse of world matrix)
    /// </summary>
    public Matrix4x4 GetViewMatrix()
    {
        var worldMatrix = GetWorldMatrix();
        return Matrix4x4.Invert(worldMatrix, out var result) ? result : Matrix4x4.Identity;
    }
    
    /// <summary>
    /// Forward direction vector
    /// </summary>
    public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Rotation);
    
    /// <summary>
    /// Right direction vector
    /// </summary>
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);
    
    /// <summary>
    /// Up direction vector
    /// </summary>
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);

    /// <summary>
    /// Identity transform (zero position, identity rotation, unit scale)
    /// </summary>
    public static Transform Identity => new Transform();

    /// <summary>
    /// Combine two transforms (parent * child)
    /// </summary>
    public static Transform Combine(Transform parent, Transform child)
    {
        if (parent == null) return child;
        if (child == null) return parent;

        var combinedPosition = parent.Position + Vector3.Transform(child.Position, parent.Rotation);
        var combinedRotation = parent.Rotation * child.Rotation;
        var combinedScale = parent.Scale * child.Scale;

        return new Transform(combinedPosition, combinedRotation, combinedScale);
    }

    /// <summary>
    /// Create a copy of this transform
    /// </summary>
    public Transform Clone()
    {
        return new Transform(Position, Rotation, Scale);
    }
}



