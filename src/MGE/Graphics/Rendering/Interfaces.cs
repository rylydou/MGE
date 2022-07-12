using System;

namespace MGE;

/// <summary>
/// An Implementation of the Graphics Module that supports the OpenGL Graphics API
/// </summary>
public interface IGraphicsOpenGL
{

}

/// <summary>
/// An Implementation of the Graphics Module that supports the Vulkan Graphics API
/// </summary>
public interface IGraphicsVulkan
{
	IntPtr GetVulkanInstancePointer();
}
