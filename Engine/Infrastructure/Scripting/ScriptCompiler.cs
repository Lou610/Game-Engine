using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Engine.Domain.Scripting;
using Engine.Domain.Scripting.ValueObjects;

namespace Engine.Infrastructure.Scripting;

/// <summary>
/// Roslyn-based compilation implementation
/// </summary>
public class ScriptCompiler
{
    public ScriptAssembly Compile(Script script)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(script.Source.Code);
        var compilation = CSharpCompilation.Create(
            script.Id.Value,
            new[] { syntaxTree },
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (result.Success)
        {
            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());
            return new ScriptAssembly
            {
                Id = script.Id.Value,
                Assembly = assembly,
                IsLoaded = true
            };
        }

        return new ScriptAssembly
        {
            Id = script.Id.Value,
            IsLoaded = false
        };
    }

    private static MetadataReference[] GetReferences()
    {
        return new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
        };
    }
}

