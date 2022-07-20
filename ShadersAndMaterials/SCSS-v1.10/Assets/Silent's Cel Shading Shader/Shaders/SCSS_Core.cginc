#ifndef SCSS_CORE_INCLUDED
#define SCSS_CORE_INCLUDED

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"

#include "SCSS_Config.cginc"
#include "SCSS_UnityGI.cginc"
#include "SCSS_Utils.cginc"
#include "SCSS_Input.cginc"
#include "SCSS_Attributes.cginc"


// Shade4PointLights from UnityCG.cginc but only returns their attenuation.
float4 Shade4PointLightsAtten (
    float4 lightPosX, float4 lightPosY, float4 lightPosZ,
    float4 lightAttenSq,
    float3 pos, float3 normal)
{
    // to light vectors
    float4 toLightX = lightPosX - pos.x;
    float4 toLightY = lightPosY - pos.y;
    float4 toLightZ = lightPosZ - pos.z;
    // squared lengths
    float4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;
    // don't produce NaNs if some vertex position overlaps with the light
    lengthSq = max(lengthSq, 0.000001);

    // NdotL
    float4 ndotl = 0;
    ndotl += toLightX * normal.x;
    ndotl += toLightY * normal.y;
    ndotl += toLightZ * normal.z;
    // correct NdotL
    float4 corr = 0;//rsqrt(lengthSq);
    corr.x = fastRcpSqrtNR0(lengthSq.x);
    corr.y = fastRcpSqrtNR0(lengthSq.y);
    corr.z = fastRcpSqrtNR0(lengthSq.z);
    corr.w = fastRcpSqrtNR0(lengthSq.x);

    ndotl = corr * (ndotl * 0.5 + 0.5); // Match with Forward for light ramp sampling
    ndotl = max (float4(0,0,0,0), ndotl);
    // attenuation
    // Fixes popin. Thanks, d4rkplayer!
    float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
	float4 atten2 = saturate(1 - (lengthSq * lightAttenSq / 25));
	atten = min(atten, atten2 * atten2);

    float4 diff = ndotl * atten;
    #if defined(SCSS_UNIMPORTANT_LIGHTS_FRAGMENT)
    return atten;
    #else
    return diff;
    #endif
}

// Based on Standard Shader's forwardbase vertex lighting calculations in VertexGIForward
// This revision does not pass the light values themselves, but only their attenuation.
inline half4 VertexLightContribution(float3 posWorld, half3 normalWorld)
{
	half4 vertexLight = 0;

	// Static lightmapped materials are not allowed to have vertex lights.
	#ifdef LIGHTMAP_ON
		return 0;
	#elif UNITY_SHOULD_SAMPLE_SH
		#ifdef VERTEXLIGHT_ON
			// Approximated illumination from non-important point lights
			vertexLight = Shade4PointLightsAtten(
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_4LightAtten0, posWorld, normalWorld);
		#endif
	#endif

	return vertexLight;
}

#if defined(_SUBSURFACE)
//SSS method from GDC 2011 conference by Colin Barre-Bresebois & Marc Bouchard and modified by Xiexe
float3 getSubsurfaceScatteringLight (SCSS_Light l, float3 normalDirection, float3 viewDirection, 
    float attenuation, float3 thickness, float3 indirectLight = 0)
{
    float3 vSSLight = l.dir + normalDirection * _SSSDist; // Distortion
    float3 vdotSS = pow(saturate(dot(viewDirection, -vSSLight)), _SSSPow) 
        * _SSSIntensity; 
    
    return lerp(1, attenuation, float(any(_WorldSpaceLightPos0.xyz))) 
                * (vdotSS + _SSSAmbient) * abs(_ThicknessMapInvert-thickness)
                * (l.color + indirectLight) * _SSSCol;
                
}
#endif

