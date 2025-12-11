namespace Engine.Application.ECS;

/// <summary>
/// Application service processing components
/// </summary>
public abstract class System
{
    public virtual void Initialize() { }
    public virtual void Update(float deltaTime) { }
    public virtual void FixedUpdate(float fixedDeltaTime) { }
    public virtual void Shutdown() { }
}

