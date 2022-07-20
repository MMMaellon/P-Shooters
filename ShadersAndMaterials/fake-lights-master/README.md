# Fake Lights

Shaders for Unity/VRchat that illuminate the area around them in a way almost, but not quite, like light. They're instanced, too, so multiples of the same light material and mesh will generally only take one draw call.

![Preview](https://cdn.discordapp.com/attachments/512943124436353025/545590755243327488/fakelights.jpg)

## Installation

Download the repository. Then place the Shader/ folder with the shader into your Assets/ directory.

## Usage

Provided in this package are:

- The main Volumetric Fake Lights shader.
- An inverted icosphere mesh, used to render the lights.
- A sample prefab and material for a single fake light.
- A sample material for a particle system. 

If not using the prefab, drag the icosphere into your scene and assign it a material with one of the Fake Light shaders. Done! If this doesn't work, you might need to force the depth buffer to be active. [See here for info.](https://github.com/Xiexe/XSVolumetrics)

Note that, like other shaders which depend on the depth buffer, you'll need a directional light or other depth buffer activator (like disabled depth of field) to see the effect properly.

For best batching results, give each material a different render queue, because materials on the same queue render in random order and Unity won't batch them if they aren't sorted properly.

## UI is weird!

Probably will be fixed later.

## The default settings are broken!

Play around and see what works!

## License?

This work is licensed under the MIT license where applicable.