float3 sampleCrossToneLighting(inout float x, SCSS_TonemapInput tone0, SCSS_TonemapInput tone1, float3 albedo)
{
	// A three-tiered tone system.
	// Input x is potentially affected by occlusion map.

	x = x;
	half offset0 = _1st_ShadeColor_Step * tone0.bias; 
	half width0  = _1st_ShadeColor_Feather;
	half factor0 = saturate(simpleSharpen(x, width0, offset0));

	half offset1 = _2nd_ShadeColor_Step * tone1.bias; 
	half width1  = _2nd_ShadeColor_Feather;
	half factor1 = saturate(simpleSharpen(x, width1, offset1));

	float3 final;

	// 2nd separation determines whether 1st and 2nd shading tones are combined.
	if (_Crosstone2ndSeparation == 0) 	tone1.col = tone1.col * tone0.col;
	// if (_Crosstone2ndSeparation == 1) 	tone1.col = tone1.col; // Just here for completeness
	
	// Either way, the result is interpolated against tone 0 by the 2nd factor.
	final = lerp(tone1.col, tone0.col, factor1);

	// Tone separation determines whether albedo and 1st shading tones are combined.
	if (_CrosstoneToneSeparation == 0) 	final = lerp(final, 1.0, factor0) * albedo;
	if (_CrosstoneToneSeparation == 1) 	final = lerp(final, albedo, factor0);

	x = factor0;
	
	return final;
}

#if !defined(SCSS_CROSSTONE)
float applyShadowLift(float baseLight, float occlusion)
{
	baseLight *= occlusion;
	baseLight = _ShadowLift + baseLight * (1-_ShadowLift);
	return baseLight;
}

float applyShadowLift(float4 baseLight, float occlusion)
{
	baseLight *= occlusion;
	baseLight = _ShadowLift + baseLight * (1-_ShadowLift);
	return baseLight;
}
#endif

float getRemappedLight(half perceptualRoughness, SCSS_LightParam d)
{
	float diffuseShadowing = DisneyDiffuse(abs(d.NdotV), abs(d.NdotL), d.LdotH, perceptualRoughness);
	float remappedLight = d.NdotL * LerpOneTo(diffuseShadowing, _DiffuseGeomShadowFactor);
	return remappedLight;
}

float applyAttenuation(half NdotL, half attenuation)
{
	#if defined(SCSS_CROSSTONE)
	//attenuation = round(attenuation);
	half shadeVal = _1st_ShadeColor_Step - _1st_ShadeColor_Feather * 0.5;
	shadeVal = shadeVal-0.01;
	//NdotL = min(lerp(shadeVal, NdotL, attenuation), NdotL);
	NdotL = lerp(shadeVal*NdotL, NdotL, attenuation);
	#else
	NdotL = min(NdotL * attenuation, NdotL);
	//NdotL = lerp(0.5, NdotL, attenuation);
	#endif
	return NdotL;
}

half3 calcVertexLight(float4 vertexAttenuation, float occlusion, SCSS_TonemapInput tone[2], half softness)
{
	float3 vertexContribution = 0;
	#if defined(UNITY_PASS_FORWARDBASE)

		#if !defined(SCSS_CROSSTONE)
		vertexAttenuation = applyShadowLift(vertexAttenuation, occlusion);
    	for (int num = 0; num < 4; num++) {
    		vertexContribution += unity_LightColor[num] * 
    			(sampleRampWithOptions(vertexAttenuation[num], softness)+tone[0].col);
    	}
    	#endif

		#if defined(SCSS_CROSSTONE)
    	for (int num = 0; num < 4; num++) {
    		vertexContribution += unity_LightColor[num] * 
    			sampleCrossToneLighting(vertexAttenuation[num], tone[0], tone[1], 1.0);
    	}
    	#endif

	#endif
	return vertexContribution;
}

