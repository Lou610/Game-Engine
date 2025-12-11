using Engine.Application;
using Engine.Domain.Core;

namespace GameEngine;

class Program
{
    static void Main(string[] args)
    {
        var config = new ApplicationConfig(
            windowTitle: "Game Engine",
            windowWidth: 1280,
            windowHeight: 720
        );

        using var app = new Application(config);
        app.Run();
    }
}

