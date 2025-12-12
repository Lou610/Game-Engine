namespace Engine.Domain.Core.Services;

/// <summary>
/// Base interface for all services in the engine
/// </summary>
public interface IService
{
    /// <summary>
    /// Initialize the service
    /// </summary>
    void Initialize();

    /// <summary>
    /// Shutdown the service
    /// </summary>
    void Shutdown();
}