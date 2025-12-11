using System;

namespace Engine.Domain.Input.ValueObjects;

/// <summary>
/// Input axis value (-1.0 to 1.0)
/// </summary>
public readonly record struct InputAxis
{
    public float Value { get; init; }

    public InputAxis(float value)
    {
        Value = Math.Clamp(value, -1.0f, 1.0f);
    }

    public static InputAxis Zero => new(0.0f);
}

