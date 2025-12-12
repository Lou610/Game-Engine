using System;
using System.Linq;
using NUnit.Framework;
using Engine.Domain.ECS;
using Engine.Domain.ECS.ValueObjects;
using Engine.Domain.ECS.Components;
using Engine.Application.ECS;

namespace GameEngine.Application.Tests.ECS;

[TestFixture]
public class SystemManagerTests
{
    private World _world = null!;
    private SystemManager _systemManager = null!;
    private DomainEventPublisher _eventPublisher = null!;

    [SetUp]
    public void SetUp()
    {
        _eventPublisher = new DomainEventPublisher();
        _world = new World(_eventPublisher);
        _systemManager = new SystemManager(_world);
    }

    [TearDown]
    public void TearDown()
    {
        _systemManager?.Dispose();
        _world?.Dispose();
    }

    [Test]
    public void RegisterSystem_WithValidSystem_AddsSystemToCollection()
    {
        // Arrange
        var system = new TestSystem();

        // Act
        _systemManager.RegisterSystem(system);

        // Assert
        Assert.That(_systemManager.SystemCount, Is.EqualTo(1));
        Assert.That(_systemManager.GetSystem<TestSystem>(), Is.EqualTo(system));
    }

    [Test]
    public void RegisterSystem_WithDuplicateSystem_ThrowsException()
    {
        // Arrange
        var system = new TestSystem();
        _systemManager.RegisterSystem(system);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _systemManager.RegisterSystem(system));
    }

    [Test]
    public void RegisterSystem_ByGenericType_CreatesAndRegistersSystem()
    {
        // Act
        var system = _systemManager.RegisterSystem<TestSystem>();

        // Assert
        Assert.That(system, Is.Not.Null);
        Assert.That(_systemManager.SystemCount, Is.EqualTo(1));
        Assert.That(_systemManager.GetSystem<TestSystem>(), Is.EqualTo(system));
    }

    [Test]
    public void InitializeSystems_CallsInitializeOnAllSystems()
    {
        // Arrange
        var system1 = new TestSystem();
        var system2 = new TestSystem();
        
        _systemManager.RegisterSystem(system1);
        _systemManager.RegisterSystem(system2);

        // Act
        _systemManager.InitializeSystems();

        // Assert
        Assert.That(system1.InitializeCalled, Is.True);
        Assert.That(system2.InitializeCalled, Is.True);
        Assert.That(system1.World, Is.EqualTo(_world));
        Assert.That(system2.World, Is.EqualTo(_world));
    }

    [Test]
    public void UpdateSystems_CallsUpdateOnEnabledSystems()
    {
        // Arrange
        var system1 = new TestSystem();
        var system2 = new TestSystem { IsEnabled = false };
        
        _systemManager.RegisterSystem(system1);
        _systemManager.RegisterSystem(system2);
        _systemManager.InitializeSystems();

        // Act
        _systemManager.UpdateSystems(0.016f);

        // Assert
        Assert.That(system1.UpdateCalled, Is.True);
        Assert.That(system1.LastDeltaTime, Is.EqualTo(0.016f));
        Assert.That(system2.UpdateCalled, Is.False);
    }

    [Test]
    public void FixedUpdateSystems_CallsFixedUpdateOnEnabledSystems()
    {
        // Arrange
        var system1 = new TestSystem();
        var system2 = new TestSystem { IsEnabled = false };
        
        _systemManager.RegisterSystem(system1);
        _systemManager.RegisterSystem(system2);
        _systemManager.InitializeSystems();

        // Act
        _systemManager.FixedUpdateSystems(0.02f);

        // Assert
        Assert.That(system1.FixedUpdateCalled, Is.True);
        Assert.That(system1.LastFixedDeltaTime, Is.EqualTo(0.02f));
        Assert.That(system2.FixedUpdateCalled, Is.False);
    }

    [Test]
    public void SystemPriority_ExecutesSystemsInCorrectOrder()
    {
        // Arrange
        var highPrioritySystem = new TestSystem { TestPriority = 1 };
        var lowPrioritySystem = new TestSystem { TestPriority = 10 };
        
        // Register in reverse order to test sorting
        _systemManager.RegisterSystem(lowPrioritySystem);
        _systemManager.RegisterSystem(highPrioritySystem);
        
        _systemManager.InitializeSystems();

        // Act
        _systemManager.UpdateSystems(0.016f);

        // Assert
        var systems = _systemManager.GetAllSystems();
        Assert.That(systems[0], Is.EqualTo(highPrioritySystem));
        Assert.That(systems[1], Is.EqualTo(lowPrioritySystem));
    }

    [Test]
    public void UnregisterSystem_RemovesSystemAndCallsShutdown()
    {
        // Arrange
        var system = new TestSystem();
        _systemManager.RegisterSystem(system);
        _systemManager.InitializeSystems();

        // Act
        _systemManager.UnregisterSystem(system);

        // Assert
        Assert.That(_systemManager.SystemCount, Is.EqualTo(0));
        Assert.That(system.ShutdownCalled, Is.True);
    }

    [Test]
    public void SetSystemEnabled_ChangesSystemEnabledState()
    {
        // Arrange
        var system = _systemManager.RegisterSystem<TestSystem>();
        _systemManager.InitializeSystems();

        // Act
        _systemManager.SetSystemEnabled<TestSystem>(false);
        _systemManager.UpdateSystems(0.016f);

        // Assert
        Assert.That(system.IsEnabled, Is.False);
        Assert.That(system.UpdateCalled, Is.False);
    }

    [Test]
    public void ShutdownSystems_CallsShutdownOnAllSystems()
    {
        // Arrange
        var system1 = new TestSystem();
        var system2 = new TestSystem();
        
        _systemManager.RegisterSystem(system1);
        _systemManager.RegisterSystem(system2);
        _systemManager.InitializeSystems();

        // Act
        _systemManager.ShutdownSystems();

        // Assert
        Assert.That(system1.ShutdownCalled, Is.True);
        Assert.That(system2.ShutdownCalled, Is.True);
    }

    // Test system implementation
    private class TestSystem : Engine.Application.ECS.System
    {
        public bool InitializeCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool FixedUpdateCalled { get; private set; }
        public bool ShutdownCalled { get; private set; }
        
        public float LastDeltaTime { get; private set; }
        public float LastFixedDeltaTime { get; private set; }
        
        public int TestPriority { get; set; } = 0;
        public override int Priority => TestPriority;

        public override void Initialize(World world)
        {
            base.Initialize(world);
            InitializeCalled = true;
        }

        public override void Update(float deltaTime)
        {
            UpdateCalled = true;
            LastDeltaTime = deltaTime;
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            FixedUpdateCalled = true;
            LastFixedDeltaTime = fixedDeltaTime;
        }

        public override void Shutdown()
        {
            base.Shutdown();
            ShutdownCalled = true;
        }
    }
}