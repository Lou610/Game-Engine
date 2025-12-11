using System;
using Engine.Domain.Scripting;
using Engine.Infrastructure.Scripting.Lag;

namespace Engine.Infrastructure.Scripting.Lag;

/// <summary>
/// Transpiles lag AST to C#
/// </summary>
public class LagTranspiler
{
    private readonly LagParser _parser;
    private readonly LagTypeChecker _typeChecker;

    public LagTranspiler(LagParser parser, LagTypeChecker typeChecker)
    {
        _parser = parser;
        _typeChecker = typeChecker;
    }

    public string Transpile(LagScript lagScript)
    {
        // Parse lag script
        var syntaxTree = _parser.Parse(lagScript.SourceCode);

        // Type check
        if (!_typeChecker.CheckTypes(syntaxTree))
        {
            throw new InvalidOperationException("Type checking failed");
        }

        // Transpile to C#
        return TranspileNode(syntaxTree.Root);
    }

    private string TranspileNode(LagNode node)
    {
        // Transpilation logic - convert lag AST to C# code
        // This is a simplified placeholder
        var csharpCode = new System.Text.StringBuilder();

        switch (node.Type)
        {
            case "Program":
                foreach (var child in node.Children)
                {
                    csharpCode.AppendLine(TranspileNode(child));
                }
                break;
            // Add more node type handling here
        }

        return csharpCode.ToString();
    }
}

