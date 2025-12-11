namespace Engine.Domain.Scripting.ValueObjects;

/// <summary>
/// Script source code
/// </summary>
public readonly record struct ScriptSource
{
    public string Code { get; init; }
    public ScriptLanguage Language { get; init; }

    public ScriptSource(string code, ScriptLanguage language)
    {
        Code = code;
        Language = language;
    }
}