void getDirectIndirectLighting(float3 normal, out float3 directLighting, out float3 indirectLighting)
{
	directLighting = 0.0;
	indirectLighting = 0.0;
	switch (_LightingCalculationType)
	{
	case 0: // Unbiased
		directLighting   = GetSHMaxL1();
		indirectLighting = GetSHAverage(); 
	break;
	case 1: // Standard
		directLighting = 
		indirectLighting = BetterSH9(half4(normal, 1.0));
	break;
	case 2: // Cubed
		directLighting   = BetterSH9(half4(0.0,  1.0, 0.0, 1.0));
		indirectLighting = BetterSH9(half4(0.0, -1.0, 0.0, 1.0)); 
	break;
	case 3: // True Directional
		float4 ambientDir = float4(Unity_SafeNormalize(unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz), 1.0);
		directLighting   = BetterSH9(ambientDir);
		indirectLighting = BetterSH9(-ambientDir); 
	break;
	case 4: // Biased
		directLighting   = GetSHMaxL1();
		indirectLighting = BetterSH9(half4(0.0, 0.0, 0.0, 1.0)); 
	break;
	}
    // Workaround for scenes with HDR off blowing out in VRchat.
    if (getLightClampActive())
    {
        directLighting = saturate(directLighting);
        indirectLighting = saturate(indirectLighting);
    }
}

// For baked lighting.
half3 calcDiffuseGI(float3 albedo, SCSS_TonemapInput tone[2], float occlusion, half softness,
	float3 indirectLighting, float3 directLighting, SCSS_LightParam d)
{
	float ambientLight = d.NdotAmb;

	/*
	Ambient lighting splitting: 
	Strong shading looks good, but weak shading looks bad. 
	This system removes shading if it's too weak.
	*/
	
	float3 indirectAverage = 0.5 * (indirectLighting + directLighting);

	// Make this a UI value later.
	const half ambientLightSplitThreshold = 1.0/1.0;
	half ambientLightSplitFactor = 
	saturate(
		dot(abs((directLighting-indirectLighting)/indirectAverage), 
		ambientLightSplitThreshold * sRGB_Luminance));

	#if !defined(SCSS_CROSSTONE)
	ambientLight = applyShadowLift(ambientLight, occlusion);
	float3 indirectContribution = sampleRampWithOptions(ambientLight, softness);
	indirectLighting = lerp(indirectLighting, directLighting, tone[0].col);
	indirectAverage = lerp(indirectAverage, directLighting, tone[0].col);
	#endif

	#if defined(SCSS_CROSSTONE)
	ambientLight *= occlusion;
	indirectAverage *= albedo;
	float3 indirectContribution = sampleCrossToneLighting(ambientLight, tone[0], tone[1], albedo);
	#endif

	float3 lightContribution;

	#if defined(SCSS_CROSSTONE)
	if (_CrosstoneToneSeparation == 0) lightContribution = 
	lerp(indirectAverage,
	lerp(indirectLighting, directLighting, indirectContribution),
	ambientLightSplitFactor) * albedo;

	if (_CrosstoneToneSeparation == 1) lightContribution = 
	lerp(indirectAverage,
	directLighting*indirectContribution,
	ambientLightSplitFactor);
	#endif

	#if !defined(SCSS_CROSSTONE)
	lightContribution = 
	lerp(indirectAverage,
	lerp(indirectLighting, directLighting, indirectContribution),
	ambientLightSplitFactor) * albedo;
	#endif

	return lightContribution;
}

// For directional lights where attenuation is shadow.
half3 calcDiffuseBase(float3 albedo, SCSS_TonemapInput tone[2], float occlusion, half perceptualRoughness, 
	half attenuation, half softness, SCSS_LightParam d, SCSS_Light l)
{
	float remappedLight = getRemappedLight(perceptualRoughness, d);
	remappedLight = remappedLight * 0.5 + 0.5;

	remappedLight = applyAttenuation(remappedLight, attenuation);

	#if !defined(SCSS_CROSSTONE)
	remappedLight = applyShadowLift(remappedLight, occlusion);
	float3 lightContribution = lerp(tone[0].col, 1.0, sampleRampWithOptions(remappedLight, softness)) * albedo;
	#endif

	#if defined(SCSS_CROSSTONE)
	remappedLight *= occlusion;
	float3 lightContribution = sampleCrossToneLighting(remappedLight, tone[0], tone[1], albedo);
	#endif

	lightContribution *= l.color;

	return lightContribution;	
}

