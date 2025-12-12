using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.Vulkan;
using Engine.Domain.Rendering;
using Engine.Domain.Rendering.ValueObjects;
using Engine.Infrastructure.Rendering.Vulkan;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Vector4 = System.Numerics.Vector4;

namespace Engine.Application.Rendering;

/// <summary>
/// High-level Vulkan renderer that coordinates all Vulkan components
/// </summary>
public class VulkanRenderer : IRenderer, IDisposable
{
    private readonly VulkanContext _context;
    private VulkanSwapchain _swapchain;
    private VulkanCommandBuffer _commandBuffer;
    private VulkanRenderPass _renderPass;
    private VulkanPipeline _pipeline;
    private VulkanShaderManager _shaderManager;
    
    private bool _initialized;
    private bool _disposed;
    
    private uint _currentWidth = 800;
    private uint _currentHeight = 600;

    public bool IsInitialized => _initialized;

    public VulkanRenderer(VulkanContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _shaderManager = new VulkanShaderManager(context.VulkanApi, context.Device);
        
        // Initialize to satisfy nullable warnings - will be properly set in Initialize()
        _swapchain = null!;
        _commandBuffer = null!;
        _renderPass = null!;
        _pipeline = null!;
    }

    /// <summary>
    /// Initialize the Vulkan renderer with the specified surface size
    /// </summary>
    public void Initialize(nint windowHandle, uint width, uint height)
    {
        if (_initialized)
            return;

        _currentWidth = width;
        _currentHeight = height;

        try
        {
            // Initialize Vulkan context if not already done
            if (!_context.IsInitialized)
            {
                _context.Initialize("Game Engine", new Version(1, 0, 0));
            }

            // Create surface if window handle provided
            if (windowHandle != nint.Zero)
            {
                _context.CreateSurface(windowHandle);
            }

            // Select physical device and create logical device
            if (_context.PhysicalDevice.Handle == 0)
            {
                _context.SelectPhysicalDevice();
                _context.CreateLogicalDevice();
            }

            // Create Vulkan components
            CreateRenderingComponents();
            
            _initialized = true;
        }
        catch
        {
            Cleanup();
            throw;
        }
    }

    /// <summary>
    /// Resize the renderer (typically called on window resize)
    /// </summary>
    public void Resize(uint width, uint height)
    {
        if (!_initialized || (width == _currentWidth && height == _currentHeight))
            return;

        _currentWidth = width;
        _currentHeight = height;

        // Recreate swapchain and related objects
        _swapchain?.RecreateSwapchain(width, height);
        RecreateFramebuffers();
    }

    /// <summary>
    /// Begin rendering a frame
    /// </summary>
    public void BeginFrame()
    {
        if (!_initialized)
            throw new InvalidOperationException("Renderer not initialized");

        // Begin command buffer recording
        _commandBuffer.BeginRecording();

        // Acquire next swapchain image
        var (imageAvailable, _, _) = _commandBuffer.GetCurrentFrameSync();
        var result = _swapchain.AcquireNextImage(imageAvailable);

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr)
        {
            // Swapchain needs recreation
            _swapchain.RecreateSwapchain(_currentWidth, _currentHeight);
            RecreateFramebuffers();
            return;
        }
        else if (result != Result.Success)
        {
            throw new Exception($"Failed to acquire swapchain image: {result}");
        }

        // Begin render pass
        var clearColor = new ClearValue
        {
            Color = new ClearColorValue(0.0f, 0.0f, 0.0f, 1.0f)
        };

        var framebuffer = _swapchain.Framebuffers[_swapchain.CurrentImageIndex];
        _commandBuffer.BeginRenderPass(_renderPass.Handle, framebuffer, _swapchain.Extent, clearColor);

        // Bind graphics pipeline
        _commandBuffer.BindPipeline(_pipeline.Handle);

