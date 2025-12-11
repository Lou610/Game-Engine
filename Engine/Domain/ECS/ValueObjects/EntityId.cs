namespace Engine.Domain.ECS.ValueObjects;

/// <summary>
/// Value object for entity identification
/// </summary>
public readonly record struct EntityId
{
    public ulong Value { get; init; }

    public EntityId(ulong value)
    {
        Value = value;
    }

    public static EntityId Invalid => new(0);
    public static bool operator ==(EntityId left, EntityId right) => left.Value == right.Value;
    public static bool operator !=(EntityId left, EntityId right) => !(left == right);
}

