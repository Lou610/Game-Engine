namespace Engine.Domain.Core;

/// <summary>
/// Value object for engine configuration
/// </summary>
public readonly record struct ApplicationConfig
{
    public string WindowTitle { get; init; }
    public int WindowWidth { get; init; }
    public int WindowHeight { get; init; }
    public bool Fullscreen { get; init; }
    public bool VSync { get; init; }
    public float FixedTimeStep { get; init; }
    public int MaxFPS { get; init; }

    public ApplicationConfig(
        string windowTitle = "Game Engine",
        int windowWidth = 1280,
        int windowHeight = 720,
        bool fullscreen = false,
        bool vSync = true,
        float fixedTimeStep = 1.0f / 60.0f,
        int maxFPS = 0)
    {
        WindowTitle = windowTitle;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
        Fullscreen = fullscreen;
        VSync = vSync;
        FixedTimeStep = fixedTimeStep;
        MaxFPS = maxFPS;
    }
}

