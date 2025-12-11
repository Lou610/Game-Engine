namespace Engine.Domain.Audio.Events;

/// <summary>
/// Domain event for audio played
/// </summary>
public record AudioPlayed(string AudioSourceId, string AudioClipId);

