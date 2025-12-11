using System.IO;
using Newtonsoft.Json;
using Engine.Domain.Scene;
using SceneEntity = Engine.Domain.Scene.Scene;

namespace Engine.Infrastructure.Scene;

/// <summary>
/// JSON/YAML serialization implementation
/// </summary>
public class SceneSerializer
{
    public void Serialize(SceneEntity scene, string filePath)
    {
        var json = JsonConvert.SerializeObject(scene, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public SceneEntity? Deserialize(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<SceneEntity>(json);
    }
}

