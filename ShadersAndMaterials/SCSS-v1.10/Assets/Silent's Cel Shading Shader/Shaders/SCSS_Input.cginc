#ifndef SCSS_INPUT_INCLUDED
#define SCSS_INPUT_INCLUDED

//---------------------------------------

// Keyword squeezing. 

#if (defined(_DETAIL_MULX2) || defined(_DETAIL_MUL) || defined(_DETAIL_ADD) || defined(_DETAIL_LERP))
    #define _DETAIL
#endif

#if (defined(_METALLICGLOSSMAP) || defined(_SPECGLOSSMAP))
	#define _SPECULAR
#endif

#if (defined(_SUNDISK_NONE))
	#define _SUBSURFACE
#endif

//---------------------------------------

#if defined(_EMISSION)
#include "SCSS_AudioLink.cginc"
#endif

UNITY_DECLARE_TEX2D(_MainTex); uniform half4 _MainTex_ST; uniform half4 _MainTex_TexelSize;
UNITY_DECLARE_TEX2D_NOSAMPLER(_ColorMask); uniform half4 _ColorMask_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap); uniform half4 _BumpMap_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap); uniform half4 _EmissionMap_ST;

// Workaround for shadow compiler error. 
#if defined(SCSS_SHADOWS_INCLUDED)
UNITY_DECLARE_TEX2D(_ClippingMask); uniform half4 _ClippingMask_ST;
#else
UNITY_DECLARE_TEX2D_NOSAMPLER(_ClippingMask); uniform half4 _ClippingMask_ST;
#endif

#if defined(_DETAIL)
UNITY_DECLARE_TEX2D(_DetailAlbedoMap); uniform half4 _DetailAlbedoMap_ST; uniform half4 _DetailAlbedoMap_TexelSize;
UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); uniform half4 _DetailNormalMap_ST;
UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularDetailMask); uniform half4 _SpecularDetailMask_ST;
uniform float _DetailAlbedoMapScale;
uniform float _DetailNormalMapScale;
uniform float _SpecularDetailStrength;
#endif

#if defined(_EMISSION)
uniform float _EmissionDetailType;
uniform float _DetailEmissionUVSec;
UNITY_DECLARE_TEX2D(_DetailEmissionMap); uniform half4 _DetailEmissionMap_ST; uniform half4 _DetailEmissionMap_TexelSize;
uniform float4 _EmissionDetailParams;
uniform float _UseEmissiveLightSense;
uniform float _EmissiveLightSenseStart;
uniform float _EmissiveLightSenseEnd;
#endif

#if defined(_SPECULAR)
//uniform float4 _SpecColor; // Defined elsewhere
UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecGlossMap); uniform half4 _SpecGlossMap_ST;
uniform float _UseMetallic;
uniform float _SpecularType;
uniform float _Smoothness;
uniform float _UseEnergyConservation;
uniform float _Anisotropy;
uniform float _CelSpecularSoftness;
uniform float _CelSpecularSteps;
#else
// Default to zero
uniform float _SpecularType;
uniform float _UseEnergyConservation;
uniform float _Anisotropy; // Can not be removed yet.
#endif

#if defined(SCSS_CROSSTONE)
UNITY_DECLARE_TEX2D_NOSAMPLER(_1st_ShadeMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_2nd_ShadeMap);
UNITY_DECLARE_TEX2D_NOSAMPLER(_ShadingGradeMap);
#endif

#if !defined(SCSS_CROSSTONE)
UNITY_DECLARE_TEX2D_NOSAMPLER(_ShadowMask); uniform half4 _ShadowMask_ST;
uniform sampler2D _Ramp; uniform half4 _Ramp_ST;
uniform float _LightRampType;
uniform float4 _ShadowMaskColor;
uniform float _ShadowMaskType;
uniform float _IndirectLightingBoost;
uniform float _Shadow;
uniform float _ShadowLift;
#endif

uniform float4 _Color;
uniform float _BumpScale;
uniform float _Cutoff;
uniform float _AlphaSharp;
uniform float _UVSec;
uniform float _AlbedoAlphaMode;
uniform float _Tweak_Transparency;

