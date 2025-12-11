namespace Engine.Domain.Core;

/// <summary>
/// Value object representing time information (delta time, fixed time)
/// </summary>
public readonly record struct Time
{
    public float DeltaTime { get; init; }
    public float FixedDeltaTime { get; init; }
    public float TotalTime { get; init; }
    public float FixedTotalTime { get; init; }

    public Time(float deltaTime, float fixedDeltaTime, float totalTime, float fixedTotalTime)
    {
        DeltaTime = deltaTime;
        FixedDeltaTime = fixedDeltaTime;
        TotalTime = totalTime;
        FixedTotalTime = fixedTotalTime;
    }

    public static Time Zero => new(0f, 0f, 0f, 0f);
}