// For point/spot lights where attenuation is shadow+attenuation.
half3 calcDiffuseAdd(float3 albedo, SCSS_TonemapInput tone[2], float occlusion, half perceptualRoughness, 
	half softness, SCSS_LightParam d, SCSS_Light l)
{
	float remappedLight = getRemappedLight(perceptualRoughness, d);
	remappedLight = remappedLight * 0.5 + 0.5;

	#if !defined(SCSS_CROSSTONE)
	remappedLight = applyShadowLift(remappedLight, occlusion);
	float3 lightContribution = sampleRampWithOptions(remappedLight, softness);

	float3 directLighting = l.color;
	float3 indirectLighting = l.color * tone[0].col;

	lightContribution = lerp(indirectLighting, directLighting, lightContribution) * albedo;
	#endif

	#if defined(SCSS_CROSSTONE)
	float3 lightContribution = sampleCrossToneLighting(remappedLight, tone[0], tone[1], albedo);
	lightContribution *= l.color;
	#endif

	return lightContribution;
}

#if defined(_SPECULAR)
void getSpecularVD(float roughness, SCSS_LightParam d, SCSS_Light l, VertexOutput i,
	out half V, out half D)
{
	V = 0; D = 0;

	#ifndef SHADER_TARGET_GLSL
	[call]
	#endif
	switch(_SpecularType)
	{
	case 1: // GGX
		V = SmithJointGGXVisibilityTerm (d.NdotL, d.NdotV, roughness);
	    D = GGXTerm (d.NdotH, roughness);
	    break;

	case 2: // Charlie (cloth)
		V = V_Neubelt (d.NdotV, d.NdotL);
	    D = D_Charlie (roughness, d.NdotH);
	    break;

	case 3: // GGX anisotropic
	    float anisotropy = _Anisotropy;
	    float at = max(roughness * (1.0 + anisotropy), 0.002);
	    float ab = max(roughness * (1.0 - anisotropy), 0.002);

		V = SmithJointGGXVisibilityTerm (d.NdotL, d.NdotV, roughness);
	    D = D_GGX_Anisotropic(d.NdotH, d.halfDir, i.tangentDir, i.bitangentDir, at, ab);
	    break;
	}
	return;
}

half3 calcSpecularBase(float3 specColor, float smoothness, float3 normal, float oneMinusReflectivity, float perceptualRoughness,
	float attenuation, float occlusion, SCSS_LightParam d, SCSS_Light l, VertexOutput i)
{
	UnityGI gi = (UnityGI)0;
	
	half V = 0; half D = 0; 
	float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

	// "GGX with roughness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughness remapping."
	// This also fixes issues with the other specular types.
	roughness = max(roughness, 0.002);

	d = saturate(d);

	getSpecularVD(roughness, d, l, i, /*out*/ V, /*out*/ D);

	half specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later
	specularTerm = max(0, specularTerm * d.NdotL);

	#if defined(_SPECULARHIGHLIGHTS_OFF)
    	specularTerm = 0.0;
	#endif

	half surfaceReduction = 1.0 / (roughness*roughness + 1);

    // To provide true Lambert lighting, we need to be able to kill specular completely.
    specularTerm *= any(specColor) ? 1.0 : 0.0;

	gi =  GetUnityGI(l.color.rgb, l.dir, normal, 
		d.viewDir, d.reflDir, attenuation, occlusion, perceptualRoughness, i.posWorld.xyz);

	float grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));

	return
	specularTerm * (gi.light.color) * FresnelTerm(specColor, d.LdotH) +
	surfaceReduction * (gi.indirect.specular.rgb) * FresnelLerp(specColor, grazingTerm, d.NdotV);
	
}

half3 calcSpecularBase(SCSS_Input c, float perceptualRoughness, float attenuation,
	SCSS_LightParam d, SCSS_Light l, VertexOutput i)
{
	return calcSpecularBase(c.specColor, c.smoothness, c.normal, c.oneMinusReflectivity, 
		perceptualRoughness, attenuation, c.occlusion, d, l, i);
}

