using System.Collections.Generic;

namespace Engine.Infrastructure.Scripting.Lag;

/// <summary>
/// AST representation
/// </summary>
public class LagSyntaxTree
{
    public LagNode Root { get; set; } = new();
}

public class LagNode
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public List<LagNode> Children { get; set; } = new();
}

