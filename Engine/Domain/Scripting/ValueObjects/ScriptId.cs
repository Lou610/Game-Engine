namespace Engine.Domain.Scripting.ValueObjects;

/// <summary>
/// Script identifier
/// </summary>
public readonly record struct ScriptId
{
    public string Value { get; init; }

    public ScriptId(string value)
    {
        Value = value;
    }

    public static ScriptId Invalid => new(string.Empty);
}