        // Set dynamic viewport and scissor
        SetViewportAndScissor();
    }

    /// <summary>
    /// End rendering a frame and present to screen
    /// </summary>
    public void EndFrame()
    {
        if (!_initialized)
            throw new InvalidOperationException("Renderer not initialized");

        // End render pass
        _commandBuffer.EndRenderPass();

        // End command buffer recording
        _commandBuffer.EndRecording();

        // Submit command buffer
        _commandBuffer.Submit(_swapchain.CurrentImageIndex);

        // Present the image
        var (_, renderFinished, _) = _commandBuffer.GetCurrentFrameSync();
        var presentResult = _swapchain.PresentImage(renderFinished, _swapchain.CurrentImageIndex);

        if (presentResult == Result.ErrorOutOfDateKhr || presentResult == Result.SuboptimalKhr)
        {
            _swapchain.RecreateSwapchain(_currentWidth, _currentHeight);
            RecreateFramebuffers();
        }

        // Move to next frame
        _commandBuffer.NextFrame();
    }

    /// <summary>
    /// Draw a mesh with material and transform
    /// </summary>
    public void DrawMesh(Engine.Domain.Rendering.Entities.Mesh mesh, Engine.Domain.Rendering.Entities.Material material, Matrix4x4 transform)
    {
        if (!_initialized)
            throw new InvalidOperationException("Renderer not initialized");

        // For Phase 4.1, we'll render a simple triangle/quad
        // This is a placeholder implementation
        RenderSimpleGeometry();
    }

    /// <summary>
    /// Set the active camera for rendering
    /// </summary>
    public void SetCamera(Engine.Domain.ECS.Components.Camera camera, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        // For Phase 4.1, camera matrices will be handled via push constants or UBOs
        // This is a placeholder for the interface
    }

    private void CreateRenderingComponents()
    {
        // Create swapchain
        _swapchain = new VulkanSwapchain(_context);
        _swapchain.CreateSwapchain(_currentWidth, _currentHeight);

        // Create render pass
        _renderPass = new VulkanRenderPass(_context);
        _renderPass.CreateColorRenderPass(_swapchain.ImageFormat);

        // Create framebuffers for swapchain images
        _swapchain.CreateFramebuffers(_renderPass.Handle);

        // Create command buffer system
        _commandBuffer = new VulkanCommandBuffer(_context);

        // Create graphics pipeline
        _pipeline = new VulkanPipeline(_context, _shaderManager);
        _pipeline.CreateBasicGraphicsPipeline(_renderPass.Handle, _swapchain.Extent);
    }

    private void RecreateFramebuffers()
    {
        if (_swapchain?.IsInitialized == true && _renderPass?.IsCreated == true)
        {
            _swapchain.CreateFramebuffers(_renderPass.Handle);
        }
    }

    private void SetViewportAndScissor()
    {
        // Set dynamic viewport
        var viewport = new Viewport
        {
            X = 0.0f,
            Y = 0.0f,
            Width = _swapchain.Extent.Width,
            Height = _swapchain.Extent.Height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f
        };

        var scissor = new Rect2D
        {
            Offset = { X = 0, Y = 0 },
            Extent = _swapchain.Extent
        };

        unsafe
        {
            _context.VulkanApi.CmdSetViewport(_commandBuffer.CurrentCommandBuffer, 0, 1, &viewport);
            _context.VulkanApi.CmdSetScissor(_commandBuffer.CurrentCommandBuffer, 0, 1, &scissor);
        }
    }

    private void RenderSimpleGeometry()
    {
        // For Phase 4.1, render a simple colored triangle
        // This demonstrates the pipeline working without vertex buffers
        
        // Draw 3 vertices (triangle) - vertices are defined in the vertex shader
        _commandBuffer.Draw(3, 1, 0, 0);
    }

    private void Cleanup()
    {
        if (_pipeline != null)
        {
            _pipeline.Dispose();
            _pipeline = null!;
        }

        if (_commandBuffer != null)
        {
            _commandBuffer.Dispose();
            _commandBuffer = null!;
        }

        if (_renderPass != null)
        {
            _renderPass.Dispose();
            _renderPass = null!;
        }

        if (_swapchain != null)
        {
            _swapchain.Dispose();
            _swapchain = null!;
        }

        if (_shaderManager != null)
        {
            _shaderManager.Dispose();
            _shaderManager = null!;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Cleanup();
            _disposed = true;
        }
    }
}