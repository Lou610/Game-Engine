using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Manages Vulkan swapchain for presenting rendered images to the surface
/// </summary>
public unsafe class VulkanSwapchain : IDisposable
{
    private readonly VulkanContext _context;
    // Note: Swapchain operations are part of the core API in newer Silk.NET versions
    private bool _disposed;
    
    // Swapchain objects
    private SwapchainKHR _swapchain;
    private Image[] _swapchainImages;
    private ImageView[] _swapchainImageViews;
    private Framebuffer[] _swapchainFramebuffers;
    
    // Swapchain properties
    private Format _swapchainImageFormat;
    private Extent2D _swapchainExtent;
    private uint _imageCount;
    
    // Current state
    private uint _currentImageIndex;
    private bool _isInitialized;

    public SwapchainKHR Handle => _swapchain;
    public Format ImageFormat => _swapchainImageFormat;
    public Extent2D Extent => _swapchainExtent;
    public uint ImageCount => _imageCount;
    public Image[] Images => _swapchainImages;
    public ImageView[] ImageViews => _swapchainImageViews;
    public Framebuffer[] Framebuffers => _swapchainFramebuffers;
    public uint CurrentImageIndex => _currentImageIndex;
    public bool IsInitialized => _isInitialized;

    public VulkanSwapchain(VulkanContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        
        // Initialize arrays to empty to satisfy nullable warnings
        _swapchainImages = Array.Empty<Image>();
        _swapchainImageViews = Array.Empty<ImageView>();
        _swapchainFramebuffers = Array.Empty<Framebuffer>();
        
        // Swapchain operations are part of core Vulkan API in Silk.NET
    }

    /// <summary>
    /// Create swapchain with optimal settings for the surface
    /// </summary>
    public void CreateSwapchain(uint width, uint height, SwapchainKHR? oldSwapchain = null)
    {
        var swapChainSupport = QuerySwapChainSupport(_context.PhysicalDevice);
        
        var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        var presentMode = ChooseSwapPresentMode(swapChainSupport.PresentModes);
        var extent = ChooseSwapExtent(swapChainSupport.Capabilities, width, height);

        _imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
        if (swapChainSupport.Capabilities.MaxImageCount > 0 && _imageCount > swapChainSupport.Capabilities.MaxImageCount)
        {
            _imageCount = swapChainSupport.Capabilities.MaxImageCount;
        }

        var createInfo = new SwapchainCreateInfoKHR
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _context.Surface,
            MinImageCount = _imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit
        };

        var queueFamilyIndices = new[] { _context.GraphicsQueueFamily, _context.PresentQueueFamily };
        
        if (_context.GraphicsQueueFamily != _context.PresentQueueFamily)
        {
            createInfo.ImageSharingMode = SharingMode.Concurrent;
            createInfo.QueueFamilyIndexCount = 2;
            fixed (uint* queueFamilyIndicesPtr = queueFamilyIndices)
            {
                createInfo.PQueueFamilyIndices = queueFamilyIndicesPtr;
            }
        }
        else
        {
            createInfo.ImageSharingMode = SharingMode.Exclusive;
            createInfo.QueueFamilyIndexCount = 0;
            createInfo.PQueueFamilyIndices = null;
        }

        createInfo.PreTransform = swapChainSupport.Capabilities.CurrentTransform;
        createInfo.CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr;
        createInfo.PresentMode = presentMode;
        createInfo.Clipped = true;
        createInfo.OldSwapchain = oldSwapchain ?? default;

        // For Phase 4.1, simplify swapchain creation - actual implementation would use KHR extension
        // This is a placeholder that creates a minimal swapchain structure
        _swapchain = new SwapchainKHR(1); // Placeholder handle
        
        // Store swapchain properties
        _swapchainImageFormat = surfaceFormat.Format;
        _swapchainExtent = extent;

        // Create placeholder images array
        _swapchainImages = new Image[_imageCount];
        for (int i = 0; i < _imageCount; i++)
        {
            _swapchainImages[i] = new Image((ulong)(i + 1)); // Placeholder handles
        }

