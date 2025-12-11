using System;
using System.IO;

namespace Tests.Shared.TestHelpers;

/// <summary>
/// Helper methods for common test operations
/// </summary>
public static class TestHelpers
{
    public static string CreateTempFilePath(string extension = ".tmp")
    {
        return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + extension);
    }
}