uniform float4 _EmissionColor;

uniform float _UseFresnel;
uniform float _UseFresnelLightMask;
uniform float4 _FresnelTint;
uniform float _FresnelWidth;
uniform float _FresnelStrength;
uniform float _FresnelLightMask;
uniform float4 _FresnelTintInv;
uniform float _FresnelWidthInv;
uniform float _FresnelStrengthInv;

uniform float4 _CustomFresnelColor;

#if defined(SCSS_OUTLINE)
uniform float _OutlineMode;
uniform float _OutlineZPush;
uniform float _outline_width;
uniform float4 _outline_color;
#endif

uniform float _LightingCalculationType;


UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapMask); uniform half4 _MatcapMask_ST; 
uniform sampler2D _Matcap1; uniform half4 _Matcap1_ST; 
uniform sampler2D _Matcap2; uniform half4 _Matcap2_ST; 
uniform sampler2D _Matcap3; uniform half4 _Matcap3_ST; 
uniform sampler2D _Matcap4; uniform half4 _Matcap4_ST; 

uniform float _UseMatcap;
uniform float _Matcap1Strength;
uniform float _Matcap2Strength;
uniform float _Matcap3Strength;
uniform float _Matcap4Strength;
uniform float _Matcap1Blend;
uniform float _Matcap2Blend;
uniform float _Matcap3Blend;
uniform float _Matcap4Blend;
uniform float4 _Matcap1Tint;
uniform float4 _Matcap2Tint;
uniform float4 _Matcap3Tint;
uniform float4 _Matcap4Tint;

#if defined(_SUBSURFACE)
UNITY_DECLARE_TEX2D_NOSAMPLER(_ThicknessMap); uniform half4 _ThicknessMap_ST;
uniform float _UseSubsurfaceScattering;
uniform float _ThicknessMapPower;
uniform float _ThicknessMapInvert;
uniform float3 _SSSCol;
uniform float _SSSIntensity;
uniform float _SSSPow;
uniform float _SSSDist;
uniform float _SSSAmbient;
#endif

uniform float4 _LightSkew;
uniform float _PixelSampleMode;
uniform float _VertexColorType;

// CrossTone
uniform float4 _1st_ShadeColor;
uniform float4 _2nd_ShadeColor;
uniform float _1st_ShadeColor_Step;
uniform float _1st_ShadeColor_Feather;
uniform float _2nd_ShadeColor_Step;
uniform float _2nd_ShadeColor_Feather;

uniform float _Tweak_ShadingGradeMapLevel;

uniform float _DiffuseGeomShadowFactor;
uniform float _LightWrappingCompensationFactor;

uniform float _IndirectShadingType;
uniform float _CrosstoneToneSeparation;
uniform float _Crosstone2ndSeparation;

uniform float _UseInteriorOutline;
uniform float _InteriorOutlineWidth;

uniform sampler2D _OutlineMask; uniform half4 _OutlineMask_ST; 

// Animation
uniform float _UseAnimation;
uniform float _AnimationSpeed;
uniform int _TotalFrames;
uniform int _FrameNumber;
uniform int _Columns;
uniform int _Rows;

// Vanishing
uniform float _UseVanishing;
uniform float _VanishingStart;
uniform float _VanishingEnd;

// Inventory 
uniform fixed _UseInventory;
uniform float _InventoryStride;
uniform fixed _InventoryItem01Animated;
uniform fixed _InventoryItem02Animated;
uniform fixed _InventoryItem03Animated;
uniform fixed _InventoryItem04Animated;
uniform fixed _InventoryItem05Animated;
uniform fixed _InventoryItem06Animated;
uniform fixed _InventoryItem07Animated;
uniform fixed _InventoryItem08Animated;
uniform fixed _InventoryItem09Animated;
uniform fixed _InventoryItem10Animated;
uniform fixed _InventoryItem11Animated;
uniform fixed _InventoryItem12Animated;
uniform fixed _InventoryItem13Animated;
uniform fixed _InventoryItem14Animated;
uniform fixed _InventoryItem15Animated;
uniform fixed _InventoryItem16Animated;

