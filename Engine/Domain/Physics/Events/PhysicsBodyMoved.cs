namespace Engine.Domain.Physics.Events;

/// <summary>
/// Domain event for physics body moved
/// </summary>
public record PhysicsBodyMoved(string RigidbodyId, float X, float Y, float Z);

