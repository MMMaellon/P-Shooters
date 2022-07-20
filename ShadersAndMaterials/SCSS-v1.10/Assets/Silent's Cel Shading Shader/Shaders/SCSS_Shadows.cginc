#ifndef SCSS_SHADOWS_INCLUDED
#define SCSS_SHADOWS_INCLUDED

#pragma multi_compile_shadowcaster
#pragma fragmentoption ARB_precision_hint_fastest

#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "SCSS_Utils.cginc"
#include "SCSS_Input.cginc"

#if (defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)) && defined(UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS)
    #define UNITY_STANDARD_USE_DITHER_MASK 1
#endif

// Need to output UVs in shadow caster, since we need to sample texture and do clip/dithering based on it
#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
    #define UNITY_STANDARD_USE_SHADOW_UVS 1
#endif

// Has a non-empty shadow caster output struct (it's an error to have empty structs on some platforms...)
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
    #define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif

#ifdef UNITY_STEREO_INSTANCING_ENABLED
    #define UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT 1
#endif

//uniform float4      _Color;
//uniform float       _Cutoff;
//uniform sampler2D   _MainTex;
//uniform sampler2D   _ClippingMask;
//uniform float4      _MainTex_ST;
//uniform float       _AlbedoAlphaMode;
//uniform float       _VanishingStart;
//uniform float       _VanishingEnd;
//uniform float       _UseVanishing;
//uniform float       _AlphaSharp;
//uniform float       _Tweak_Transparency;

#if defined(_SPECULAR)
/*
half SpecularSetup_ShadowGetOneMinusReflectivity(half2 uv)
{
    half3 specColor = _SpecColor.rgb;
    #ifdef _SPECGLOSSMAP
        specColor = tex2D(_SpecGlossMap, uv).rgb;
    #endif
    return (1 - SpecularStrength(specColor));
}
*/
#endif


struct VertexInput
{
    float4 vertex   : POSITION;
    float3 normal   : NORMAL;
    float2 uv0      : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
struct VertexOutputShadowCaster
{
    V2F_SHADOW_CASTER_NOPOS
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        float2 tex : TEXCOORD1;
    #endif
};
#endif

#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
struct VertexOutputStereoShadowCaster
{
    UNITY_VERTEX_OUTPUT_STEREO
};
#endif

float4 TexCoordsShadowCaster(float2 texcoords)
{
    float4 texcoord;
    texcoord.xy = TRANSFORM_TEX(texcoords, _MainTex);// Always source from uv0
    texcoord.xy = _PixelSampleMode? 
        sharpSample(_MainTex_TexelSize * _MainTex_ST.xyxy, texcoord.xy) : texcoord.xy;

    return texcoord;
}

// We have to do these dances of outputting SV_POSITION separately from the vertex shader,
// and inputting VPOS in the pixel shader, since they both map to "POSITION" semantic on
// some platforms, and then things don't go well.

void vertShadowCaster (VertexInput v
    , out float4 opos : SV_POSITION
    #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    , out VertexOutputShadowCaster o
    #endif
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
    , out VertexOutputStereoShadowCaster os
    #endif
)
{
    UNITY_SETUP_INSTANCE_ID(v);

    // Vertex modifications go here.
    
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
    #endif
    TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
    //TRANSFER_SHADOW_CASTER_NOPOS_LEGACY (o, opos)

    // Standard would apply texcoords here, but we need to apply them in fragment
    // due to pixel sampling mode options.

    // Simple inventory.
    float inventoryMask = getInventoryMask(v.uv0);

    // Apply the inventory mask.
    // Set the output variables based on the mask to completely remove it.
    // - Set the clip-space position to one that won't be rendered
    // - Set the vertex alpha to zero
    // - Disable outlines
    if (_UseInventory)
    {
        opos.z =     inventoryMask ? opos.z : 1e+9;
        //o.vertex =    inventoryMask ? o.vertex : 1e+9;
    }

    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.tex = AnimateTexcoords(v.uv0);
    #endif
}

half4 fragShadowCaster (UNITY_POSITION(vpos)
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    , VertexOutputShadowCaster i
#endif
) : SV_Target
{
    half alpha = Alpha(0);
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        float4 texcoords = TexCoordsShadowCaster(i.tex);
        fixed3 albedo = Albedo(texcoords);
        alpha = Alpha(texcoords);
    #endif // #if defined(UNITY_STANDARD_USE_SHADOW_UVS)

    #if defined(ALPHAFUNCTION)
    alphaFunction(alpha);
    #endif

    applyVanishing(alpha);

    /* To-do
    #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
        #if defined(_ALPHAPREMULTIPLY_ON)
            half outModifiedAlpha;
            PreMultiplyAlpha(half3(0, 0, 0), alpha, SpecularSetup_ShadowGetOneMinusReflectivity(i.tex), outModifiedAlpha);
            alpha = outModifiedAlpha;
        #endif
    #endif
    */
    clip(alpha);

    applyAlphaClip(alpha, _Cutoff, vpos.xy, _AlphaSharp);

    SHADOW_CASTER_FRAGMENT(i) 
}

#endif