namespace Engine.Domain.Rendering.ValueObjects;

/// <summary>
/// Shader identifier
/// </summary>
public readonly record struct ShaderId(System.Guid Value)
{
    public static ShaderId NewId() => new(System.Guid.NewGuid());
    public static ShaderId Empty => new(System.Guid.Empty);
    
    public override string ToString() => Value.ToString();
}

