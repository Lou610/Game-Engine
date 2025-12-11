namespace Engine.Domain.Input.Events;

/// <summary>
/// Domain event for input axis changed
/// </summary>
public record InputAxisChanged(string AxisName, float Value);

