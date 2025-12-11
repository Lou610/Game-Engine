using Engine.Domain.Core;
using Engine.Domain.Core.Services;
using Engine.Infrastructure.Logging;
using Engine.Infrastructure.Memory;
using Engine.Infrastructure.Platform;

namespace Engine.Application;

/// <summary>
/// Application service orchestrating engine lifecycle
/// </summary>
public class Application
{
    private readonly ConfigurationService _configService;
    private readonly TimeService _timeService;
    private readonly Logger _logger;
    private readonly MemoryManager _memoryManager;
    private readonly PlatformAbstraction _platform;
    private readonly GameLoop _gameLoop;
    private bool _isRunning;

    public Application(ApplicationConfig config)
    {
        _configService = new ConfigurationService(config);
        _timeService = new TimeService();
        _logger = new Logger();
        _memoryManager = new MemoryManager();
        _platform = PlatformAbstraction.Create();
        _gameLoop = new GameLoop(_timeService, _logger);

        _logger.Info($"Initialized on platform: {_platform.GetPlatformName()}");
    }

    public void Run()
    {
        _isRunning = true;
        _logger.Info("Application started");

        _gameLoop.Run(() => _isRunning);

        _logger.Info("Application stopped");
    }

    public void Stop()
    {
        _isRunning = false;
    }

    public void Dispose()
    {
        _memoryManager.ClearPools();
        _logger.Info("Application disposed");
    }
}

