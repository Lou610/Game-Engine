using System;

namespace Engine.Domain.Resources.ValueObjects;

/// <summary>
/// Globally unique asset identifier
/// </summary>
public readonly record struct AssetGuid
{
    public Guid Value { get; init; }

    public AssetGuid(Guid value)
    {
        Value = value;
    }

    public static AssetGuid NewGuid() => new(Guid.NewGuid());
    public static AssetGuid Invalid => new(Guid.Empty);
}