// Light adjustment
uniform float _LightMultiplyAnimated;
uniform float _LightClampAnimated;

//-------------------------------------------------------------------------------------
// Input functions

struct SCSS_RimLightInput
{
	half width;
	half power;
	half3 tint;
	half alpha;

	half invWidth;
	half invPower;
	half3 invTint;
	half invAlpha;
};

// Contains tonemap colour and shade offset.
struct SCSS_TonemapInput
{
	half3 col; 
	half bias;
};

struct SCSS_Input 
{
	half3 albedo;
	half alpha;
	float3 normal;

	half occlusion;

	half3 specColor;
	float oneMinusReflectivity, smoothness, perceptualRoughness;
	half softness;
	half3 emission;

	half3 thickness;

	SCSS_RimLightInput rim;
	SCSS_TonemapInput tone[2];
};

void initMaterial(out SCSS_Input material)
{
	material = (SCSS_Input) 0;
	material.albedo = 1.0;
	material.alpha = 1.0;
	material.normal = float3(0.0, 0.0, 1.0);
	material.occlusion = 1.0;
	material.specColor = 0.0;
	material.oneMinusReflectivity = 1.0;
	material.smoothness = 0.0;
	material.perceptualRoughness = 1.0;
	material.softness = 0.0;
	material.emission = 0.0;
	material.thickness = 1.0;

	SCSS_RimLightInput rim = (SCSS_RimLightInput) 0;
	rim.width = 0.0;
	rim.power = 0.0;
	rim.tint = 0.0;
	rim.alpha = 0.0;
	rim.invWidth = 0.0;
	rim.invPower = 0.0;
	rim.invTint = 0.0;
	rim.invAlpha = 0.0;

	material.rim = rim;

	material.tone[0].col = 1.0;
	material.tone[0].bias = 1.0;
	material.tone[1].col = 1.0;
	material.tone[1].bias = 1.0;
}

struct SCSS_LightParam
{
	half3 viewDir, halfDir, reflDir;
	half2 rlPow4;
	half NdotL, NdotV, LdotH, NdotH;
	half NdotAmb;
};

#if defined(UNITY_STANDARD_BRDF_INCLUDED)
bool inMirror()
{
	return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
}

// Only needed in Unity versions before Unity 2017.4.28 or so.
// However, 2017.4.15 is a higher UNITY_VERSION.
bool backfaceInMirror()
{
	#if ( (UNITY_VERSION <= 201711) || (UNITY_VERSION == 201755) )
	return inMirror();
	#else
	return false;
	#endif
}

bool getLightClampActive()
{
	#if !UNITY_HDR_ON && SCSS_CLAMP_IN_NON_HDR
	return true;
	#endif
	#if SCSS_NO_CLAMPING
	return false;
	#endif
	return (_LightClampAnimated == 1.0);
}

struct SCSS_Light
{
    half3 color;
    half3 dir;
    half  intensity; 
};


SCSS_Light MainLight()
{
    SCSS_Light l;

    l.color = _LightColor0.rgb;
    l.intensity = _LightColor0.w;
    l.dir = Unity_SafeNormalize(_WorldSpaceLightPos0.xyz); 

    // Workaround for scenes with HDR off blowing out in VRchat.
    if (getLightClampActive())
        l.color = saturate(l.color);

    return l;
}

SCSS_Light MainLight(float3 worldPos)
{
    SCSS_Light l = MainLight();
    l.dir = Unity_SafeNormalize(UnityWorldSpaceLightDir(worldPos)); 
    return l;
}

float getAmbientLight (float3 normal)
{
	float3 ambientLightDirection = Unity_SafeNormalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * _LightSkew.xyz);

	if (_IndirectShadingType == 2) // Flatten
	{
		ambientLightDirection = any(_LightColor0) 
		? normalize(_WorldSpaceLightPos0) 
		: ambientLightDirection;
	}

	float ambientLight = dot(normal, ambientLightDirection);
	ambientLight = ambientLight * 0.5 + 0.5;

	if (_IndirectShadingType == 0) // Dynamic
		ambientLight = getGreyscaleSH(normal);
	return ambientLight;
}

