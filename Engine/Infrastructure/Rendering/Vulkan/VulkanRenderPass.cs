using System;
using Silk.NET.Vulkan;

namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Manages Vulkan render passes for organizing rendering operations
/// </summary>
public unsafe class VulkanRenderPass : IDisposable
{
    private readonly VulkanContext _context;
    private bool _disposed;
    
    private RenderPass _renderPass;
    private bool _isCreated;

    public RenderPass Handle => _renderPass;
    public bool IsCreated => _isCreated;

    public VulkanRenderPass(VulkanContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Create a basic color render pass
    /// </summary>
    public void CreateColorRenderPass(Format colorFormat)
    {
        if (_isCreated)
            throw new InvalidOperationException("Render pass already created");

        // Color attachment description
        var colorAttachment = new AttachmentDescription
        {
            Format = colorFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };

        // Color attachment reference
        var colorAttachmentRef = new AttachmentReference
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal
        };

        // Subpass description
        var subpass = new SubpassDescription
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef
        };

        // Subpass dependency
        var dependency = new SubpassDependency
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = 0,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit
        };

        // Create render pass
        var renderPassInfo = new RenderPassCreateInfo
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency
        };

        var result = _context.VulkanApi.CreateRenderPass(_context.Device, in renderPassInfo, null, out _renderPass);
        if (result != Result.Success)
        {
            throw new Exception($"Failed to create render pass: {result}");
        }

        _isCreated = true;
    }

    /// <summary>
    /// Create a render pass with color and depth attachments
    /// </summary>
    public void CreateColorDepthRenderPass(Format colorFormat, Format depthFormat)
    {
        if (_isCreated)
            throw new InvalidOperationException("Render pass already created");

        // Attachment descriptions
        var attachments = new AttachmentDescription[2];
        
        // Color attachment
        attachments[0] = new AttachmentDescription
        {
            Format = colorFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };

        // Depth attachment
        attachments[1] = new AttachmentDescription
        {
            Format = depthFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
        };

        // Attachment references
        var colorAttachmentRef = new AttachmentReference
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal
        };

        var depthAttachmentRef = new AttachmentReference
        {
            Attachment = 1,
            Layout = ImageLayout.DepthStencilAttachmentOptimal
        };

        // Subpass description
        var subpass = new SubpassDescription
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
            PDepthStencilAttachment = &depthAttachmentRef
        };

        // Subpass dependency
        var dependency = new SubpassDependency
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            SrcAccessMask = 0,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
        };

        // Create render pass
        fixed (AttachmentDescription* attachmentsPtr = attachments)
        {
            var renderPassInfo = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 2,
                PAttachments = attachmentsPtr,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency
            };

            var result = _context.VulkanApi.CreateRenderPass(_context.Device, in renderPassInfo, null, out _renderPass);
            if (result != Result.Success)
            {
                throw new Exception($"Failed to create color-depth render pass: {result}");
            }
        }

        _isCreated = true;
    }

    /// <summary>
    /// Create a multi-subpass render pass for deferred rendering
    /// </summary>
    public void CreateDeferredRenderPass(Format colorFormat, Format normalFormat, Format depthFormat)
    {
        if (_isCreated)
            throw new InvalidOperationException("Render pass already created");

        // Attachment descriptions for deferred rendering
        var attachments = new AttachmentDescription[3];
        
        // Color attachment (albedo)
        attachments[0] = new AttachmentDescription
        {
            Format = colorFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.ShaderReadOnlyOptimal
        };

        // Normal attachment
        attachments[1] = new AttachmentDescription
        {
            Format = normalFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.ShaderReadOnlyOptimal
        };

        // Depth attachment
        attachments[2] = new AttachmentDescription
        {
            Format = depthFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
        };

        // Geometry subpass attachments
        var geoColorRef = new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
        var geoNormalRef = new AttachmentReference { Attachment = 1, Layout = ImageLayout.ColorAttachmentOptimal };
        var geoDepthRef = new AttachmentReference { Attachment = 2, Layout = ImageLayout.DepthStencilAttachmentOptimal };
        
        var geoColorRefs = new[] { geoColorRef, geoNormalRef };

        // Lighting subpass attachment references (reading from previous subpass)
        var lightInputRefs = new[] 
        { 
            new AttachmentReference { Attachment = 0, Layout = ImageLayout.ShaderReadOnlyOptimal },
            new AttachmentReference { Attachment = 1, Layout = ImageLayout.ShaderReadOnlyOptimal }
        };

        // Subpasses
        var subpasses = new SubpassDescription[2];
        
        // Geometry subpass
        fixed (AttachmentReference* geoColorRefsPtr = geoColorRefs)
        {
            subpasses[0] = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 2,
                PColorAttachments = geoColorRefsPtr,
                PDepthStencilAttachment = &geoDepthRef
            };
        }

        // Lighting subpass
        fixed (AttachmentReference* lightInputRefsPtr = lightInputRefs)
        {
            subpasses[1] = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                InputAttachmentCount = 2,
                PInputAttachments = lightInputRefsPtr,
                ColorAttachmentCount = 1,
                PColorAttachments = &geoColorRef // Reuse color attachment for final output
            };
        }

        // Dependencies between subpasses
        var dependencies = new SubpassDependency[3];
        
        // External to geometry subpass
        dependencies[0] = new SubpassDependency
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.BottomOfPipeBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = AccessFlags.MemoryReadBit,
            DstAccessMask = AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit,
            DependencyFlags = DependencyFlags.ByRegionBit
        };

        // Geometry to lighting subpass
        dependencies[1] = new SubpassDependency
        {
            SrcSubpass = 0,
            DstSubpass = 1,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstStageMask = PipelineStageFlags.FragmentShaderBit,
            SrcAccessMask = AccessFlags.ColorAttachmentWriteBit,
            DstAccessMask = AccessFlags.InputAttachmentReadBit,
            DependencyFlags = DependencyFlags.ByRegionBit
        };

        // Lighting to external
        dependencies[2] = new SubpassDependency
        {
            SrcSubpass = 1,
            DstSubpass = Vk.SubpassExternal,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstStageMask = PipelineStageFlags.BottomOfPipeBit,
            SrcAccessMask = AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit,
            DstAccessMask = AccessFlags.MemoryReadBit,
            DependencyFlags = DependencyFlags.ByRegionBit
        };

        // Create render pass
        fixed (AttachmentDescription* attachmentsPtr = attachments)
        fixed (SubpassDescription* subpassesPtr = subpasses)
        fixed (SubpassDependency* dependenciesPtr = dependencies)
        {
            var renderPassInfo = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = 3,
                PAttachments = attachmentsPtr,
                SubpassCount = 2,
                PSubpasses = subpassesPtr,
                DependencyCount = 3,
                PDependencies = dependenciesPtr
            };

            var result = _context.VulkanApi.CreateRenderPass(_context.Device, in renderPassInfo, null, out _renderPass);
            if (result != Result.Success)
            {
                throw new Exception($"Failed to create deferred render pass: {result}");
            }
        }

        _isCreated = true;
    }

    /// <summary>
    /// Get clear values for the render pass
    /// </summary>
    public ClearValue[] GetClearValues(ClearColorValue? clearColor = null, float depth = 1.0f, uint stencil = 0)
    {
        var colorValue = clearColor ?? new ClearColorValue(0.0f, 0.0f, 0.0f, 1.0f);
        
        return new[]
        {
            new ClearValue { Color = colorValue },
            new ClearValue { DepthStencil = new ClearDepthStencilValue(depth, stencil) }
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_renderPass.Handle != 0)
            {
                _context.VulkanApi.DestroyRenderPass(_context.Device, _renderPass, null);
            }
            _disposed = true;
        }
    }
}