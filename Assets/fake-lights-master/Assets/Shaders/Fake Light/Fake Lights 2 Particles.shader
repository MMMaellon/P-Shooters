// Fake point lights, Deus Ex: Human Revolution style
// Unity version by Silent, with lots of help from 1001. 
// Original Volumetric Sphere Density by iq
// http://www.iquilezles.org/www/articles/spheredensity/spheredensity.htm
// Unity version by 1001
// Cleaned up/useability tweaks/instancing support by Silent

// When setting up for particles,
// set Custom Vertex Streams to 
// 		POSITION
// 		COLOR
// 		Center
// 		Size
// And use Billboard mode!

Shader "Silent/Volumetric Fake Lights (Particle System)"
{
	Properties
	{
		[Header(Requires custom vertex streams.)]
        [Header(See shader file for more details.)]
        [Space]
        [HeaderEx(Main Settings)]
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0) 
		_Intensity ("Intensity", Float) = 1.0
        [Space]
        [Toggle(BLOOM)]_NoLight("Disable Fake Light", Float) = 0.0
        [Toggle(FINALPASS)]_NoFog("Disable Volumetric Fog", Float) = 0.0
        [Space]
        _LightPow("Light Strength", Range(0, 1)) = 1
        _FogPow("Fog Strength", Range(0, 1)) = 1
		[Space]
        _MinimumSize("Light Volumetric Radius", Range(0.01, 1)) = 0.1
        _LightScale("Light Scale", Range(0, 1)) = 0.75
        _FogScale("Fog Scale", Range(0, 1)) = 0.75
        _EdgeFalloff("Edge Falloff Softness", Range(0, 10)) = 1
        [Space]
        _VanishingStart("Camera Fade In Start (m)", Float) = 1000
        _VanishingEnd("Camera Fade In End (m)", Float) = 100
		[Space]
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2

        [HeaderEx(Blend Mode)]
        [Enum(UnityEngine.Rendering.BlendMode)]
        _ParticleSrcBlend("Src Factor", Float) = 1  // One
        [Enum(UnityEngine.Rendering.BlendMode)]
        _ParticleDstBlend("Dst Factor", Float) = 10 // OneMinusSrcAlpha
        [Enum(Off, 0, To Fog Colour, 1, To Zero, 1)]_ApplyFog ("Apply Fog", Float) = 0
        [ToggleUI]_PremultiplyAlpha("Premultiply Alpha", Float) = 0
    
        [HeaderEx(Stencil)]
        [Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 15 
        [IntRange] _Stencil ("Stencil ID [0;255]", Range(0,255)) = 0
        [IntRange] _ReadMask ("ReadMask [0;255]", Range(0,255)) = 255
        [IntRange] _WriteMask ("WriteMask [0;255]", Range(0,255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
        [HideInInspector]_RenderQueueOverride("Render Queue Override", Range(-1.0, 5000)) = -1
        [HideInInspector][Enum(RenderingMode)] _Mode("Rendering Mode", Float) = 0                                     // "Opaque"
        [HideInInspector][Enum(CustomRenderingMode)] _CustomMode("Mode", Float) = 0                                   // "Opaque"
	}
	Subshader
	{
		Tags { "Queue"="Transparent-196" "ForceNoShadowCasting"="True" "IgnoreProjector"="True" "DisableBatching"="True" }
		LOD 100
		Cull[_CullMode]
        Blend [_ParticleSrcBlend] [_ParticleDstBlend]
        ColorMask [_ColorMask]
		ZWrite Off ZTest Always
		Lighting Off
		SeparateSpecular Off

        Stencil
        {
            Ref [_Stencil]
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }
		
		CGINCLUDE
		#define PARTICLE_SHADER

        // Fake Light settings
        #define LIGHT_COLOUR _Color*_Intensity
        #define FALLOFF_POWER _EdgeFalloff

		#pragma shader_feature BLOOM
        #pragma shader_feature FINALPASS

CBUFFER_START(UnityPerMaterial)
		uniform float4 _Color;
		uniform float _Intensity;
		uniform float _MinimumSize;
		uniform float _EdgeFalloff;
		uniform float _LightScale;
        uniform float _FogScale;

        uniform fixed _LightPow;
        uniform fixed _FogPow;

        uniform fixed _ApplyFog;
        uniform fixed _PremultiplyAlpha;

        uniform float _VanishingStart;
        uniform float _VanishingEnd;
CBUFFER_END
		
		ENDCG

		Pass
		{
			CGPROGRAM
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			#pragma target 5.0
            #pragma multi_compile_instancing
            #pragma multi_compile_particles
			#pragma multi_compile_fog    
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma instancing_options assumeuniformscaling 
        	#pragma instancing_options procedural:vertInstancingSetup

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

			#include "UnityCG.cginc"

			#include "FakeLight.cginc"

			fixed4 pixel_shader(v2f ps ) : SV_TARGET
			{
                fixed3 viewDirection = normalize(ps.world_vertex-_WorldSpaceCameraPos.xyz);

				fixed3 baseWorldPos = ps.params.xyz;

                float z = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(ps.projPos));

                // Only tested on setup with reversed Z buffer
                #if UNITY_REVERSED_Z
                    if (z == 0)
                #else
                    if (z == 1)
                #endif
                z = 0;

                float sceneDepth = CorrectedLinearEyeDepth (z, (ps.ray.w/ps.projPos.w));
                float3 depthPosition = sceneDepth * ps.ray / ps.projPos.z + _WorldSpaceCameraPos;

                float finalLight = 0; 

                #if !defined(BLOOM)
                finalLight += renderFakeLight(baseWorldPos, viewDirection, depthPosition, ps.params.w * _LightScale
                    , _MinimumSize) * _LightPow;
                #endif
                #if !defined(FINALPASS)
                finalLight += renderVolumetricSphere(baseWorldPos, viewDirection, depthPosition, ps.params.w * _FogScale
                    , _MinimumSize) * _FogPow;
                #endif

                float vanishFac = getVanishingFactor(_VanishingStart, _VanishingEnd, 
                    ps.world_vertex.w);

				float4 finalColor = ps.color * finalLight;

                // Call Unity fog functions directly to apply fog properly.
                // Calculate fog factor from depth.
                #if 1
                    UNITY_CALC_FOG_FACTOR_RAW(sceneDepth); // out: unityFogFactor
                    float4 fogColour = unity_FogColor * saturate(2-_ApplyFog); 
                    if (_ApplyFog>0) UNITY_FOG_LERP_COLOR(finalColor,float3(0,0,0),fogColour);
                #endif

                finalColor.a = saturate(finalColor.a*vanishFac);
                finalColor *= lerp(1, finalLight, _PremultiplyAlpha);

				return max(finalColor, 0);
			}
			ENDCG
		}
	}
CustomEditor "SilentFakeLights.Unity.Inspector"
}