SCSS_LightParam initialiseLightParam (SCSS_Light l, float3 normal, float3 posWorld)
{
	SCSS_LightParam d = (SCSS_LightParam) 0;
	d.viewDir = normalize(_WorldSpaceCameraPos.xyz - posWorld.xyz);
	d.halfDir = Unity_SafeNormalize (l.dir + d.viewDir);
	d.reflDir = reflect(-d.viewDir, normal); // Calculate reflection vector
	d.NdotL = (dot(l.dir, normal)); // Calculate NdotL
	d.NdotV = (dot(d.viewDir,  normal)); // Calculate NdotV
	d.LdotH = (dot(l.dir, d.halfDir));
	d.NdotH = (dot(normal, d.halfDir)); // Saturate seems to cause artifacts
	d.rlPow4 = Pow4(float2(dot(d.reflDir, l.dir), 1 - d.NdotV));  
	d.NdotAmb = getAmbientLight(normal);
	return d;
}
#endif

// Allows saturate to be called on light params. 
// Does not affect directions. Those are already normalized.
// Only the required saturations will be left in code.
SCSS_LightParam saturate (SCSS_LightParam d)
{
	d.NdotL = saturate(d.NdotL);
	d.NdotV = saturate(d.NdotV);
	d.LdotH = saturate(d.LdotH);
	d.NdotH = saturate(d.NdotH);
	return d;
}

SCSS_RimLightInput initialiseRimParam()
{
	SCSS_RimLightInput rim = (SCSS_RimLightInput) 0;
	rim.width = _FresnelWidth;
	rim.power = _FresnelStrength;
	rim.tint = _FresnelTint.rgb;
	rim.alpha = _FresnelTint.a;

	rim.invWidth = _FresnelWidthInv;
	rim.invPower = _FresnelStrengthInv;
	rim.invTint = _FresnelTintInv.rgb;
	rim.invAlpha = _FresnelTintInv.a;
	return rim;
}

float2 AnimateTexcoords(float2 texcoord)
{
	float2 spriteUV = texcoord;
	if (_UseAnimation)
	{
		float currentFrame = _FrameNumber + frac(_Time[0] * _AnimationSpeed) * _TotalFrames;

		float frame = floor(clamp(currentFrame, 0, _TotalFrames));

		float2 offPerFrame = float2((1 / (float)_Columns), (1 / (float)_Rows));

		float2 spriteSize = texcoord * offPerFrame;

		float2 currentSprite = 
				float2(frame * offPerFrame.x,  1 - offPerFrame.y);
		
		float rowIndex;
		float mod = modf(frame / (float)_Columns, rowIndex);
		currentSprite.y -= rowIndex * offPerFrame.y;
		currentSprite.x -= rowIndex * _Columns * offPerFrame.x;
		
		spriteUV = (spriteSize + currentSprite); 
	}
	return spriteUV;

}

float4 TexCoords(float2 uv0, float2 uv1)
{
    float4 texcoord;
	texcoord.xy = TRANSFORM_TEX(uv0, _MainTex);// Always source from uv0
	texcoord.xy = _PixelSampleMode? 
		sharpSample(_MainTex_TexelSize * _MainTex_ST.xyxy, texcoord.xy) : texcoord.xy;

#if defined(_DETAIL) 
	texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? uv0 : uv1), _DetailAlbedoMap);
	texcoord.zw = _PixelSampleMode? 
		sharpSample(_DetailAlbedoMap_TexelSize * _DetailAlbedoMap_ST.xyxy, texcoord.zw) : texcoord.zw;
#else
	texcoord.zw = texcoord.xy;
#endif
    return texcoord;
}

float2 EmissionDetailTexCoords(float2 uv0, float2 uv1)
{
	float2 texcoord;
#if defined(_EMISSION) 
	texcoord.xy = TRANSFORM_TEX(((_DetailEmissionUVSec == 0) ? uv0 : uv1), _DetailEmissionMap);
	texcoord.xy = _PixelSampleMode? 
		sharpSample(_DetailEmissionMap_TexelSize * _DetailEmissionMap_ST.xyxy, texcoord.xy) : texcoord.xy;
#else
	texcoord.xy = uv0; // Default we won't need
#endif
    return texcoord;
}

