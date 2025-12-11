namespace Engine.Domain.Input.Events;

/// <summary>
/// Domain event for input action triggered
/// </summary>
public record InputActionTriggered(string ActionId, string ActionName);

