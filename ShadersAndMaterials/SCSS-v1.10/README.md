<div style="width: 5em">
![Silent's Cel Shading Shaders](https://cdn.discordapp.com/attachments/414634326995763201/758190255521398784/SCSS_header_1.png)
</div>

Shaders for Unity for cel shading, designed to take Unity's lighting into account while also presenting materials in the best possible way. Featuring lots of features with good performance!
# [Want to know how to use this shader? Here's the manual!](https://gitlab.com/s-ilent/SCSS/wikis/Manual/Setting-Overview)
# [Can't find the Download link? Click here!](https://gitlab.com/s-ilent/SCSS/-/archive/master/SCSS-master.zip)
* After downloading, install the shader by moving the contents of the Assets folder into your project's Assets folder.

<div style="width: 5em">
![Suitable for shade or shine!](https://cdn.discordapp.com/attachments/414634326995763201/758184322708275220/Crosstone_proto.jpg)
</div>

## Features include:
* **Customisable lighting**

  The shadow tone system allows for true anime-style material shade colouring and light bias.<br>
  Use the Crosstone system and define multiple shadow tones with ramp parameters, or provide your own light ramp texture!<br>
  All integrated with Unity's lighting system!

* **NPR**

  SCSS contains a unique matcap system. You can combine multiple blend modes and multiple matcaps. <br>
  They can be anchored in world or tangent space, stopping them from shifting with head movement in VR. <br>
  Customisable ambient and emissive rim lights are also provided for highlight effects. <br>
  Cel-shaded specular gives you a stylised shiny highlight.

* **PBR**

  Contains metalness and gloss functionality accurate to Unity's Standard shader. <br>
  You can combine a cel-shaded material with realistic metal and gloss using the same parameters as Standard.<br>
  Detail maps are supported, allowing you to give materials a close-up fine texture. <br>
  You can also use the secondary UV channel to add isolated details through decals.

* **Outlines and control**

  The outline system is optimised for VR, with outlines that reduce size based on camera proximity to avoid models breaking up at close inspection. Outline size can be finely controlled using the vertex colour channels. 

* **Advanced Options**

  Many advanced options for blend mode and more. Provides support for using premutiplied transparency, which allows for glossy transparent objects that naturally fit into their surroundings.

* **VRchat Features**

  Simple Inventory System lets you toggle parts of a material easily, allowing for runtime customisation of clothing without the costs of extra skinned meshes.<br>
  AudioLink compatibility allows for audio-reactive effects on materials, integrated into the emission system.<br>
  Shader baking creates an optimised variant of each material's shaders, allowing for a smaller upload size and better runtime performance. 

<div style="width: 5em">
![Too Much Preview](https://cdn.discordapp.com/attachments/414634326995763201/694118872110071880/screen_10328x5640_2020-03-30_19-58-06.png.jpg)
</div>

# [For more details, please check the setting overview!](https://gitlab.com/s-ilent/SCSS/wikis/Manual/Setting-Overview)

Tested with Unity 2018.4.20f1 LTS and Unity 2019.4.13f1.

For support, contact me on Discord or Twitter.