#ifndef UNITY_SAMPLE_TEX2D_SAMPLER_LOD
#define UNITY_SAMPLE_TEX2D_SAMPLER_LOD(tex,samplertex,coord,lod) tex.Sample (sampler##samplertex,coord,lod)
#endif
#ifndef UNITY_SAMPLE_TEX2D_LOD
#define UNITY_SAMPLE_TEX2D_LOD(tex,coord,lod) tex.Sample (sampler##tex,coord,lod)
#endif

half OutlineMask(float2 uv)
{
	// Needs LOD, sampled in vertex function
    return tex2Dlod(_OutlineMask, float4(uv, 0, 0)).r;
}

half ColorMask(float2 uv)
{
    return UNITY_SAMPLE_TEX2D_SAMPLER (_ColorMask, _MainTex, uv).g;
}

half RimMask(float2 uv)
{
    return UNITY_SAMPLE_TEX2D_SAMPLER (_ColorMask, _MainTex, uv).b;
}

half DetailMask(float2 uv)
{
    return UNITY_SAMPLE_TEX2D_SAMPLER (_ColorMask, _MainTex, uv).a;
}

half4 MatcapMask(float2 uv)
{
    return UNITY_SAMPLE_TEX2D_SAMPLER (_MatcapMask, _MainTex, uv);
}

half3 Thickness(float2 uv)
{
#if defined(_SUBSURFACE)
	return pow(
		UNITY_SAMPLE_TEX2D_SAMPLER (_ThicknessMap, _MainTex, uv), 
		_ThicknessMapPower);
#else
	return 1;
#endif
}

half3 Albedo(float4 texcoords)
{
    half3 albedo = UNITY_SAMPLE_TEX2D (_MainTex, texcoords.xy).rgb;
    return albedo;
}

half3 Emission(float2 uv)
{
    return UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, uv*_EmissionMap_ST.xy+_EmissionMap_ST.zw).rgb;
}

half ClippingMask(float2 uv)
{
	uv = TRANSFORM_TEX(uv, _ClippingMask);
	// Workaround for shadow compiler error. 
	#if defined(SCSS_SHADOWS_INCLUDED)
	float alpha = UNITY_SAMPLE_TEX2D(_ClippingMask, uv);
	#else
	float alpha = UNITY_SAMPLE_TEX2D_SAMPLER(_ClippingMask, _MainTex, uv);
	#endif 
	return saturate(alpha + _Tweak_Transparency);
}

half Alpha(float2 uv)
{
	half alpha = _Color.a;
	switch(_AlbedoAlphaMode)
	{
		case 0: alpha *= UNITY_SAMPLE_TEX2D(_MainTex, uv).a; break;
		case 2: alpha *= ClippingMask(uv); break;
	}
	return alpha;
}

void applyVanishing (inout float alpha) {
    const fixed3 baseWorldPos = unity_ObjectToWorld._m03_m13_m23;
    float closeDist = distance(_WorldSpaceCameraPos, baseWorldPos);
    float vanishing = saturate(lerpstep(_VanishingStart, _VanishingEnd, closeDist));
    alpha = lerp(alpha, alpha * vanishing, _UseVanishing);
}

inline float getInventoryMask(float2 in_texcoord)
{
    // Initialise mask. This will cut things out.
    float inventoryMask = 0.0;
    // Which UV section are we in?
    uint itemID = floor((in_texcoord.x) / _InventoryStride);
    // If the item ID is zero or below, always render.
    // But if it's higher, check against toggles.

    inventoryMask += (itemID <= 0);
    inventoryMask += (itemID == 1) * _InventoryItem01Animated;
    inventoryMask += (itemID == 2) * _InventoryItem02Animated;
    inventoryMask += (itemID == 3) * _InventoryItem03Animated;
    inventoryMask += (itemID == 4) * _InventoryItem04Animated;
    inventoryMask += (itemID == 5) * _InventoryItem05Animated;
    inventoryMask += (itemID == 6) * _InventoryItem06Animated;
    inventoryMask += (itemID == 7) * _InventoryItem07Animated;
    inventoryMask += (itemID == 8) * _InventoryItem08Animated;
    inventoryMask += (itemID == 9) * _InventoryItem09Animated;
    inventoryMask += (itemID == 10) * _InventoryItem10Animated;
    inventoryMask += (itemID == 11) * _InventoryItem11Animated;
    inventoryMask += (itemID == 12) * _InventoryItem12Animated;
    inventoryMask += (itemID == 13) * _InventoryItem13Animated;
    inventoryMask += (itemID == 14) * _InventoryItem14Animated;
    inventoryMask += (itemID == 15) * _InventoryItem15Animated;
    inventoryMask += (itemID == 16) * _InventoryItem16Animated;

    // Higher than 17? Enabled by default
    inventoryMask += (itemID >= 17);

    return inventoryMask;
}