half3 calcSpecularAdd(float3 specColor, float smoothness, float3 normal, float oneMinusReflectivity, float perceptualRoughness,
	SCSS_LightParam d, SCSS_Light l, VertexOutput i)
{
	#if defined(_SPECULARHIGHLIGHTS_OFF)
		return 0.0;
	#endif
	
	half V = 0; half D = 0; 
	float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);

	// "GGX with roughness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughness remapping."
	// This also fixes issues with the other specular types.
	roughness = max(roughness, 0.002);

	d = saturate(d);

	getSpecularVD(roughness, d, l, i, /*out*/ V, /*out*/ D);

	half specularTerm = V*D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later
	specularTerm = max(0, specularTerm * d.NdotL);

    // To provide true Lambert lighting, we need to be able to kill specular completely.
    specularTerm *= any(specColor) ? 1.0 : 0.0;

	return
	specularTerm * l.color * FresnelTerm(specColor, d.LdotH);
	
}

half3 calcSpecularAdd(SCSS_Input c, float perceptualRoughness,
	SCSS_LightParam d, SCSS_Light l, VertexOutput i)
{
	return calcSpecularAdd(c.specColor, c.smoothness, c.normal, c.oneMinusReflectivity, perceptualRoughness, d, l, i);
}

half3 calcSpecularCel(float3 specColor, float smoothness, float3 normal, float oneMinusReflectivity, float perceptualRoughness,
	float attenuation, SCSS_LightParam d, SCSS_Light l, VertexOutput i)
{
	if (_SpecularType == 4) {
		// 
		float spec = max(d.NdotH, 0);
		spec = pow(spec, (smoothness)*40) * _CelSpecularSteps;
		spec = sharpenLighting(frac(spec), _CelSpecularSoftness)+floor(spec);
    	spec = max(0.02,spec);
    	spec *= UNITY_PI * rcp(_CelSpecularSteps);

    	float3 envLight = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, normal, UNITY_SPECCUBE_LOD_STEPS);
		return (spec * specColor *  l.color) + (spec * specColor * envLight);
	}
	if (_SpecularType == 5) {
		// It might be better if these are passed in parameters in future
		float anisotropy = _Anisotropy;
		float softness = _CelSpecularSoftness;
		float3 strandTangent = (anisotropy < 0)
		? i.tangentDir
		: i.bitangentDir;
		anisotropy = abs(anisotropy);
		strandTangent = lerp(normal, strandTangent, anisotropy);
		float exponent = smoothness;
		float spec  = StrandSpecular(strandTangent, d.halfDir, 
			exponent*80, 1.0 );
		float spec2 = StrandSpecular(strandTangent, d.halfDir, 
			exponent*40, 0.5 );
		spec  = sharpenLighting(frac(spec), softness)+floor(spec);
		spec2 = sharpenLighting(frac(spec2), softness)+floor(spec2);
		spec += spec2;
		
    	float3 envLight = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, normal, UNITY_SPECCUBE_LOD_STEPS);
		return (spec * specColor *  l.color) + (spec * specColor * envLight);
	}
	return 0;
}

half3 calcSpecularCel(SCSS_Input c, float perceptualRoughness, float attenuation, SCSS_LightParam d, SCSS_Light l, VertexOutput i)
{
	return calcSpecularCel(c.specColor, c.smoothness, c.normal, c.oneMinusReflectivity, perceptualRoughness, attenuation, d, l, i);
}

#endif // _SPECULAR