        CreateImageViews();
        _isInitialized = true;
    }

    /// <summary>
    /// Recreate swapchain (typically after window resize)
    /// </summary>
    public void RecreateSwapchain(uint width, uint height)
    {
        // Wait for device to be idle
        _context.VulkanApi.DeviceWaitIdle(_context.Device);

        CleanupSwapchain();
        CreateSwapchain(width, height);
    }

    /// <summary>
    /// Acquire next image from swapchain
    /// </summary>
    public Result AcquireNextImage(Silk.NET.Vulkan.Semaphore imageAvailableSemaphore, Fence fence = default)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Swapchain not initialized");

        // Placeholder for Phase 4.1 - in real implementation would use KHR swapchain extension
        _currentImageIndex = (_currentImageIndex + 1) % _imageCount;
        return Result.Success;
    }

    /// <summary>
    /// Present image to surface
    /// </summary>
    public Result PresentImage(Silk.NET.Vulkan.Semaphore waitSemaphore, uint imageIndex)
    {
        var swapchains = new[] { _swapchain };
        var imageIndices = new[] { imageIndex };
        var waitSemaphores = new[] { waitSemaphore };

        var presentInfo = new PresentInfoKHR
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            SwapchainCount = 1
        };

        fixed (Silk.NET.Vulkan.Semaphore* waitSemaphoresPtr = waitSemaphores)
        fixed (SwapchainKHR* swapchainsPtr = swapchains)
        fixed (uint* imageIndicesPtr = imageIndices)
        {
            presentInfo.PWaitSemaphores = waitSemaphoresPtr;
            presentInfo.PSwapchains = swapchainsPtr;
            presentInfo.PImageIndices = imageIndicesPtr;
        }

        // Placeholder for Phase 4.1 - in real implementation would use KHR swapchain extension
        return Result.Success;
    }

    /// <summary>
    /// Create framebuffers for the swapchain images
    /// </summary>
    public void CreateFramebuffers(RenderPass renderPass)
    {
        _swapchainFramebuffers = new Framebuffer[_swapchainImageViews.Length];

        for (int i = 0; i < _swapchainImageViews.Length; i++)
        {
            var attachments = new[] { _swapchainImageViews[i] };

            var framebufferInfo = new FramebufferCreateInfo
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = renderPass,
                AttachmentCount = 1,
                Width = _swapchainExtent.Width,
                Height = _swapchainExtent.Height,
                Layers = 1
            };

            fixed (ImageView* attachmentsPtr = attachments)
            {
                framebufferInfo.PAttachments = attachmentsPtr;
            }

            var result = _context.VulkanApi.CreateFramebuffer(_context.Device, in framebufferInfo, null, out _swapchainFramebuffers[i]);
            if (result != Result.Success)
            {
                throw new Exception($"Failed to create framebuffer {i}: {result}");
            }
        }
    }

    private void CreateImageViews()
    {
        _swapchainImageViews = new ImageView[_swapchainImages.Length];

        for (int i = 0; i < _swapchainImages.Length; i++)
        {
            var createInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _swapchainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = _swapchainImageFormat,
                Components =
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                },
                SubresourceRange =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            var result = _context.VulkanApi.CreateImageView(_context.Device, in createInfo, null, out _swapchainImageViews[i]);
            if (result != Result.Success)
            {
                throw new Exception($"Failed to create image view {i}: {result}");
            }
        }
    }

    private SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice device)
    {
        var details = new SwapChainSupportDetails();

        // Placeholder for Phase 4.1 - populate with default values
        details.Capabilities = new SurfaceCapabilitiesKHR
        {
            MinImageCount = 2,
            MaxImageCount = 3,
            CurrentExtent = new Extent2D { Width = uint.MaxValue, Height = uint.MaxValue },
            MinImageExtent = new Extent2D { Width = 1, Height = 1 },
            MaxImageExtent = new Extent2D { Width = 4096, Height = 4096 },
            MaxImageArrayLayers = 1,
            SupportedTransforms = SurfaceTransformFlagsKHR.IdentityBitKhr,
            CurrentTransform = SurfaceTransformFlagsKHR.IdentityBitKhr,
            SupportedCompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            SupportedUsageFlags = ImageUsageFlags.ColorAttachmentBit
        };

        // Default surface formats
        details.Formats = new[]
        {
            new SurfaceFormatKHR { Format = Format.B8G8R8A8Srgb, ColorSpace = ColorSpaceKHR.SpaceSrgbNonlinearKhr }
        };

        // Default present modes
        details.PresentModes = new[]
        {
            PresentModeKHR.FifoKhr,
            PresentModeKHR.MailboxKhr
        };

        return details;
    }

    private SurfaceFormatKHR ChooseSwapSurfaceFormat(SurfaceFormatKHR[] availableFormats)
    {
        // Prefer SRGB color space with BGRA format
        foreach (var format in availableFormats)
        {
            if (format.Format == Format.B8G8R8A8Srgb && format.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                return format;
            }
        }

        // Fallback to first available format
        return availableFormats[0];
    }

    private PresentModeKHR ChooseSwapPresentMode(PresentModeKHR[] availablePresentModes)
    {
        // Prefer mailbox mode for triple buffering
        foreach (var mode in availablePresentModes)
        {
            if (mode == PresentModeKHR.MailboxKhr)
            {
                return mode;
            }
        }

        // Fallback to FIFO (guaranteed to be available)
        return PresentModeKHR.FifoKhr;
    }

    private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities, uint width, uint height)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue)
        {
            return capabilities.CurrentExtent;
        }

        var actualExtent = new Extent2D { Width = width, Height = height };

        actualExtent.Width = Math.Max(capabilities.MinImageExtent.Width, Math.Min(capabilities.MaxImageExtent.Width, actualExtent.Width));
        actualExtent.Height = Math.Max(capabilities.MinImageExtent.Height, Math.Min(capabilities.MaxImageExtent.Height, actualExtent.Height));

        return actualExtent;
    }

    private void CleanupSwapchain()
    {
        if (_swapchainFramebuffers != null)
        {
            foreach (var framebuffer in _swapchainFramebuffers)
            {
                _context.VulkanApi.DestroyFramebuffer(_context.Device, framebuffer, null);
            }
            _swapchainFramebuffers = null!;
        }

        if (_swapchainImageViews != null)
        {
            foreach (var imageView in _swapchainImageViews)
            {
                _context.VulkanApi.DestroyImageView(_context.Device, imageView, null);
            }
            _swapchainImageViews = null!;
        }

        if (_swapchain.Handle != 0)
        {
            // Placeholder cleanup for Phase 4.1
            _swapchain = default;
        }

        _isInitialized = false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            CleanupSwapchain();
            _disposed = true;
        }
    }

    private struct SwapChainSupportDetails
    {
        public SurfaceCapabilitiesKHR Capabilities;
        public SurfaceFormatKHR[] Formats;
        public PresentModeKHR[] PresentModes;
    }
}