//-----------------------------------------------------------------------------
// These functions use data or functions not available in the shadow pass
//-----------------------------------------------------------------------------

#if defined(UNITY_STANDARD_BRDF_INCLUDED)

float3 applyDetailToAlbedo(float3 albedo, float3 detail, float mask)
{
    #if defined(_DETAIL_MULX2)
    	albedo *= LerpWhiteTo (detail.rgb * unity_ColorSpaceDouble.rgb, mask);
    #elif defined(_DETAIL_MUL)
        albedo *= LerpWhiteTo (detail.rgb, mask);
    #elif defined(_DETAIL_ADD)
        albedo += detail.rgb * mask;
    #elif defined(_DETAIL_LERP)
        albedo = lerp (albedo, detail.rgb, mask);
    #endif
    // Standard doesn't saturate albedo, but it can't go negative.
    return max(albedo, 0);
}

SCSS_Input applyDetail(SCSS_Input c, float4 texcoords)
{
	c.albedo *= LerpWhiteTo(_Color.rgb, ColorMask(texcoords.xy));
	if (_CrosstoneToneSeparation) c.tone[0].col *= LerpWhiteTo(_Color.rgb, ColorMask(texcoords.xy));
	if (_Crosstone2ndSeparation) c.tone[1].col *= LerpWhiteTo(_Color.rgb, ColorMask(texcoords.xy));

#if defined(_DETAIL)
    half mask = DetailMask(texcoords.xy);
    half4 detailAlbedo = UNITY_SAMPLE_TEX2D_SAMPLER (_DetailAlbedoMap, _DetailAlbedoMap, texcoords.zw);
    mask *= detailAlbedo.a;
    mask *= _DetailAlbedoMapScale;

	c.albedo = applyDetailToAlbedo(c.albedo, detailAlbedo, mask);
    if (_CrosstoneToneSeparation) c.tone[0].col = applyDetailToAlbedo(c.tone[0].col, detailAlbedo, mask);
	if (_Crosstone2ndSeparation)  c.tone[1].col = applyDetailToAlbedo(c.tone[1].col, detailAlbedo, mask);
#endif
    return c;
}

SCSS_Input applyVertexColour(SCSS_Input c, float4 color, float isOutline)
{
	switch (_VertexColorType)
	{
		case 2: 
		case 0: 
		c.albedo = c.albedo * color.rgb; 
		if (_CrosstoneToneSeparation) c.tone[0].col *= color.rgb; 
		if (_Crosstone2ndSeparation) c.tone[1].col *= color.rgb; 
		break;
		case 1: 
		c.albedo = lerp(c.albedo, color.rgb, isOutline); 
		if (_CrosstoneToneSeparation) c.tone[0].col = lerp(c.tone[0].col, color.rgb, isOutline); 
		if (_Crosstone2ndSeparation)  c.tone[1].col = lerp(c.tone[1].col, color.rgb, isOutline); 
		break;
	}
    return c;
}

half4 SpecularGloss(float4 texcoords, half mask)
{
    half4 sg;
#if defined(_SPECULAR)
    sg = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, texcoords.xy);

    sg.a = _AlbedoAlphaMode == 1? UNITY_SAMPLE_TEX2D(_MainTex, texcoords.xy).a : sg.a;

    sg.rgb *= _SpecColor * _SpecColor.a; // Use alpha as an overall multiplier
    sg.a *= _Smoothness; // _GlossMapScale is what Standard uses for this
