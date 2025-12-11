using System.Collections.Generic;
using Engine.Domain.Input;
using Engine.Domain.Input.Services;
using Engine.Domain.Input.ValueObjects;

namespace Engine.Application.Input;

/// <summary>
/// Application service for input orchestration
/// </summary>
public class InputManager
{
    private readonly InputMapping _mapping;
    private readonly Dictionary<KeyCode, bool> _keyStates = new();

    public InputManager(InputMapping mapping)
    {
        _mapping = mapping;
    }

    public void UpdateKeyState(KeyCode key, bool isPressed)
    {
        _keyStates[key] = isPressed;
    }

    public bool IsKeyPressed(KeyCode key)
    {
        return _keyStates.TryGetValue(key, out var pressed) && pressed;
    }
}

