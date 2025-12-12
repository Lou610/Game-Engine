using System;
using System.Threading.Tasks;
using Engine.Application.Scene;
using Engine.Domain.Resources;
using Engine.Domain.Resources.ValueObjects;
using Engine.Domain.Scene;
using Engine.Domain.Scene.ValueObjects;
using Engine.Infrastructure.Resources;
using Engine.Infrastructure.Logging;

namespace Engine.Demo;

/// <summary>
/// Demonstration of Phase 4.5 Asset Integration features
/// </summary>
public static class AssetIntegrationDemo
{
    public static async Task DemonstrateAssetIntegration()
    {
        Console.WriteLine("\n=== Phase 4.5: Asset Integration Demo ===");
        
        // Setup services
        var logger = new Logger("AssetDemo");
        var assetLoader = new AssetLoader();
        var assetCache = new AssetCache();
        var sceneAssetService = new SceneAssetService(assetLoader, assetCache);
        var sceneManager = new SceneManager(logger, null!, sceneAssetService);
        
        // Create a scene
        var scene = sceneManager.CreateScene("TestScene");
        Console.WriteLine($"Created scene: {scene.Name}");
        
        // Create sample assets
        var meshAsset = CreateSampleMeshAsset();
        var materialAsset = CreateSampleMaterialAsset();
        var textureAsset = CreateSampleTextureAsset();
        
        // Register assets with the scene
        scene.Assets.RegisterAsset(meshAsset, isRequired: true);
        scene.Assets.RegisterAsset(materialAsset, isRequired: true);
        scene.Assets.RegisterAsset(textureAsset, isRequired: false);
        
        Console.WriteLine($"Registered {scene.Assets.TotalAssetCount} assets with scene");
        Console.WriteLine($"Required assets: {scene.Assets.RequiredAssets.Count}");
        
        // Demonstrate asset queries
        var meshAssets = scene.Assets.GetAssetsByType<MeshAsset>();
        var materialAssets = scene.Assets.GetAssetsByType<MaterialAsset>();
        var textureAssets = scene.Assets.GetAssetsByType<TextureAsset>();
        
        Console.WriteLine($"Found: {meshAssets.Count} meshes, {materialAssets.Count} materials, {textureAssets.Count} textures");
        
        // Load the scene (which will trigger asset loading)
        sceneManager.LoadScene(scene);
        Console.WriteLine("Scene loaded with assets");
        
        // Demonstrate asset service functionality
        Console.WriteLine($"Assets in service for scene: {sceneAssetService.GetSceneAssets(scene.Id.Value).Count}");
        
        foreach (var assetId in sceneAssetService.GetSceneAssets(scene.Id.Value))
        {
            var refCount = sceneAssetService.GetReferenceCount(assetId);
            Console.WriteLine($"Asset {assetId.Value}: {refCount} references");
        }
        
        // Unload scene (which will unload assets)
        sceneManager.UnloadScene(scene);
        Console.WriteLine("Scene and assets unloaded");
        
        Console.WriteLine("Asset Integration demo completed successfully!");
    }
    
    private static MeshAsset CreateSampleMeshAsset()
    {
        var meshAsset = new MeshAsset(
            AssetGuid.NewGuid(),
            "TestCube",
            new AssetPath("assets/meshes/cube.mesh")
        );
        
        // Simple cube vertices (position + normal + uv)
        meshAsset.Vertices = new float[]
        {
            // Front face
            -1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f, 0.0f,
             1.0f, -1.0f,  1.0f,  0.0f,  0.0f,  1.0f,  1.0f, 0.0f,
             1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f,  1.0f, 1.0f,
            -1.0f,  1.0f,  1.0f,  0.0f,  0.0f,  1.0f,  0.0f, 1.0f,
        };
        
        meshAsset.Indices = new uint[] { 0, 1, 2, 2, 3, 0 };
        meshAsset.CalculateBounds();
        
        return meshAsset;
    }
    
    private static MaterialAsset CreateSampleMaterialAsset()
    {
        var materialAsset = new MaterialAsset(
            AssetGuid.NewGuid(),
            "TestMaterial",
            new AssetPath("assets/materials/test.mat"),
            "StandardShader"
        );
        
        // Set some material properties
        materialAsset.SetColor("albedo", 1.0f, 0.5f, 0.0f, 1.0f); // Orange
        materialAsset.SetFloat("metallic", 0.2f);
        materialAsset.SetFloat("roughness", 0.8f);
        
        return materialAsset;
    }
    
    private static TextureAsset CreateSampleTextureAsset()
    {
        var textureAsset = new TextureAsset(
            AssetGuid.NewGuid(),
            "TestTexture",
            new AssetPath("assets/textures/test.png"),
            width: 256,
            height: 256,
            channels: 4
        );
        
        // Create simple checkerboard pattern
        textureAsset.Data = new byte[256 * 256 * 4];
        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                var checker = ((x / 32) + (y / 32)) % 2;
                var color = checker == 0 ? (byte)255 : (byte)0;
                
                var index = (y * 256 + x) * 4;
                textureAsset.Data[index] = color;     // R
                textureAsset.Data[index + 1] = color; // G
                textureAsset.Data[index + 2] = color; // B
                textureAsset.Data[index + 3] = 255;   // A
            }
        }
        
        return textureAsset;
    }
}