#else
    sg = _SpecColor;
    sg.a = _AlbedoAlphaMode == 1? UNITY_SAMPLE_TEX2D(_MainTex, texcoords.xy).a : sg.a;
#endif

#if defined(_DETAIL) 
		float4 sdm = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularDetailMask,_DetailAlbedoMap,texcoords.zw);
		sg *= saturate(sdm + 1-(_SpecularDetailStrength*mask));		
#endif

    return sg;
}

half4 EmissionDetail(float2 uv)
{
#if defined(_EMISSION) 
	if (_EmissionDetailType == 0) // Pulse
	{
		uv += _EmissionDetailParams.xy * _Time.y;
		half4 ed = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailEmissionMap, _DetailEmissionMap, uv);
		if (_EmissionDetailParams.z != 0)
		{
			float s = (sin(ed.r * _EmissionDetailParams.w + _Time.y * _EmissionDetailParams.z))+1;
			ed.rgb = s;
		}
		return ed;
	}
	if (_EmissionDetailType == 1) // AudioLink
	{
		// Load weights texture
		half4 weights = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailEmissionMap, _DetailEmissionMap, uv);
		// Apply a small epsilon to the weights to avoid artifacts.
	    const float epsilon = (1.0/255.0);
	    weights = saturate(weights-epsilon);
	    // sample the texture
	    float4 col = 0;
	    col.rgb += (_alBandR >= 1) ? audioLinkGetLayer(weights.r, _alTimeRangeR, _alBandR, _alModeR) * _alColorR : 0;
	    col.rgb += (_alBandG >= 1) ? audioLinkGetLayer(weights.g, _alTimeRangeG, _alBandG, _alModeG) * _alColorG : 0;
	    col.rgb += (_alBandB >= 1) ? audioLinkGetLayer(weights.b, _alTimeRangeB, _alBandB, _alModeB) * _alColorB : 0;
	    col.rgb += (_alBandA >= 1) ? audioLinkGetLayer(weights.a, _alTimeRangeA, _alBandA, _alModeA) * _alColorA : 0;
	    col.a = 1.0;
	    return col;
	}
#endif
	return 1;
}

half3 NormalInTangentSpace(float4 texcoords, half mask)
{
	float3 normalTangent = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, TRANSFORM_TEX(texcoords.xy, _MainTex)), _BumpScale);
#if defined(_DETAIL) 
    half3 detailNormalTangent = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER (_DetailNormalMap, _MainTex, texcoords.zw), _DetailNormalMapScale);
    #if defined(_DETAIL_LERP)
        normalTangent = lerp(
            normalTangent,
            detailNormalTangent,
            mask);
    #else
        normalTangent = lerp(
            normalTangent,
            BlendNormalsPD(normalTangent, detailNormalTangent),
            mask);
    #endif
#endif

    return normalTangent;
}

#if !defined(SCSS_CROSSTONE)
SCSS_TonemapInput Tonemap(float2 uv, inout float occlusion)
{
	SCSS_TonemapInput t = (SCSS_TonemapInput)0;
	float4 _ShadowMask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_ShadowMask, _MainTex, uv.xy);

	// Occlusion
	if (_ShadowMaskType == 0) 
	{
		// RGB will boost shadow range. Raising _Shadow reduces its influence.
		// Alpha will boost light range. Raising _Shadow reduces its influence.
		t.col = saturate(_IndirectLightingBoost+1-_ShadowMask_var.a) * _ShadowMaskColor.rgb;
		t.bias = _ShadowMaskColor.a*_ShadowMask_var.r;
	}
	// Tone
	if (_ShadowMaskType == 1) 
	{
		t.col = saturate(_ShadowMask_var+_IndirectLightingBoost) * _ShadowMaskColor.rgb;
		t.bias = _ShadowMaskColor.a*_ShadowMask_var.a;
	}
	// Auto-Tone
	if (_ShadowMaskType == 2) 
	{
		float3 albedo = Albedo(uv.xyxy);
		t.col = saturate(AutoToneMapping(albedo)+_IndirectLightingBoost) * _ShadowMaskColor.rgb;
		t.bias = _ShadowMaskColor.a*_ShadowMask_var.r;
	}
	t.bias = (1 - _Shadow) * t.bias + _Shadow;
	occlusion = t.bias;
	return t;
}

