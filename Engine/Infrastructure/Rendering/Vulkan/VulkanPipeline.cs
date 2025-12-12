using System;
using System.Collections.Generic;
using System.IO;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Engine.Domain.Rendering.ValueObjects;

namespace Engine.Infrastructure.Rendering.Vulkan;

/// <summary>
/// Manages Vulkan graphics pipelines for rendering operations
/// </summary>
public unsafe class VulkanPipeline : IDisposable
{
    private readonly VulkanContext _context;
    private readonly VulkanShaderManager _shaderManager;
    private bool _disposed;
    
    private Pipeline _graphicsPipeline;
    private PipelineLayout _pipelineLayout;
    private bool _isCreated;

    public Pipeline Handle => _graphicsPipeline;
    public PipelineLayout Layout => _pipelineLayout;
    public bool IsCreated => _isCreated;

    public VulkanPipeline(VulkanContext context, VulkanShaderManager shaderManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _shaderManager = shaderManager ?? throw new ArgumentNullException(nameof(shaderManager));
    }

    /// <summary>
    /// Create a basic graphics pipeline for rendering colored vertices
    /// </summary>
    public void CreateBasicGraphicsPipeline(RenderPass renderPass, Extent2D viewportExtent)
    {
        if (_isCreated)
            throw new InvalidOperationException("Pipeline already created");

        // Load shaders (assuming they exist)
        var vertShaderModule = LoadShader("Shaders/Default.vert.spv");
        var fragShaderModule = LoadShader("Shaders/Default.frag.spv");

        try
        {
            CreateGraphicsPipeline(vertShaderModule, fragShaderModule, renderPass, viewportExtent);
        }
        finally
        {
            // Cleanup shader modules
            _context.VulkanApi.DestroyShaderModule(_context.Device, vertShaderModule, null);
            _context.VulkanApi.DestroyShaderModule(_context.Device, fragShaderModule, null);
        }
    }

    /// <summary>
    /// Create a graphics pipeline with custom vertex input
    /// </summary>
    public void CreateVertexPipeline(RenderPass renderPass, Extent2D viewportExtent, 
        VertexInputBindingDescription[] bindingDescriptions,
        VertexInputAttributeDescription[] attributeDescriptions)
    {
        if (_isCreated)
            throw new InvalidOperationException("Pipeline already created");

        // Load shaders
        var vertShaderModule = LoadShader("Shaders/Default.vert.spv");
        var fragShaderModule = LoadShader("Shaders/Default.frag.spv");

        try
        {
            CreateGraphicsPipeline(vertShaderModule, fragShaderModule, renderPass, viewportExtent, 
                bindingDescriptions, attributeDescriptions);
        }
        finally
        {
            // Cleanup shader modules
            _context.VulkanApi.DestroyShaderModule(_context.Device, vertShaderModule, null);
            _context.VulkanApi.DestroyShaderModule(_context.Device, fragShaderModule, null);
        }
    }

