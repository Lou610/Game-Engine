using System;
using System.Collections.Generic;
using System.IO;
using Engine.Domain.Scripting;
using Engine.Domain.Scripting.ValueObjects;
using Engine.Infrastructure.Logging;

namespace Engine.Application.Scripting;

/// <summary>
/// Application service for hot-reload
/// </summary>
public class HotReloadService
{
    private readonly ScriptEngine _scriptEngine;
    private readonly Logger _logger;
    private readonly Dictionary<string, DateTime> _fileTimestamps = new();

    public HotReloadService(ScriptEngine scriptEngine, Logger logger)
    {
        _scriptEngine = scriptEngine;
        _logger = logger;
    }

    public void WatchScript(Script script)
    {
        if (File.Exists(script.FilePath))
        {
            _fileTimestamps[script.FilePath] = File.GetLastWriteTime(script.FilePath);
        }
    }

    public bool CheckForChanges(Script script)
    {
        if (!File.Exists(script.FilePath))
        {
            return false;
        }

        var currentTime = File.GetLastWriteTime(script.FilePath);
        if (_fileTimestamps.TryGetValue(script.FilePath, out var lastTime))
        {
            if (currentTime > lastTime)
            {
                _fileTimestamps[script.FilePath] = currentTime;
                _logger.Info($"Script changed: {script.FilePath}");
                return true;
            }
        }

        return false;
    }
}