// Sample ramp with the specified options.
// rampPosition: 0-1 position on the light ramp from light to dark
// softness: 0-1 position on the light ramp on the other axis
float3 sampleRampWithOptions(float rampPosition, half softness) 
{
	if (_LightRampType == 3) // No sampling
	{
		return saturate(rampPosition*2-1);
	}
	if (_LightRampType == 2) // None
	{
		float shadeWidth = 0.0002 * (1+softness*100);

		const float shadeOffset = 0.5; 
		float lightContribution = simpleSharpen(rampPosition, shadeWidth, shadeOffset);
		return saturate(lightContribution);
	}
	if (_LightRampType == 1) // Vertical
	{
		float2 rampUV = float2(softness, rampPosition);
		return tex2D(_Ramp, saturate(rampUV));
	}
	else // Horizontal
	{
		float2 rampUV = float2(rampPosition, softness);
		return tex2D(_Ramp, saturate(rampUV));
	}
}
#endif

#if defined(SCSS_CROSSTONE)
// Tonemaps contain tone in RGB, occlusion in A.
// Midpoint/width is handled in the application function.
SCSS_TonemapInput Tonemap1st (float2 uv)
{
	float4 tonemap = UNITY_SAMPLE_TEX2D_SAMPLER(_1st_ShadeMap, _MainTex, uv.xy);
	tonemap.rgb = tonemap * _1st_ShadeColor;
	SCSS_TonemapInput t = (SCSS_TonemapInput)1;
	t.col = tonemap.rgb;
	t.bias = tonemap.a;
	return t;
}
SCSS_TonemapInput Tonemap2nd (float2 uv)
{
	float4 tonemap = UNITY_SAMPLE_TEX2D_SAMPLER(_2nd_ShadeMap, _MainTex, uv.xy);
	tonemap.rgb *= _2nd_ShadeColor;
	SCSS_TonemapInput t = (SCSS_TonemapInput)1;
	t.col = tonemap.rgb;
	t.bias = tonemap.a;
	return t;
}

float adjustShadeMap(float x, float y)
{
	// Might be changed later.
	return (x * (1+y));
}

float ShadingGradeMap (float2 uv)
{
	float4 tonemap = UNITY_SAMPLE_TEX2D_SAMPLER(_ShadingGradeMap, _MainTex, uv.xy);
	// Red to match UCTS
	return adjustShadeMap(tonemap.r, _Tweak_ShadingGradeMapLevel);
}
#endif

/*
float innerOutline (VertexOutput i)
{
	// The compiler should merge this with the later calls.
	// Use the vertex normals for this to avoid artifacts.
	SCSS_LightParam d = initialiseLightParam((SCSS_Light)0, i.normalDir, i.posWorld.xyz);
	float baseRim = d.NdotV;
	baseRim = simpleSharpen(baseRim, 0, _InteriorOutlineWidth * OutlineMask(i.uv0.xy));
	return baseRim;
}
*/

float3 applyOutline(float3 col, float is_outline)
{    
	#if defined(SCSS_OUTLINE)
	col = lerp(col, col * _outline_color.rgb, is_outline);
    if (_OutlineMode == 2) 
    {
        col = lerp(col, _outline_color.rgb, is_outline);
    }
    return col;
    #else
    return col;
	#endif
}

SCSS_Input applyOutline(SCSS_Input c, float is_outline)
{
	c.albedo = applyOutline(c.albedo, is_outline);
    if (_CrosstoneToneSeparation) c.tone[0].col = applyOutline(c.tone[0].col, is_outline);
	if (_Crosstone2ndSeparation)  c.tone[1].col = applyOutline(c.tone[1].col, is_outline);

    return c;
}

#endif // if UNITY_STANDARD_BRDF_INCLUDED

#endif // SCSS_INPUT_INCLUDED