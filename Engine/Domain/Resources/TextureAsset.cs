using System;
using Engine.Domain.Resources.ValueObjects;

namespace Engine.Domain.Resources;

/// <summary>
/// Asset representing a texture with image data and sampling properties
/// </summary>
public class TextureAsset : Asset
{
    /// <summary>
    /// Texture width in pixels
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Texture height in pixels
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Number of channels (1=R, 2=RG, 3=RGB, 4=RGBA)
    /// </summary>
    public int Channels { get; set; }

    /// <summary>
    /// Raw pixel data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Texture format
    /// </summary>
    public TextureFormat Format { get; set; } = TextureFormat.RGBA8;

    /// <summary>
    /// Texture filtering mode
    /// </summary>
    public TextureFilter Filter { get; set; } = TextureFilter.Linear;

    /// <summary>
    /// Texture wrapping mode
    /// </summary>
    public TextureWrap WrapMode { get; set; } = TextureWrap.Repeat;

    /// <summary>
    /// Whether to generate mipmaps
    /// </summary>
    public bool GenerateMipmaps { get; set; } = true;

    /// <summary>
    /// Whether this is an sRGB texture (for color textures)
    /// </summary>
    public bool IsSRGB { get; set; } = true;

    public TextureAsset()
    {
        Type = AssetType.Texture;
    }

    public TextureAsset(AssetGuid id, string name, AssetPath path, int width, int height, int channels) : this()
    {
        Id = id;
        Name = name;
        Path = path;
        Width = width;
        Height = height;
        Channels = channels;
    }

    /// <summary>
    /// Calculate the expected data size in bytes
    /// </summary>
    public int CalculateDataSize()
    {
        var bytesPerPixel = Format switch
        {
            TextureFormat.R8 => 1,
            TextureFormat.RG8 => 2,
            TextureFormat.RGB8 => 3,
            TextureFormat.RGBA8 => 4,
            TextureFormat.R16F => 2,
            TextureFormat.RG16F => 4,
            TextureFormat.RGB16F => 6,
            TextureFormat.RGBA16F => 8,
            TextureFormat.R32F => 4,
            TextureFormat.RG32F => 8,
            TextureFormat.RGB32F => 12,
            TextureFormat.RGBA32F => 16,
            _ => 4
        };

        return Width * Height * bytesPerPixel;
    }

    /// <summary>
    /// Validate that the texture data is consistent
    /// </summary>
    public bool IsValid()
    {
        return Width > 0 && 
               Height > 0 && 
               Channels > 0 && 
               Channels <= 4 &&
               Data.Length >= CalculateDataSize();
    }

    /// <summary>
    /// Get pixel data at specific coordinates (for RGBA8 format)
    /// </summary>
    public (byte r, byte g, byte b, byte a) GetPixel(int x, int y)
    {
        if (Format != TextureFormat.RGBA8)
            throw new InvalidOperationException("GetPixel only supports RGBA8 format");

        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException("Pixel coordinates out of bounds");

        var index = (y * Width + x) * 4;
        return (Data[index], Data[index + 1], Data[index + 2], Data[index + 3]);
    }

    /// <summary>
    /// Set pixel data at specific coordinates (for RGBA8 format)
    /// </summary>
    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
    {
        if (Format != TextureFormat.RGBA8)
            throw new InvalidOperationException("SetPixel only supports RGBA8 format");

        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException("Pixel coordinates out of bounds");

        var index = (y * Width + x) * 4;
        Data[index] = r;
        Data[index + 1] = g;
        Data[index + 2] = b;
        Data[index + 3] = a;
    }
}

/// <summary>
/// Texture data formats
/// </summary>
public enum TextureFormat
{
    R8,       // 8-bit red
    RG8,      // 8-bit red, green
    RGB8,     // 8-bit RGB
    RGBA8,    // 8-bit RGBA
    R16F,     // 16-bit float red
    RG16F,    // 16-bit float red, green
    RGB16F,   // 16-bit float RGB
    RGBA16F,  // 16-bit float RGBA
    R32F,     // 32-bit float red
    RG32F,    // 32-bit float red, green
    RGB32F,   // 32-bit float RGB
    RGBA32F   // 32-bit float RGBA
}

/// <summary>
/// Texture filtering modes
/// </summary>
public enum TextureFilter
{
    Point,     // Nearest neighbor
    Linear,    // Bilinear
    Trilinear  // Trilinear with mipmaps
}

/// <summary>
/// Texture wrapping modes
/// </summary>
public enum TextureWrap
{
    Repeat,         // Tile the texture
    Clamp,          // Clamp to edge
    Mirror,         // Mirror the texture
    ClampToBorder   // Clamp to border color
}