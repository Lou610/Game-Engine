using Engine.Application;
using Engine.Domain.Core;

namespace GameEngine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🚀 Game Engine with ECS Phase 2 Starting...");
        
        try
        {
            // Run ECS Demo to show Phase 2 functionality
            EngineDemo.RunDemo();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}