float3 SCSS_ShadeBase(SCSS_Input c, VertexOutput i, SCSS_Light l, float attenuation)
{	
	float3 finalColor;

	float isOutline = i.extraData.x;

	SCSS_LightParam d = initialiseLightParam(l, c.normal, i.posWorld.xyz);

	float3 directLighting, indirectLighting;

	getDirectIndirectLighting(c.normal, /*out*/ directLighting, /*out*/ indirectLighting);

	finalColor  = calcDiffuseGI(c.albedo, c.tone, c.occlusion, c.softness, indirectLighting, directLighting, d);
	finalColor += calcDiffuseBase(c.albedo, c.tone, c.occlusion,
		c.perceptualRoughness, attenuation, c.softness, d, l);

	// Prepare fake light params for subsurface scattering.
	SCSS_Light iL = l;
	SCSS_LightParam iD = d;
	iL.color = GetSHMaxL1();
	iL.dir = Unity_SafeNormalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * _LightSkew.xyz);
	iD = initialiseLightParam(iL, c.normal, i.posWorld.xyz);

	// Prepare fake light params for spec/fresnel which simulate specular.
	SCSS_Light fL = l;
	SCSS_LightParam fD = d;
	fL.color = attenuation * fL.color + GetSHMaxL1();
	fL.dir = Unity_SafeNormalize(fL.dir + (unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz) * _LightSkew.xyz);
	fD = initialiseLightParam(fL, c.normal, i.posWorld.xyz);

	if (isOutline <= 0)
	{
		#if defined(_SUBSURFACE)
			#if defined(USING_DIRECTIONAL_LIGHT)
			finalColor += getSubsurfaceScatteringLight(l, c.normal, d.viewDir,
				attenuation, c.thickness) * c.albedo;
			#endif
		finalColor += getSubsurfaceScatteringLight(iL, c.normal, iD.viewDir,
			1, c.thickness) * c.albedo;
		#endif

		#if defined(_METALLICGLOSSMAP)
	    finalColor += calcSpecularBase(c, c.perceptualRoughness, attenuation, d, l, i);
	    #endif

	    #if defined(_SPECGLOSSMAP)
    	finalColor += calcSpecularCel(c, c.perceptualRoughness, attenuation, fD, fL, i);
   		#endif
    };

    return finalColor;
}

float3 SCSS_ShadeLight(SCSS_Input c, VertexOutput i, SCSS_Light l, half attenuation)
{
	float3 finalColor;

	float isOutline = i.extraData.x;

	SCSS_LightParam d = initialiseLightParam(l, c.normal, i.posWorld.xyz);

    finalColor = calcDiffuseAdd(c.albedo, c.tone, c.occlusion, c.perceptualRoughness, c.softness, d, l);

	if (isOutline <= 0)
	{
		#if defined(_SUBSURFACE) 
		finalColor += c.albedo * getSubsurfaceScatteringLight(l, c.normal, d.viewDir,
			attenuation, c.thickness, c.tone[0].col);
		#endif

		#if defined(_METALLICGLOSSMAP)
    	finalColor += calcSpecularAdd(c, c.perceptualRoughness, d, l, i);
    	#endif

		#if defined(_SPECGLOSSMAP)
		finalColor += calcSpecularCel(c, c.perceptualRoughness, attenuation, d, l, i);
		#endif
	};
	return finalColor;
}


void addEmissive(const SCSS_Input c, const VertexOutput i, float3 effectLighting, inout float3 color)
{
	float isOutline = i.extraData.x;
	float3 emission;
	float2 emissionDetailUV = EmissionDetailTexCoords(i.uv0, i.uv1);
	float4 emissionDetail = EmissionDetail(emissionDetailUV);

	color = max(0, color - saturate((1-emissionDetail.w)- (1-c.emission)));
	emission = emissionDetail.rgb * c.emission * _EmissionColor.rgb;

	// Glow in the dark modifier.
	#if defined(_EMISSION)
		float glowModifier = smoothstep(_EmissiveLightSenseStart, _EmissiveLightSenseEnd, dot(effectLighting, sRGB_Luminance));
		if (_UseEmissiveLightSense) emission *= glowModifier;
	#endif

	emission *= (1-isOutline);
	color += emission;
}

