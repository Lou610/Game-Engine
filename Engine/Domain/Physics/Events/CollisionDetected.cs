namespace Engine.Domain.Physics.Events;

/// <summary>
/// Domain event for collision detected
/// </summary>
public record CollisionDetected(string ColliderAId, string ColliderBId);

