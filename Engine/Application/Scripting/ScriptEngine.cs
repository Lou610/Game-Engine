using Engine.Domain.Scripting;
using Engine.Domain.Scripting.Services;
using Engine.Domain.Scripting.ValueObjects;
using Engine.Infrastructure.Logging;

namespace Engine.Application.Scripting;

/// <summary>
/// Application service for script orchestration
/// </summary>
public class ScriptEngine
{
    private readonly ScriptCompilation _compilation;
    private readonly ScriptExecution _execution;
    private readonly Logger _logger;

    public ScriptEngine(ScriptCompilation compilation, ScriptExecution execution, Logger logger)
    {
        _compilation = compilation;
        _execution = execution;
        _logger = logger;
    }

    public ScriptAssembly CompileScript(Script script)
    {
        var assembly = _compilation.Compile(script);
        _logger.Debug($"Script compiled: {script.Id.Value}");
        return assembly;
    }

    public void ExecuteScript(ScriptAssembly assembly)
    {
        _execution.Execute(assembly);
        _logger.Debug($"Script executed: {assembly.Id}");
    }
}