    private void CreateGraphicsPipeline(ShaderModule vertShaderModule, ShaderModule fragShaderModule, 
        RenderPass renderPass, Extent2D viewportExtent,
        VertexInputBindingDescription[]? bindingDescriptions = null,
        VertexInputAttributeDescription[]? attributeDescriptions = null)
    {
        // Shader stage create infos
        var shaderStages = new PipelineShaderStageCreateInfo[]
        {
            new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vertShaderModule,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            },
            new()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = fragShaderModule,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            }
        };

        // Vertex input state
        var vertexInputInfo = new PipelineVertexInputStateCreateInfo
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo
        };

        if (bindingDescriptions != null && attributeDescriptions != null)
        {
            fixed (VertexInputBindingDescription* bindingPtr = bindingDescriptions)
            fixed (VertexInputAttributeDescription* attributePtr = attributeDescriptions)
            {
                vertexInputInfo.VertexBindingDescriptionCount = (uint)bindingDescriptions.Length;
                vertexInputInfo.PVertexBindingDescriptions = bindingPtr;
                vertexInputInfo.VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length;
                vertexInputInfo.PVertexAttributeDescriptions = attributePtr;
            }
        }

        // Input assembly state
        var inputAssembly = new PipelineInputAssemblyStateCreateInfo
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = PrimitiveTopology.TriangleList,
            PrimitiveRestartEnable = false
        };

        // Viewport state
        var viewport = new Viewport
        {
            X = 0.0f,
            Y = 0.0f,
            Width = viewportExtent.Width,
            Height = viewportExtent.Height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f
        };

        var scissor = new Rect2D
        {
            Offset = { X = 0, Y = 0 },
            Extent = viewportExtent
        };

        var viewportState = new PipelineViewportStateCreateInfo
        {
            SType = StructureType.PipelineViewportStateCreateInfo,
            ViewportCount = 1,
            PViewports = &viewport,
            ScissorCount = 1,
            PScissors = &scissor
        };

        // Rasterizer state
        var rasterizer = new PipelineRasterizationStateCreateInfo
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            DepthClampEnable = false,
            RasterizerDiscardEnable = false,
            PolygonMode = PolygonMode.Fill,
            LineWidth = 1.0f,
            CullMode = CullModeFlags.BackBit,
            FrontFace = FrontFace.Clockwise,
            DepthBiasEnable = false
        };

        // Multisampling state
        var multisampling = new PipelineMultisampleStateCreateInfo
        {
            SType = StructureType.PipelineMultisampleStateCreateInfo,
            SampleShadingEnable = false,
            RasterizationSamples = SampleCountFlags.Count1Bit
        };

        // Color blend attachment state
        var colorBlendAttachment = new PipelineColorBlendAttachmentState
        {
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | 
                           ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            BlendEnable = false
        };

        // Color blend state
        var colorBlending = new PipelineColorBlendStateCreateInfo
        {
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            LogicOpEnable = false,
            LogicOp = LogicOp.Copy,
            AttachmentCount = 1,
            PAttachments = &colorBlendAttachment
        };

        // Dynamic state (for viewport and scissor)
        var dynamicStates = new DynamicState[]
        {
            DynamicState.Viewport,
            DynamicState.Scissor
        };

        fixed (DynamicState* dynamicStatesPtr = dynamicStates)
        {
            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = (uint)dynamicStates.Length,
                PDynamicStates = dynamicStatesPtr
            };

            // Create pipeline layout
            CreatePipelineLayout();

            // Graphics pipeline create info
            fixed (PipelineShaderStageCreateInfo* shaderStagesPtr = shaderStages)
            {
                var pipelineInfo = new GraphicsPipelineCreateInfo
                {
                    SType = StructureType.GraphicsPipelineCreateInfo,
                    StageCount = (uint)shaderStages.Length,
                    PStages = shaderStagesPtr,
                    PVertexInputState = &vertexInputInfo,
                    PInputAssemblyState = &inputAssembly,
                    PViewportState = &viewportState,
                    PRasterizationState = &rasterizer,
                    PMultisampleState = &multisampling,
                    PColorBlendState = &colorBlending,
                    PDynamicState = &dynamicState,
                    Layout = _pipelineLayout,
                    RenderPass = renderPass,
                    Subpass = 0
                };

                var result = _context.VulkanApi.CreateGraphicsPipelines(_context.Device, default, 1, 
                    in pipelineInfo, null, out _graphicsPipeline);
                
                if (result != Result.Success)
                {
                    throw new Exception($"Failed to create graphics pipeline: {result}");
                }
            }
        }

        // Cleanup shader stage names
        SilkMarshal.Free((nint)shaderStages[0].PName);
        SilkMarshal.Free((nint)shaderStages[1].PName);

        _isCreated = true;
    }

    private void CreatePipelineLayout()
    {
        var pipelineLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 0,
            PushConstantRangeCount = 0
        };

        var result = _context.VulkanApi.CreatePipelineLayout(_context.Device, in pipelineLayoutInfo, 
            null, out _pipelineLayout);
        
        if (result != Result.Success)
        {
            throw new Exception($"Failed to create pipeline layout: {result}");
        }
    }

    private ShaderModule LoadShader(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // For Phase 4.1, create a simple placeholder shader
            return CreatePlaceholderShader(filePath.Contains("vert"));
        }

        var code = File.ReadAllBytes(filePath);
        return CreateShaderModule(code);
    }

    private ShaderModule CreateShaderModule(byte[] code)
    {
        fixed (byte* codePtr = code)
        {
            var createInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)code.Length,
                PCode = (uint*)codePtr
            };

            var result = _context.VulkanApi.CreateShaderModule(_context.Device, in createInfo, null, out var shaderModule);
            if (result != Result.Success)
            {
                throw new Exception($"Failed to create shader module: {result}");
            }

            return shaderModule;
        }
    }

    private ShaderModule CreatePlaceholderShader(bool isVertex)
    {
        // Create minimal shader for basic rendering
        // This is a placeholder - real shaders should be compiled separately
        if (isVertex)
        {
            // Simple vertex shader SPIR-V (basic passthrough with color)
            var vertexSpirv = new uint[]
            {
                0x07230203, 0x00010000, 0x00080001, 0x00000028, 0x00000000, 0x00020011, 0x00000001,
                0x0006000B, 0x00000001, 0x4C534C47, 0x6474732E, 0x3035342E, 0x00000000, 0x0003000E,
                0x00000000, 0x00000001
                // ... (truncated for brevity - would be full SPIR-V)
            };

            return CreateShaderModuleFromUints(vertexSpirv);
        }
        else
        {
            // Simple fragment shader SPIR-V (solid red color)
            var fragmentSpirv = new uint[]
            {
                0x07230203, 0x00010000, 0x00080001, 0x00000013, 0x00000000, 0x00020011, 0x00000001,
                0x0006000B, 0x00000001, 0x4C534C47, 0x6474732E, 0x3035342E, 0x00000000, 0x0003000E,
                0x00000000, 0x00000001
                // ... (truncated for brevity - would be full SPIR-V)
            };

            return CreateShaderModuleFromUints(fragmentSpirv);
        }
    }

    private ShaderModule CreateShaderModuleFromUints(uint[] code)
    {
        fixed (uint* codePtr = code)
        {
            var createInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)(code.Length * sizeof(uint)),
                PCode = codePtr
            };

            var result = _context.VulkanApi.CreateShaderModule(_context.Device, in createInfo, null, out var shaderModule);
            if (result != Result.Success)
            {
                throw new Exception($"Failed to create placeholder shader module: {result}");
            }

            return shaderModule;
        }
    }

    /// <summary>
    /// Get vertex input descriptions for the Vertex struct
    /// </summary>
    public static (VertexInputBindingDescription[], VertexInputAttributeDescription[]) GetVertexDescriptions()
    {
        var bindingDescription = new VertexInputBindingDescription
        {
            Binding = 0,
            Stride = (uint)sizeof(Vertex),
            InputRate = VertexInputRate.Vertex
        };

        var attributeDescriptions = new VertexInputAttributeDescription[]
        {
            new() // Position
            {
                Binding = 0,
                Location = 0,
                Format = Format.R32G32B32Sfloat,
                Offset = 0
            },
            new() // Color
            {
                Binding = 0,
                Location = 1,
                Format = Format.R32G32B32Sfloat,
                Offset = 12 // 3 floats * 4 bytes each
            }
        };

        return (new[] { bindingDescription }, attributeDescriptions);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_graphicsPipeline.Handle != 0)
            {
                _context.VulkanApi.DestroyPipeline(_context.Device, _graphicsPipeline, null);
            }

            if (_pipelineLayout.Handle != 0)
            {
                _context.VulkanApi.DestroyPipelineLayout(_context.Device, _pipelineLayout, null);
            }

            _disposed = true;
        }
    }
}

