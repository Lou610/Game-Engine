using System;
using System.Collections.Generic;
using Silk.NET.Vulkan;

namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Manages Vulkan command buffers for recording and submitting rendering commands
/// </summary>
public unsafe class VulkanCommandBuffer : IDisposable
{
    private readonly VulkanContext _context;
    private bool _disposed;
    
    // Command buffer objects
    private CommandPool _commandPool;
    private CommandBuffer[] _commandBuffers;
    private int _currentFrameIndex;
    
    // Synchronization objects
    private Silk.NET.Vulkan.Semaphore[] _imageAvailableSemaphores;
    private Silk.NET.Vulkan.Semaphore[] _renderFinishedSemaphores;
    private Fence[] _inFlightFences;
    
    // State tracking
    private bool _isRecording;
    private int _currentCommandBuffer;
    private const int MAX_FRAMES_IN_FLIGHT = 2;

    public CommandPool CommandPool => _commandPool;
    public CommandBuffer CurrentCommandBuffer => _commandBuffers[_currentCommandBuffer];
    public bool IsRecording => _isRecording;
    public int MaxFramesInFlight => MAX_FRAMES_IN_FLIGHT;
    public int CurrentFrameIndex => _currentFrameIndex;

    public VulkanCommandBuffer(VulkanContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        
        // Initialize arrays to empty to satisfy nullable warnings
        _commandBuffers = Array.Empty<CommandBuffer>();
        _imageAvailableSemaphores = Array.Empty<Silk.NET.Vulkan.Semaphore>();
        _renderFinishedSemaphores = Array.Empty<Silk.NET.Vulkan.Semaphore>();
        _inFlightFences = Array.Empty<Fence>();
        
        CreateCommandPool();
        AllocateCommandBuffers();
        CreateSynchronizationObjects();
    }

    /// <summary>
    /// Begin recording commands to the current command buffer
    /// </summary>
    public void BeginRecording(CommandBufferUsageFlags usage = CommandBufferUsageFlags.OneTimeSubmitBit)
    {
        if (_isRecording)
            throw new InvalidOperationException("Command buffer is already recording");

        _currentCommandBuffer = _currentFrameIndex;
        var commandBuffer = _commandBuffers[_currentCommandBuffer];

        // Wait for fence
        var fence = _inFlightFences[_currentFrameIndex];
        _context.VulkanApi.WaitForFences(_context.Device, 1, in fence, true, ulong.MaxValue);
        _context.VulkanApi.ResetFences(_context.Device, 1, in fence);

        // Reset and begin command buffer
        _context.VulkanApi.ResetCommandBuffer(commandBuffer, CommandBufferResetFlags.None);

        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = usage,
            PInheritanceInfo = null
        };

        var result = _context.VulkanApi.BeginCommandBuffer(commandBuffer, in beginInfo);
        if (result != Result.Success)
        {
            throw new Exception($"Failed to begin recording command buffer: {result}");
        }

