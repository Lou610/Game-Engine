using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.Rendering.ValueObjects;

namespace Engine.Domain.ECS.Components;

/// <summary>
/// Transform component - value object (position, rotation, scale)
/// </summary>
public class Transform : Component
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public Transform()
    {
        Position = Vector3.Zero;
        Rotation = Vector3.Zero;
        Scale = Vector3.One;
    }

    public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}



