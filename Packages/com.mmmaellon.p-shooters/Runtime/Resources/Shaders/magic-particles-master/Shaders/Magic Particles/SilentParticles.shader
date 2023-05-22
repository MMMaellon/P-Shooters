Shader "Silent/Magic Particles" {
Properties {
    _TintColor ("Tint Color", Color) = (1,1,1,1)
    _HDRIntensity("Intensity", Float) = 2.0
    _MainTex ("Main Texture", 2D) = "white" {}
    _TimePow ("Scroll Power", Vector) = (0, 0, 0, 0)
    [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 0

    [HeaderEx(Blend Mode)]
    [Enum(UnityEngine.Rendering.BlendMode)]
    _ParticleSrcBlend("Src Factor", Float) = 1  // One
    [Enum(UnityEngine.Rendering.BlendMode)]
    _ParticleDstBlend("Dst Factor", Float) = 10 // OneMinusSrcAlpha
    [ToggleUI]_MultiplyAlpha ("Multiply by Alpha", Float) = 1

    [HeaderEx(Visual)]

    [ToggleUI]_ParticleCutoffEnable("Enable Cutoff", Float) = 0
    _ParticleCutoff("Cutoff", Range(0, 1)) = 0
    _ParticleCutoffSoftness("Cutoff Softness", Range(0, 1)) = 1
    [Space(20)]

    [Enum(Off, 0, To Fog Colour, 1, To Black, 1)]_ApplyFog ("Apply Fog", Float) = 0
    [Space(20)]

    [ToggleUI]_SoftParticles ("Use Soft Particle", Float) = 0
    _InvFade ("Soft Particles Sharpness", Range(0.001,3.0)) = 1.0

    [Space(20)]
    _VanishingStart("Camera Fade Start", Float) = 0.0
    _VanishingEnd("Camera Fade End", Float) = 0.1

    [Space(20)]
    [Toggle(_METALLICGLOSSMAP)]_UseBicubic ("Use Bicubic Filtering", Float) = 0.0

    [Space(20)]
    _ViewOffset ("View Offset", Range(0, 1)) = 0

    [HeaderEx(Gradient Map)]
    [Toggle(_REQUIRE_UV2)]_UseGradient("Use Gradient Maps", Float) = 0
    [ToggleUI]_UseRampAlpha("Use Ramp Alpha", Float) = 0
    [NoScaleOffset]_Ramp ("Gradient Texture", 2D) = "white" {}

    [HeaderEx(Second Layer)]
    [Toggle(_DETAIL_MULX2)]_Detail ("Use Second Multiply Layer", Float) = 0
    _DetailTex ("Detail Texture", 2D) = "white" {}
    _DetailTimePow ("Detail Scroll and Power", Vector) = (0, 0, 0, 0)
    
    [HeaderEx(Distortion)]
    [Toggle(_NORMALMAP)]_UseDistortion("Use Distortion", Float) = 0
    [Normal]_WarpTex ("Distortion", 2D) = "bump" {}
    _WarpPow ("Warp Power", Vector) = (1, 1, 1, 1)

    [HeaderEx(Custom Parameter for Particle System)]
    [ToggleUI]_UseCustom ("Use Custom Vertex Streams (TEXCOORD1)", Float) = 0
    [Enum(X, 0, Y, 1, Z, 2, W, 3)]
    _CustomWarpPow("Warp Power", Float) = 0
    [Enum(X, 0, Y, 1, Z, 2, W, 3)]
    _CustomGradient("Gradient Y Position", Float) = 1
    [Enum(X, 0, Y, 1, Z, 2, W, 3)]
    _CustomGradientAlpha("Gradient Alpha Y Position", Float) = 2

    [HeaderEx(Advanced)]    
    _VisDistance ("Visibility Range", Float) = 0
    [Enum(Off, 0, Far, 1, Near, 2)] _ZEdge ("Render at Clip Plane", Float) = 0
    [Space(20)]
    [Enum(Off,0,On,1)] _ParticleZWrite("ZWrite", Int) = 0
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
    _Offset("Depth Offset", Vector) = (0, 0, 0, 0)
    [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Float) = 0                 // "Add"
    [Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 15 

    [HeaderEx(Stencil)]
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

Category {
    Tags { "Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend [_ParticleSrcBlend] [_ParticleDstBlend]
    ColorMask [_ColorMask]
    ZTest [_ZTest]
    ZWrite [_ParticleZWrite]

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
    Cull [_CullMode]
    Offset[_Offset.x], [_Offset.y]

    SubShader {
        Pass {

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 5.0

#pragma multi_compile_particles
#pragma multi_compile_fog
#pragma multi_compile _ LOD_FADE_CROSSFADE

#pragma shader_feature _ _NORMALMAP
#pragma shader_feature _ _DETAIL_MULX2
#pragma shader_feature _ _METALLICGLOSSMAP
#pragma shader_feature _ _PARALLAXMAP
#pragma shader_feature _ _REQUIRE_UV2
#pragma shader_feature _ _SUNDISK_HIGH_QUALITY _SUNDISK_SIMPLE _SUNDISK_NONE

#define _BICUBIC defined(_METALLICGLOSSMAP)
#define _GRADIENT defined(_REQUIRE_UV2)

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"

#if !(defined(SHADER_STAGE_VERTEX) || defined(SHADER_STAGE_FRAGMENT) || defined(SHADER_STAGE_DOMAIN) || defined(SHADER_STAGE_HULL) || defined(SHADER_STAGE_GEOMETRY))
#define centroid 
#endif


struct appdata_t {
    float4 vertex : POSITION;
    centroid fixed4 color : COLOR;
    float4 texcoord : TEXCOORD0;
    float4 custom : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 vertex : SV_POSITION;
    centroid fixed4 color : COLOR;
    float4 texcoord : TEXCOORD0;
    float4 custom : TEXCOORD1;
    UNITY_FOG_COORDS(3)
    #ifdef SOFTPARTICLES_ON
    float4 projPos : TEXCOORD2;
    #endif
    UNITY_VERTEX_OUTPUT_STEREO
};

#ifdef SOFTPARTICLES_ON
float _SoftParticles;
#endif

sampler2D _MainTex;
float4 _MainTex_ST; float4 _MainTex_TexelSize;

#if defined(_DETAIL_MULX2)
sampler2D _DetailTex;
float4 _DetailTex_ST; float4 _DetailTex_TexelSize;
#endif

#if defined(_NORMALMAP)
sampler2D _WarpTex;
float4 _WarpTex_ST; float4 _WarpTex_TexelSize;
#endif

#if _GRADIENT
sampler2D _Ramp;
float _UseRampAlpha;
#endif

fixed4 _TintColor;
float4 _TimePow;
float4 _DetailTimePow;
float4 _WarpPow;
float _ZEdge;
float _VisDistance;
float _ApplyFog;
float _HDRIntensity;
float _ParticleCutoff;
float _ParticleCutoffSoftness;
float _ViewOffset;
float _VanishingStart;
float _VanishingEnd;
float _UseCustom;
float _CustomGradient;
float _CustomGradientAlpha;
float _CustomWarpPow;
float _MultiplyAlpha;
float _ParticleCutoffEnable;


float lerpstep( float a, float b, float t)
{
    return saturate( ( t - a ) / ( b - a ) );
}

float smootherstep(float a, float b, float t) 
{
    t = saturate( ( t - a ) / ( b - a ) );
    return t * t * t * (t * (t * 6. - 15.) + 10.);
}

void applyVanishing (inout float alpha, float closeDist) {
    // Add near clip plane to start/end so that (0, 0.1) looks right
    _VanishingStart += _ProjectionParams.y;
    _VanishingEnd += _ProjectionParams.y;
    float vanishing = saturate(smootherstep(_VanishingStart, _VanishingEnd, closeDist));
    alpha = alpha * vanishing;
}

v2f vert (appdata_t v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float4 worldPos = mul(v.vertex, unity_ObjectToWorld);
    float distanceObjToCam = distance(_WorldSpaceCameraPos, worldPos);
    v.vertex.xyz += _ViewOffset * normalize(ObjSpaceViewDir(v.vertex.xyzz)) * saturate(distanceObjToCam);

    o.vertex = UnityObjectToClipPos(v.vertex);

    if (_ZEdge==1) {
        #if defined(UNITY_REVERSED_Z)
        // when using reversed-Z, make the Z be just a tiny
        // bit above 0.0
        //o.vertex.z = 1.0e-9f;
        o.vertex.z = 1.0e-8f;
        #else
        // when not using reversed-Z, make Z/W be just a tiny
        // bit below 1.0
        //o.vertex.z = o.vertex.w - 1.0e-6f;
        o.vertex.z = o.vertex.w - 1.0e-5f;
        #endif
    }
    if (_ZEdge==2) {
        #if !defined(UNITY_REVERSED_Z)
        // when using reversed-Z, make the Z be just a tiny
        // bit above 0.0
        //o.vertex.z = 1.0e-9f;
        o.vertex.z = 1.0e-8f;
        #else
        // when not using reversed-Z, make Z/W be just a tiny
        // bit below 1.0
        //o.vertex.z = o.vertex.w - 1.0e-6f;
        o.vertex.z = o.vertex.w - 1.0e-5f;
        #endif
    }
    #ifdef SOFTPARTICLES_ON
    o.projPos = ComputeScreenPos (o.vertex);
    COMPUTE_EYEDEPTH(o.projPos.z);
    #endif
    o.color = v.color * _TintColor;
    o.color.rgb *= _HDRIntensity;

    if (_VisDistance) {
        fixed3 baseWorldPos = unity_ObjectToWorld._m03_m13_m23;
        const float scale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
        float closeDist = distance(_WorldSpaceCameraPos, baseWorldPos);
        o.color *= saturate((_VisDistance*scale)-closeDist);
    }
    o.texcoord = float4(TRANSFORM_TEX(v.texcoord,_MainTex)+(_Time.x*_TimePow.xy+_TimePow.zw), v.texcoord.xy);

    // Custom is hardcoded in PS to be 
    // X: warp power         | y: gradient position
    // Z: gradient alpha pos | z: none
    o.custom = float4(1.0, 0.0, 0.0, 0.0);
    if (_UseCustom)
    {
        o.custom.x = v.custom[_CustomWarpPow];
        o.custom.y = v.custom[_CustomGradient];
        o.custom.z = v.custom[_CustomGradientAlpha];
    };

    UNITY_TRANSFER_FOG(o,o.vertex);
    return o;
}

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
float _InvFade;

float4 cubic(float v)
{
    float4 n = float4(1.0, 2.0, 3.0, 4.0) - v;
    float4 s = n * n * n;
    float x = s.x;
    float y = s.y - 4.0 * s.x;
    float z = s.z - 4.0 * s.y + 6.0 * s.x;
    float w = 6.0 - x - y - z;
    return float4(x, y, z, w);
}

float4 bicubicFilter(sampler2D inTex, float2 texcoord, float4 texscale)
{
    #if _BICUBIC
    texcoord *= texscale.zw;
    float fx = frac(texcoord.x);
    float fy = frac(texcoord.y);
    texcoord.x -= fx;
    texcoord.y -= fy;

    float4 xcubic = cubic(fx);
    float4 ycubic = cubic(fy);

    float4 c = float4(texcoord.x - 0.5, texcoord.x + 1.5, texcoord.y - 0.5, texcoord.y + 1.5);
    float4 s = float4(xcubic.x + xcubic.y, xcubic.z + xcubic.w, ycubic.x + ycubic.y, ycubic.z + ycubic.w);
    float4 offset = c + float4(xcubic.y, xcubic.w, ycubic.y, ycubic.w) / s;

    float4 sample0 = tex2D(inTex, float2(offset.x, offset.z) * texscale.xy);
    float4 sample1 = tex2D(inTex, float2(offset.y, offset.z) * texscale.xy);
    float4 sample2 = tex2D(inTex, float2(offset.x, offset.w) * texscale.xy);
    float4 sample3 = tex2D(inTex, float2(offset.y, offset.w) * texscale.xy);

    float sx = s.x / (s.x + s.y);
    float sy = s.z / (s.z + s.w);

    return lerp(
        lerp(sample3, sample2, sx),
        lerp(sample1, sample0, sx), sy);
    #else
    return tex2D(inTex, texcoord);
    #endif
}

void farDepthReverseFix(inout float bgDepth)
{
    #if UNITY_REVERSED_Z
        if (bgDepth == 0)
    #else
        if (bgDepth == 1)
    #endif
        bgDepth = 0.0;
}


fixed4 frag (v2f i) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(i);


    #ifdef SOFTPARTICLES_ON
    if (_SoftParticles)
    {
    float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
    farDepthReverseFix(sceneZ);
    float partZ = i.projPos.z;
    float fade = saturate (_InvFade * (sceneZ-partZ));
    i.color.a *= fade;
    }
    #endif

    #ifdef LOD_FADE_CROSSFADE
    i.color.a = i.color.a * unity_LODFade.x;
    #endif

    #ifdef SOFTPARTICLES_ON
    applyVanishing(i.color.a, i.projPos.z);
    #endif

    float2 warpUVs = 0;
    #if defined(_NORMALMAP)
        // Normal scale is just multiplication
        warpUVs = UnpackNormal(tex2D(_WarpTex, TRANSFORM_TEX(i.texcoord.zw,_WarpTex)+frac(_Time.x*_WarpPow.xy))) * _WarpPow.zw * i.custom.x;
    #endif

    float2 uvs = i.texcoord + warpUVs;
    fixed4 col = bicubicFilter(_MainTex, uvs, _MainTex_TexelSize);

    #if defined(_DETAIL_MULX2)
    float2 detUVs = TRANSFORM_TEX(i.texcoord.zw,_DetailTex)+frac(_Time.x*_DetailTimePow.xy) + warpUVs;
    col.rgb *= LerpWhiteTo(bicubicFilter(_DetailTex, detUVs, _DetailTex_TexelSize)*_DetailTimePow.w, _DetailTimePow.z);
    #endif

    #if _GRADIENT
    col.rgb = tex2D(_Ramp, float2(dot(col.rgb, 1.0/3.0), i.custom.y));
    col.a = lerp(col.a, tex2D(_Ramp, float2(col.a, i.custom.z)).a, _UseRampAlpha);
    #endif

    float cutoff = saturate(_ParticleCutoff + (1 - i.color.a));
    cutoff += 1.0/255.0;
    col.a = lerp(col.a*i.color.a, 
        lerpstep(cutoff, cutoff + _ParticleCutoffSoftness, col.a), 
        _ParticleCutoffEnable);

    col.rgb *= i.color.rgb;

    if (_MultiplyAlpha) col.rgb *= col.a;

    // Maybe this is over-optimisation... 
    // If fog is 2, then set fog to black. 
    float4 fogColour = unity_FogColor * saturate(2-_ApplyFog); 
    if (_ApplyFog>0) 
    {
        UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fogColour); 
    }

    // Don't allow alpha to go higher than 1.0.
    col.a = saturate(col.a);
    return col;
}

            ENDCG
        }
    }
}
CustomEditor "SilentMagicParticles.Unity.Inspector"
}
