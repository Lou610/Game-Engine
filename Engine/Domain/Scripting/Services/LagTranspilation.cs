using Engine.Domain.Scripting;
using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Domain.Scripting.Services;

/// <summary>
/// Domain service for lag to C# transpilation
/// </summary>
public class LagTranspilation
{
    public Script TranspileToCSharp(LagScript lagScript)
    {
        // Transpilation logic would be handled by infrastructure
        return new Script
        {
            Id = lagScript.Id,
            Source = new ScriptSource(string.Empty, ScriptLanguage.CSharp),
            FilePath = lagScript.FilePath
        };
    }
}

