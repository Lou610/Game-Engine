using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Core Vulkan context managing instance, device, and queues
/// </summary>
public unsafe class VulkanContext : IDisposable
{
    private readonly Vk _vk;
    private bool _disposed;
    
    // Vulkan objects
    private Instance _instance;
    private PhysicalDevice _physicalDevice;
    private Device _device;
    private SurfaceKHR _surface;
    
    // Queue handles
    private Queue _graphicsQueue;
    private Queue _presentQueue;
    private uint _graphicsQueueFamily;
    private uint _presentQueueFamily;
    
    // Extensions - will be implemented later
    
    // Debug messenger
    private DebugUtilsMessengerEXT _debugMessenger;
    
    // Properties
    public Instance Instance => _instance;
    public PhysicalDevice PhysicalDevice => _physicalDevice;
    public Device Device => _device;
    public SurfaceKHR Surface => _surface;
    public Queue GraphicsQueue => _graphicsQueue;
    public Queue PresentQueue => _presentQueue;
    public uint GraphicsQueueFamily => _graphicsQueueFamily;
    public uint PresentQueueFamily => _presentQueueFamily;
    public Vk VulkanApi => _vk;
    // Extension properties - will be implemented later
    
    public bool IsInitialized { get; private set; }
    
    public VulkanContext()
    {
        _vk = Vk.GetApi();
    }
    
    /// <summary>
    /// Initialize Vulkan instance and debug layers
    /// </summary>
    public void Initialize(string applicationName, Version applicationVersion, bool enableValidation = true)
    {
        if (IsInitialized)
            throw new InvalidOperationException("VulkanContext is already initialized");
            
        try
        {
            CreateInstance(applicationName, applicationVersion, enableValidation);
            
            if (enableValidation)
            {
                SetupDebugMessenger();
            }
            
            IsInitialized = true;
        }
        catch
        {
            Cleanup();
            throw;
        }
    }
    
    /// <summary>
    /// Create Vulkan surface for rendering
    /// </summary>
    public void CreateSurface(nint windowHandle)
    {
        ThrowIfNotInitialized();
        
        // Surface creation will be implemented in a later phase
        // For now, we'll focus on the core Vulkan setup without windowing
        throw new NotImplementedException("Surface creation will be implemented with windowing system integration");
    }
    