        _isRecording = true;
    }

    /// <summary>
    /// End recording commands and finalize the command buffer
    /// </summary>
    public void EndRecording()
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer is not recording");

        var commandBuffer = _commandBuffers[_currentCommandBuffer];
        var result = _context.VulkanApi.EndCommandBuffer(commandBuffer);
        if (result != Result.Success)
        {
            throw new Exception($"Failed to end recording command buffer: {result}");
        }

        _isRecording = false;
    }

    /// <summary>
    /// Submit the current command buffer to the graphics queue
    /// </summary>
    public void Submit(uint imageIndex)
    {
        if (_isRecording)
            throw new InvalidOperationException("Cannot submit while still recording");

        var commandBuffer = _commandBuffers[_currentCommandBuffer];
        var waitSemaphores = new[] { _imageAvailableSemaphores[_currentFrameIndex] };
        var waitStages = new[] { PipelineStageFlags.ColorAttachmentOutputBit };
        var signalSemaphores = new[] { _renderFinishedSemaphores[_currentFrameIndex] };
        var commandBuffers = new[] { commandBuffer };

        var submitInfo = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = 1,
            SignalSemaphoreCount = 1,
            CommandBufferCount = 1
        };

        fixed (Silk.NET.Vulkan.Semaphore* waitSemaphoresPtr = waitSemaphores)
        fixed (PipelineStageFlags* waitStagesPtr = waitStages)
        fixed (Silk.NET.Vulkan.Semaphore* signalSemaphoresPtr = signalSemaphores)
        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            submitInfo.PWaitSemaphores = waitSemaphoresPtr;
            submitInfo.PWaitDstStageMask = waitStagesPtr;
            submitInfo.PSignalSemaphores = signalSemaphoresPtr;
            submitInfo.PCommandBuffers = commandBuffersPtr;
        }

        var fence = _inFlightFences[_currentFrameIndex];
        var result = _context.VulkanApi.QueueSubmit(_context.GraphicsQueue, 1, in submitInfo, fence);
        if (result != Result.Success)
        {
            throw new Exception($"Failed to submit command buffer: {result}");
        }
    }

    /// <summary>
    /// Begin render pass on the current command buffer
    /// </summary>
    public void BeginRenderPass(RenderPass renderPass, Framebuffer framebuffer, Extent2D extent, ClearValue clearColor)
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer must be recording");

        var clearValues = new[] { clearColor };

        var renderPassInfo = new RenderPassBeginInfo
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = renderPass,
            Framebuffer = framebuffer,
            RenderArea = new Rect2D
            {
                Offset = { X = 0, Y = 0 },
                Extent = extent
            },
            ClearValueCount = 1
        };

        fixed (ClearValue* clearValuesPtr = clearValues)
        {
            renderPassInfo.PClearValues = clearValuesPtr;
        }

        _context.VulkanApi.CmdBeginRenderPass(_commandBuffers[_currentCommandBuffer], in renderPassInfo, SubpassContents.Inline);
    }

    /// <summary>
    /// End render pass on the current command buffer
    /// </summary>
    public void EndRenderPass()
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer must be recording");

        _context.VulkanApi.CmdEndRenderPass(_commandBuffers[_currentCommandBuffer]);
    }

    /// <summary>
    /// Bind graphics pipeline
    /// </summary>
    public void BindPipeline(Pipeline pipeline, PipelineBindPoint bindPoint = PipelineBindPoint.Graphics)
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer must be recording");

        _context.VulkanApi.CmdBindPipeline(_commandBuffers[_currentCommandBuffer], bindPoint, pipeline);
    }

    /// <summary>
    /// Bind vertex buffer
    /// </summary>
    public void BindVertexBuffer(Silk.NET.Vulkan.Buffer vertexBuffer, int binding = 0, ulong offset = 0)
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer must be recording");

        var vertexBuffers = new[] { vertexBuffer };
        var offsets = new[] { offset };

        fixed (Silk.NET.Vulkan.Buffer* vertexBuffersPtr = vertexBuffers)
        fixed (ulong* offsetsPtr = offsets)
        {
            _context.VulkanApi.CmdBindVertexBuffers(_commandBuffers[_currentCommandBuffer], (uint)binding, 1, vertexBuffersPtr, offsetsPtr);
        }
    }

    /// <summary>
    /// Bind index buffer
    /// </summary>
    public void BindIndexBuffer(Silk.NET.Vulkan.Buffer indexBuffer, ulong offset = 0, IndexType indexType = IndexType.Uint32)
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer must be recording");

        _context.VulkanApi.CmdBindIndexBuffer(_commandBuffers[_currentCommandBuffer], indexBuffer, offset, indexType);
    }

    /// <summary>
    /// Draw vertices
    /// </summary>
    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer must be recording");

        _context.VulkanApi.CmdDraw(_commandBuffers[_currentCommandBuffer], vertexCount, instanceCount, firstVertex, firstInstance);
    }

    /// <summary>
    /// Draw indexed vertices
    /// </summary>
    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        if (!_isRecording)
            throw new InvalidOperationException("Command buffer must be recording");

        _context.VulkanApi.CmdDrawIndexed(_commandBuffers[_currentCommandBuffer], indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    /// <summary>
    /// Get current frame synchronization objects
    /// </summary>
    public (Silk.NET.Vulkan.Semaphore ImageAvailable, Silk.NET.Vulkan.Semaphore RenderFinished, Fence InFlight) GetCurrentFrameSync()
    {
        return (_imageAvailableSemaphores[_currentFrameIndex], 
                _renderFinishedSemaphores[_currentFrameIndex], 
                _inFlightFences[_currentFrameIndex]);
    }

    /// <summary>
    /// Advance to next frame
    /// </summary>
    public void NextFrame()
    {
        _currentFrameIndex = (_currentFrameIndex + 1) % MAX_FRAMES_IN_FLIGHT;
    }

    /// <summary>
    /// Create a single-use command buffer for immediate operations
    /// </summary>
    public CommandBuffer BeginSingleTimeCommands()
    {
        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = _commandPool,
            CommandBufferCount = 1
        };

        _context.VulkanApi.AllocateCommandBuffers(_context.Device, in allocInfo, out CommandBuffer commandBuffer);

        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        _context.VulkanApi.BeginCommandBuffer(commandBuffer, in beginInfo);
        return commandBuffer;
    }

    /// <summary>
    /// End and submit single-use command buffer
    /// </summary>
    public void EndSingleTimeCommands(CommandBuffer commandBuffer)
    {
        _context.VulkanApi.EndCommandBuffer(commandBuffer);

        var submitInfo = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        _context.VulkanApi.QueueSubmit(_context.GraphicsQueue, 1, in submitInfo, default);
        _context.VulkanApi.QueueWaitIdle(_context.GraphicsQueue);

        _context.VulkanApi.FreeCommandBuffers(_context.Device, _commandPool, 1, in commandBuffer);
    }

    private void CreateCommandPool()
    {
        var poolInfo = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = _context.GraphicsQueueFamily
        };

        var result = _context.VulkanApi.CreateCommandPool(_context.Device, in poolInfo, null, out _commandPool);
        if (result != Result.Success)
        {
            throw new Exception($"Failed to create command pool: {result}");
        }
    }

    private void AllocateCommandBuffers()
    {
        _commandBuffers = new CommandBuffer[MAX_FRAMES_IN_FLIGHT];

        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = MAX_FRAMES_IN_FLIGHT
        };

        fixed (CommandBuffer* commandBuffersPtr = _commandBuffers)
        {
            var result = _context.VulkanApi.AllocateCommandBuffers(_context.Device, in allocInfo, commandBuffersPtr);
            if (result != Result.Success)
            {
                throw new Exception($"Failed to allocate command buffers: {result}");
            }
        }
    }

    private void CreateSynchronizationObjects()
    {
        _imageAvailableSemaphores = new Silk.NET.Vulkan.Semaphore[MAX_FRAMES_IN_FLIGHT];
        _renderFinishedSemaphores = new Silk.NET.Vulkan.Semaphore[MAX_FRAMES_IN_FLIGHT];
        _inFlightFences = new Fence[MAX_FRAMES_IN_FLIGHT];

        var semaphoreInfo = new SemaphoreCreateInfo
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        var fenceInfo = new FenceCreateInfo
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
        {
            var result1 = _context.VulkanApi.CreateSemaphore(_context.Device, ref semaphoreInfo, null, out _imageAvailableSemaphores[i]);
            var result2 = _context.VulkanApi.CreateSemaphore(_context.Device, ref semaphoreInfo, null, out _renderFinishedSemaphores[i]);
            var result3 = _context.VulkanApi.CreateFence(_context.Device, ref fenceInfo, null, out _inFlightFences[i]);

            if (result1 != Result.Success || result2 != Result.Success || result3 != Result.Success)
            {
                throw new Exception($"Failed to create synchronization objects for frame {i}");
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Wait for device to be idle
            _context.VulkanApi.DeviceWaitIdle(_context.Device);

            // Cleanup synchronization objects
            if (_imageAvailableSemaphores != null)
            {
                for (int i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
                {
                    _context.VulkanApi.DestroySemaphore(_context.Device, _imageAvailableSemaphores[i], null);
                    _context.VulkanApi.DestroySemaphore(_context.Device, _renderFinishedSemaphores[i], null);
                    _context.VulkanApi.DestroyFence(_context.Device, _inFlightFences[i], null);
                }
            }

            // Cleanup command pool (automatically frees command buffers)
            if (_commandPool.Handle != 0)
            {
                _context.VulkanApi.DestroyCommandPool(_context.Device, _commandPool, null);
            }

            _disposed = true;
        }
    }
}