float3 SCSS_ApplyLighting(SCSS_Input c, VertexOutput i, float4 texcoords)
{
	UNITY_LIGHT_ATTENUATION(attenuation, i, i.posWorld.xyz);

	#if defined(SCSS_SCREEN_SHADOW_FILTER) && defined(USING_SHADOWS_UNITY)
	correctedScreenShadowsForMSAA(i._ShadowCoord, attenuation);
	#endif

	float isOutline = i.extraData.x;

	// Lighting parameters
	SCSS_Light l = MainLight(i.posWorld.xyz);
	#if defined(UNITY_PASS_FORWARDADD) && !defined(USING_DIRECTIONAL_LIGHT)
	l.dir = normalize(_WorldSpaceLightPos0.xyz - i.posWorld.xyz);
	#endif 

	SCSS_LightParam d = initialiseLightParam(l, c.normal, i.posWorld.xyz);

	#if defined(_METALLICGLOSSMAP)
	// Geometric Specular AA from HDRP
	c.smoothness = GeometricNormalFiltering(c.smoothness, i.normalDir.xyz, 0.25, 0.5);
	#endif

	#if defined(_METALLICGLOSSMAP)
	// Perceptual roughness transformation. Without this, roughness handling is wrong.
	c.perceptualRoughness = SmoothnessToPerceptualRoughness(c.smoothness);
	#else
	// Disable DisneyDiffuse for cel specular.
	#endif

	// Generic lighting for effects.
	float3 effectLighting = l.color;
	#if defined(UNITY_PASS_FORWARDBASE)
	effectLighting += GetSHAverage();
	#endif

	// Generic lighting for effects with shadow applied.
	float3 effectLightShadow = l.color * max((1+d.NdotL)*attenuation, 0);
	#if defined(UNITY_PASS_FORWARDBASE)
	effectLightShadow += GetSHAverage();
	#endif

    // Workaround for scenes with HDR off blowing out in VRchat.
    if (getLightClampActive())
    {
	    // Colour-preserving clamp.
	    // This light value is used later to flatten the overall output intensity. 
	    // Get the maximum input value from the lighting.
	    float maxEffectLight = max3(effectLighting);
	    // The effect lighting is remapped to be within the 0-1.25 range when clamped.
	    float modLight = min(maxEffectLight, 1.25);
	    // Scale the values by the highest value. 
	    // Needs a bit more testing, but should look nice. 
	    effectLighting = (effectLighting/maxEffectLight)*modLight;
	}

	float3 finalColor = 0; 

	// Apply matcap before specular effect.
	if (_UseMatcap >= 1 && isOutline <= 0) 
	{
		half2 matcapUV;
		if (_UseMatcap == 1) matcapUV = getMatcapUVsOriented(c.normal, d.viewDir, float3(0, 1, 0));
		if (_UseMatcap == 2) matcapUV = getMatcapUVsOriented(c.normal, d.viewDir, i.bitangentDir.xyz);

		float4 _MatcapMask_var = MatcapMask(i.uv0.xy);
		c.albedo = applyMatcap(_Matcap1, matcapUV, c.albedo, _Matcap1Tint, _Matcap1Blend, _Matcap1Strength * _MatcapMask_var.r);
		c.albedo = applyMatcap(_Matcap2, matcapUV, c.albedo, _Matcap2Tint, _Matcap2Blend, _Matcap2Strength * _MatcapMask_var.g);
		c.albedo = applyMatcap(_Matcap3, matcapUV, c.albedo, _Matcap3Tint, _Matcap3Blend, _Matcap3Strength * _MatcapMask_var.b);
		c.albedo = applyMatcap(_Matcap4, matcapUV, c.albedo, _Matcap4Tint, _Matcap4Blend, _Matcap4Strength * _MatcapMask_var.a);
	}


	float3 finalRimLight = 0;
	if (_UseFresnel)
	{
		float fresnelLightMaskBase = LerpOneTo((d.NdotH), _UseFresnelLightMask);
		float fresnelLightMask = 
			saturate(pow(saturate( fresnelLightMaskBase), _FresnelLightMask));
		float fresnelLightMaskInv = 
			saturate(pow(saturate(-fresnelLightMaskBase), _FresnelLightMask));

		// Refactored to use more ifs because the compiler is smarter than me.
		float rimBase = sharpenLighting(d.rlPow4.y * c.rim.width * fresnelLightMask, c.rim.power) * c.rim.alpha;
		float3 rimCol = rimBase * c.rim.tint;

		float rimInv = sharpenLighting(d.rlPow4.y * c.rim.invWidth * fresnelLightMaskInv,
			c.rim.invPower) * _FresnelLightMask * c.rim.invAlpha;
		float3 rimInvCol = rimInv * c.rim.invTint;

		float3 rimFinal = rimCol + rimInvCol;

		float applyToAlbedo = (_UseFresnel == 1) + (_UseFresnel == 4);
		float applyToFinal = (_UseFresnel == 2);
		float applyToLightBias = (_UseFresnel == 3) + (_UseFresnel == 4);
		// Lit
		if (applyToAlbedo) c.albedo += c.albedo * rimFinal * (1-isOutline);
		// AmbientAlt
		if (applyToLightBias) c.occlusion += saturate(rimBase) * (1-isOutline);
		// Ambient
		// If applied to the final output, it can only be applied later.
		if (applyToFinal) finalRimLight = rimFinal * (1-isOutline);

	}

    // Workaround for scenes with HDR off blowing out in VRchat.
    if (getLightClampActive()) l.color = saturate(l.color);

	#if defined(UNITY_PASS_FORWARDBASE)
	finalColor = SCSS_ShadeBase(c, i, l, attenuation);
	#endif

	#if defined(UNITY_PASS_FORWARDADD)
	finalColor = SCSS_ShadeLight(c, i, l, attenuation);
	#endif

	// Proper cheap vertex lights. 
	#if defined(VERTEXLIGHT_ON) && !defined(SCSS_UNIMPORTANT_LIGHTS_FRAGMENT)
	finalColor += c.albedo * calcVertexLight(i.vertexLight, c.occlusion, c.tone, c.softness);
	#endif

	// Apply Ambient rim lighting
	if (_UseFresnel == 2)
	{
		finalColor += effectLighting*finalRimLight;
	}

    // Apply full lighting to unimportant lights. This is cheaper than you might expect.
	#if defined(UNITY_PASS_FORWARDBASE) && defined(VERTEXLIGHT_ON) && defined(SCSS_UNIMPORTANT_LIGHTS_FRAGMENT)
    for (int num = 0; num < 4; num++) {
    	UNITY_BRANCH if ((unity_LightColor[num].r + unity_LightColor[num].g + unity_LightColor[num].b + i.vertexLight[num]) != 0.0)
    	{
    	l.color = unity_LightColor[num].rgb;
    	l.dir = normalize(float3(unity_4LightPosX0[num], unity_4LightPosY0[num], unity_4LightPosZ0[num]) - i.posWorld.xyz);

    	if (getLightClampActive()) l.color = saturate(l.color);

		float3 addColor = SCSS_ShadeLight(c, i, l, 1) *  i.vertexLight[num];

    	if (getLightClampActive()) addColor = saturate(addColor);
    	
		finalColor += addColor;
    	}
    };
	#endif

	#if defined(UNITY_PASS_FORWARDADD)
		finalColor *= attenuation;
	#endif

	finalColor *= _LightWrappingCompensationFactor;

	// Apply the light scaling if the light clamp is active. When the light clamp is active,
	// the final colour is divided by the light intensity
   	if (getLightClampActive()) finalColor = finalColor / max(max3(effectLighting), 1);

	finalColor *= _LightMultiplyAnimated;

	#if defined(UNITY_PASS_FORWARDBASE)
	addEmissive(c, i, effectLightShadow, finalColor);

	// Emissive rim. 
	finalColor += _CustomFresnelColor.xyz * (pow(d.rlPow4.y, rcp(_CustomFresnelColor.w+0.0001)));
	#endif

	return finalColor;
}

#endif // SCSS_CORE_INCLUDED