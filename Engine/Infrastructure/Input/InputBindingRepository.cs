using System.Collections.Generic;
using Engine.Domain.Input;

namespace Engine.Infrastructure.Input;

/// <summary>
/// Input binding persistence
/// </summary>
public class InputBindingRepository
{
    private readonly Dictionary<string, InputBinding> _bindings = new();

    public void Save(InputBinding binding)
    {
        _bindings[binding.Id] = binding;
    }

    public InputBinding? Load(string id)
    {
        return _bindings.TryGetValue(id, out var binding) ? binding : null;
    }

    public IEnumerable<InputBinding> GetAll()
    {
        return _bindings.Values;
    }
}

