using Engine.Domain.Core;

namespace Engine.Domain.Core.Services;

/// <summary>
/// Domain service for time management
/// </summary>
public class TimeService
{
    private float _totalTime;
    private float _fixedTotalTime;
    private float _lastFrameTime;

    public Time GetCurrentTime(float currentTime)
    {
        var deltaTime = currentTime - _lastFrameTime;
        _lastFrameTime = currentTime;
        _totalTime += deltaTime;

        return new Time(
            deltaTime: deltaTime,
            fixedDeltaTime: 1.0f / 60.0f, // Default fixed timestep
            totalTime: _totalTime,
            fixedTotalTime: _fixedTotalTime
        );
    }

    public void UpdateFixedTime(float fixedDeltaTime)
    {
        _fixedTotalTime += fixedDeltaTime;
    }

    public void Reset()
    {
        _totalTime = 0f;
        _fixedTotalTime = 0f;
        _lastFrameTime = 0f;
    }
}

