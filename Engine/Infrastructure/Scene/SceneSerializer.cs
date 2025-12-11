using System.IO;
using Newtonsoft.Json;
using Engine.Domain.Scene;

namespace Engine.Infrastructure.Scene;

/// <summary>
/// JSON/YAML serialization implementation
/// </summary>
public class SceneSerializer
{
    public void Serialize(Scene scene, string filePath)
    {
        var json = JsonConvert.SerializeObject(scene, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public Scene? Deserialize(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<Scene>(json);
    }
}

