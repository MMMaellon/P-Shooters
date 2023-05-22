# Magic Particles

A simple shader for Unity to create seemingly complex particle effects. 

## Installation

Download the repository. Then place the Shader/ folder with the shader into your Assets/ directory.

## Usage

The idea behind this shader is that you can create complex effects with only a few "moving parts".

*	**Tint Color and Intensity**<br />
	These control the colour and brightness of the material. Intensity at 2.0 will give your particles a nice glow in normal circumstances.
*	**Main Texture**<br />
	This is where you place your particle's main texture. RGB and alpha channels are used.
*	**Scroll Power**<br />
	If above 0, the main texture will scroll along the X/Y axis. (Z/W are unused.)
*	**Blend Mode**<br />
	Sets how the material is blended against the world behind it. This has a huge effect on the material's appearance. Here are some common blend modes.
	*	**One OneMinusSrcAlpha** - Premultiplied transparency (default)
	*	**SrcAlpha OneMinusSrcAlpha** - Traditional transparency
	*	**One One** - Additive
	*	**OneMinusDstColor One** - Soft Additive
	*	**DstColor Zero** - Multiplicative
	*	**DstColor SrcColor** - Double Multiplicative<br />
*	**Cutoff and Cutoff Softness**<br />
	Alpha values below the cutoff will not be displayed. There is a transition between visible and cutoff controllable with the softness parameter.
*	**Use Bicubic Filtering**<br />
	Only use this with simple particles. It uses a more complex sampling method which takes extra texture samples to smooth out visible pixellation.
*	**Apply Fog**<br />
	This setting allows you to choose the method of fog application. When using additive blending, blending towards the fog colour in fog will just make the material glow. Use "To Black" when blending additively.
*	**Use Soft Particle**<br />
	If available, softens the edge of the material against the world. 
*	**View Offset**<br />
	When greater than 0, pushes the material closer to the camera in world space to simulate a more volumetric shape.
*	**Render at Clip Plane**<br />
	These options push the particle over or under everything else. Please use them sparingly.
*	**Visibility Range**<br />
	Hides the particle when not within this distance of this origin.
*	**Gradient Maps**<br />
	These ramp textures remap the main texture from greyscale to their own colouring. This allows you to have cool colour shift effects across both the colour and alpha. You can create them with the gradient editor in [my shader](https://gitlab.com/s-ilent/SCSS).

	You can store multiple gradients in a single texture (along the Y axis) and select them with Custom Vertex Streams, see below.
*	**Second Layer**<br />
	Applies a second layer of texture over the main texture. This second layer can be scrolled seperately. 
*	**Distortion**<br />
	Warps the texture sampling position with a normal map, which can add big and small details to an otherwise simple effect. Warp Power's X/Y controls the scroll speed, and Z/W control the strength the warp is applied with.
*	**Custom Parameter for Particle Systems**<br />
	When enabled, this allows you to send parameters to the shader with the Custom Vertex Streams of a Particle System. They must be sent through TEXCOORD1. When enabled, the parameters will specify what channel affects what parameter.
*	**Cull Mode**<br />
	Which side of the polygon is culled. 

## License?
MIT license!