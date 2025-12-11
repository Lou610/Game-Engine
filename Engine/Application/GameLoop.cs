using System;
using System.Diagnostics;
using Engine.Domain.Core;
using Engine.Infrastructure.Logging;

namespace Engine.Application;

/// <summary>
/// Application service managing update loop with fixed timestep
/// </summary>
public class GameLoop
{
    private readonly TimeService _timeService;
    private readonly Logger _logger;
    private readonly Stopwatch _stopwatch;
    private float _accumulator;

    public GameLoop(TimeService timeService, Logger logger)
    {
        _timeService = timeService;
        _logger = logger;
        _stopwatch = new Stopwatch();
    }

    public void Run(Func<bool> shouldContinue)
    {
        _stopwatch.Start();
        var lastTime = 0.0;
        const double fixedTimeStep = 1.0 / 60.0; // 60 FPS fixed timestep

        while (shouldContinue())
        {
            var currentTime = _stopwatch.Elapsed.TotalSeconds;
            var deltaTime = (float)(currentTime - lastTime);
            lastTime = currentTime;

            // Cap delta time to prevent spiral of death
            deltaTime = Math.Min(deltaTime, 0.25f);

            _accumulator += deltaTime;

            // Fixed timestep updates
            while (_accumulator >= fixedTimeStep)
            {
                FixedUpdate((float)fixedTimeStep);
                _accumulator -= fixedTimeStep;
            }

            // Variable timestep update
            Update(deltaTime);

            // Render (variable timestep)
            Render(deltaTime);
        }

        _stopwatch.Stop();
    }

    private void Update(float deltaTime)
    {
        var time = _timeService.GetCurrentTime((float)_stopwatch.Elapsed.TotalSeconds);
        // Update game logic here
    }

    private void FixedUpdate(float fixedDeltaTime)
    {
        _timeService.UpdateFixedTime(fixedDeltaTime);
        // Fixed update game logic here
    }

    private void Render(float deltaTime)
    {
        // Render logic here
    }
}