    /// <summary>
    /// Select appropriate physical device
    /// </summary>
    public void SelectPhysicalDevice()
    {
        ThrowIfNotInitialized();
        
        uint deviceCount = 0;
        _vk.EnumeratePhysicalDevices(_instance, ref deviceCount, null);
        
        if (deviceCount == 0)
            throw new InvalidOperationException("No Vulkan-compatible devices found");
            
        var devices = new PhysicalDevice[deviceCount];
        fixed (PhysicalDevice* devicesPtr = devices)
        {
            _vk.EnumeratePhysicalDevices(_instance, ref deviceCount, devicesPtr);
        }
        
        // Score devices and select the best one
        var bestDevice = devices
            .Select(device => new { Device = device, Score = ScoreDevice(device) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault()?.Device ?? default;
            
        if (bestDevice.Handle == 0)
            throw new InvalidOperationException("No suitable Vulkan device found");
            
        _physicalDevice = bestDevice;
    }
    
    /// <summary>
    /// Create logical device and retrieve queues
    /// </summary>
    public void CreateLogicalDevice()
    {
        ThrowIfNotInitialized();
        
        if (_physicalDevice.Handle == 0)
            throw new InvalidOperationException("Physical device not selected");
            
        // Find queue families
        var queueFamilies = FindQueueFamilies(_physicalDevice);
        
        _graphicsQueueFamily = queueFamilies.GraphicsFamily!.Value;
        _presentQueueFamily = queueFamilies.PresentFamily!.Value;
        
        // Create device queue create infos
        var uniqueQueueFamilies = new HashSet<uint> { _graphicsQueueFamily, _presentQueueFamily };
        var queueCreateInfos = new DeviceQueueCreateInfo[uniqueQueueFamilies.Count];
        var queuePriority = 1.0f;
        
        int i = 0;
        foreach (var queueFamily in uniqueQueueFamilies)
        {
            queueCreateInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = queueFamily,
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };
            i++;
        }
        
        // Device features
        var deviceFeatures = new PhysicalDeviceFeatures();
        
        // Device extensions - for now, no extensions required
        // Will add swapchain extension when implementing surface support
        
        // Create device
        var createInfo = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)queueCreateInfos.Length,
            PQueueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref queueCreateInfos[0]),
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = 0,
            PpEnabledExtensionNames = null
        };
        
        fixed (Device* devicePtr = &_device)
        {
            var result = _vk.CreateDevice(_physicalDevice, &createInfo, null, devicePtr);
            if (result != Result.Success)
                throw new InvalidOperationException($"Failed to create logical device: {result}");
        }
        
        // Get queue handles
        _vk.GetDeviceQueue(_device, _graphicsQueueFamily, 0, out _graphicsQueue);
        _vk.GetDeviceQueue(_device, _presentQueueFamily, 0, out _presentQueue);
        
        // Swapchain extension will be loaded when needed
    }
    
    /// <summary>
    /// Wait for device to become idle
    /// </summary>
    public void WaitIdle()
    {
        if (_device.Handle != 0)
        {
            _vk.DeviceWaitIdle(_device);
        }
    }
    
    private void CreateInstance(string applicationName, Version applicationVersion, bool enableValidation)
    {
        // Application info
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)SilkMarshal.StringToPtr(applicationName),
            ApplicationVersion = Vk.MakeVersion((uint)applicationVersion.Major, (uint)applicationVersion.Minor, (uint)applicationVersion.Build),
            PEngineName = (byte*)SilkMarshal.StringToPtr("LAG Game Engine"),
            EngineVersion = Vk.MakeVersion(1, 0, 0),
            ApiVersion = Vk.Version12
        };
        
        // Required extensions
        var extensions = GetRequiredExtensions(enableValidation);
        var extensionPtrs = new IntPtr[extensions.Length];
        for (int i = 0; i < extensions.Length; i++)
        {
            extensionPtrs[i] = SilkMarshal.StringToPtr(extensions[i]);
        }
        
        // Validation layers
        var validationLayers = enableValidation ? new[] { "VK_LAYER_KHRONOS_validation" } : Array.Empty<string>();
        var layerPtrs = new IntPtr[validationLayers.Length];
        for (int i = 0; i < validationLayers.Length; i++)
        {
            layerPtrs[i] = SilkMarshal.StringToPtr(validationLayers[i]);
        }
        
        // Create instance
        fixed (IntPtr* extensionPtrArray = extensionPtrs)
        fixed (IntPtr* layerPtrArray = layerPtrs)
        {
            var createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = (uint)extensionPtrs.Length,
                PpEnabledExtensionNames = (byte**)extensionPtrArray,
                EnabledLayerCount = (uint)layerPtrs.Length,
                PpEnabledLayerNames = layerPtrs.Length > 0 ? (byte**)layerPtrArray : null
            };
        
            fixed (Instance* instancePtr = &_instance)
            {
                var result = _vk.CreateInstance(&createInfo, null, instancePtr);
                if (result != Result.Success)
                    throw new InvalidOperationException($"Failed to create Vulkan instance: {result}");
            }
        }
        
        // Clean up allocated strings
        foreach (var ptr in extensionPtrs)
            SilkMarshal.Free(ptr);
        foreach (var ptr in layerPtrs)
            SilkMarshal.Free(ptr);
        
        // Cleanup application info strings
        SilkMarshal.Free((nint)appInfo.PApplicationName);
        SilkMarshal.Free((nint)appInfo.PEngineName);
    }
    
    private void CreateWin32Surface(nint windowHandle)
    {
        // Implementation would go here for Win32 surface creation
        throw new NotImplementedException("Win32 surface creation not implemented yet");
    }
    
    private void CreateXlibSurface(nint windowHandle)
    {
        // Implementation would go here for Xlib surface creation
        throw new NotImplementedException("Xlib surface creation not implemented yet");
    }
    
    private void CreateMacOSSurface(nint windowHandle)
    {
        // Implementation would go here for macOS surface creation
        throw new NotImplementedException("macOS surface creation not implemented yet");
    }
    
    private void SetupDebugMessenger()
    {
        // Debug messenger setup would go here
        // This is a simplified version - full implementation would include debug callback
    }
    
    private int ScoreDevice(PhysicalDevice device)
    {
        _vk.GetPhysicalDeviceProperties(device, out var deviceProperties);
        _vk.GetPhysicalDeviceFeatures(device, out var deviceFeatures);
        
        int score = 0;
        
        // Discrete GPUs have a significant performance advantage
        if (deviceProperties.DeviceType == PhysicalDeviceType.DiscreteGpu)
            score += 1000;
            
        // Maximum possible size of textures affects graphics quality
        score += (int)deviceProperties.Limits.MaxImageDimension2D;
        
        // Check if device supports required queue families
        var queueFamilies = FindQueueFamilies(device);
        if (!queueFamilies.IsComplete())
            return 0;
            
        // Check device extension support
        if (!CheckDeviceExtensionSupport(device))
            return 0;
            
        return score;
    }
    
    private QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
    {
        var indices = new QueueFamilyIndices();
        
        uint queueFamilyCount = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilyCount, null);
        
        var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
        {
            _vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilyCount, queueFamiliesPtr);
        }
        
        for (uint i = 0; i < queueFamilies.Length; i++)
        {
            var queueFamily = queueFamilies[i];
            
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
            {
                indices.GraphicsFamily = i;
            }
            
            // For now, assume graphics queue can also present
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
            {
                indices.PresentFamily = i;
            }
            
            if (indices.IsComplete())
                break;
        }
        
        return indices;
    }
    
    private bool CheckDeviceExtensionSupport(PhysicalDevice device)
    {
        uint extensionCount = 0;
        _vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, null);
        
        var availableExtensions = new ExtensionProperties[extensionCount];
        fixed (ExtensionProperties* extensionsPtr = availableExtensions)
        {
            _vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, extensionsPtr);
        }
        
        var requiredExtensions = new HashSet<string>(); // No required extensions for now
        
        foreach (var extension in availableExtensions)
        {
            var extensionName = SilkMarshal.PtrToString((nint)extension.ExtensionName);
            requiredExtensions.Remove(extensionName!);
        }
        
        return requiredExtensions.Count == 0;
    }
    
    private string[] GetRequiredExtensions(bool enableValidation)
    {
        var extensions = new List<string>();
        
        // Add platform-specific surface extensions
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            extensions.Add("VK_KHR_win32_surface");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            extensions.Add("VK_KHR_xlib_surface");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            extensions.Add("VK_MVK_macos_surface");
        }
        
        if (enableValidation)
        {
            extensions.Add("VK_EXT_debug_utils");
        }
        
        return extensions.ToArray();
    }
    
    private void ThrowIfNotInitialized()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("VulkanContext is not initialized");
    }
    
    private void Cleanup()
    {
        if (_device.Handle != 0)
        {
            _vk.DestroyDevice(_device, null);
            _device = default;
        }
        
        if (_surface.Handle != 0)
        {
            // Surface cleanup will be implemented with surface support
            _surface = default;
        }
        
        if (_debugMessenger.Handle != 0)
        {
            // Cleanup debug messenger
            _debugMessenger = default;
        }
        
        if (_instance.Handle != 0)
        {
            _vk.DestroyInstance(_instance, null);
            _instance = default;
        }
        
        IsInitialized = false;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            WaitIdle();
            Cleanup();
            _vk?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Helper structure for queue family indices
/// </summary>
internal struct QueueFamilyIndices
{
    public uint? GraphicsFamily;
    public uint? PresentFamily;
    
    public readonly bool IsComplete() => GraphicsFamily.HasValue && PresentFamily.HasValue;
}

