// Altered UnityGI calculations for higher quality light probe sampling.
#ifndef SCSS_UNITYGI_INCLUDED
#define SCSS_UNITYGI_INCLUDED

/* http://www.geomerics.com/wp-content/uploads/2015/08/CEDEC_Geomerics_ReconstructingDiffuseLighting1.pdf */
float shEvaluateDiffuseL1Geomerics_local(float L0, float3 L1, float3 n)
{
    // average energy
    // Add max0 to fix an issue caused by probes having a negative ambient component (???)
    // I'm not sure how normal that is but this can't handle it
    float R0 = max(L0, 0);

	// avg direction of incoming light
	float3 R1 = 0.5f * L1;

	// directional brightness
	float lenR1 = length(R1);

	// linear angle between normal and direction 0-1
	float q = dot(normalize(R1), n) * 0.5 + 0.5;
	q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.

	// power for q
	// lerps from 1 (linear) to 3 (cubic) based on directionality
	float p = 1.0f + 2.0f * lenR1 / R0;

	// dynamic range constant
	// should vary between 4 (highly directional) and 0 (ambient)
	float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);

	return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

// SH Convolution Functions
// https://github.com/lukis101/VRCUnityStuffs/tree/master/SH
// Code adapted from https://blog.selfshadow.com/2012/01/07/righting-wrap-part-2/
///////////////////////////

float3 GeneralWrapSH(float fA) // original unoptimized
{
    // Normalization factor for our model.
    float norm = 0.5 * (2 + fA) / (1 + fA);
    float4 t = float4(2 * (fA + 1), fA + 2, fA + 3, fA + 4);
    return norm * float3(t.x / t.y, 2 * t.x / (t.y * t.z),
    t.x * (fA * fA - t.x + 5) / (t.y * t.z * t.w));
}
float3 GeneralWrapSHOpt(float fA)
{
    const float4 t0 = float4(-0.047771, -0.129310, 0.214438, 0.279310);
    const float4 t1 = float4( 1.000000,  0.666667, 0.250000, 0.000000);

    float3 r;
    r.xyz = saturate(t0.xxy * fA + t0.yzw);
    r.xyz = -r * fA + t1.xyz;
    return r;
}

float3 GreenWrapSHOpt(float fW)
{
    const float4 t0 = float4(0.0, 1.0 / 4.0, -1.0 / 3.0, -1.0 / 2.0);
    const float4 t1 = float4(1.0, 2.0 / 3.0,  1.0 / 4.0,  0.0);

    float3 r;
    r.xyz = t0.xxy * fW + t0.xzw;
    r.xyz = r.xyz * fW + t1.xyz;
    return r;
}

float3 ShadeSH9_wrapped(float3 normal, float3 conv)
{
    float3 x0, x1, x2;
    conv *= float3(1, 1.5, 4); // Undo pre-applied cosine convolution
    //conv *= _Bands.xyz; // debugging

    // Constant (L0)
    // Band 0 has constant part from 6th kernel (band 1) pre-applied, but ignore for performance
    x0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);

    // Linear (L1) polynomial terms
    x1.r = (dot(unity_SHAr.xyz, normal));
    x1.g = (dot(unity_SHAg.xyz, normal));
    x1.b = (dot(unity_SHAb.xyz, normal));

    // 4 of the quadratic (L2) polynomials
    float4 vB = normal.xyzz * normal.yzzx;
    x2.r = dot(unity_SHBr, vB);
    x2.g = dot(unity_SHBg, vB);
    x2.b = dot(unity_SHBb, vB);

    // Final (5th) quadratic (L2) polynomial
    float vC = normal.x * normal.x - normal.y * normal.y;
    x2 += unity_SHC.rgb * vC;

    return x0 * conv.x + x1 * conv.y + x2 * conv.z;
}

float3 ShadeSH9_wrappedCorrect(float3 normal, float3 conv)
{
    const float3 cosconv_inv = float3(1, 1.5, 4); // Inverse of the pre-applied cosine convolution
    float3 x0, x1, x2;
    conv *= cosconv_inv; // Undo pre-applied cosine convolution
    //conv *= _Bands.xyz; // debugging

    // Constant (L0)
    x0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    // Remove the constant part from L2 and add it back with correct convolution
    float3 otherband = float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
    x0 = (x0 + otherband) * conv.x - otherband * conv.z;

    // Linear (L1) polynomial terms
    x1.r = (dot(unity_SHAr.xyz, normal));
    x1.g = (dot(unity_SHAg.xyz, normal));
    x1.b = (dot(unity_SHAb.xyz, normal));

    // 4 of the quadratic (L2) polynomials
    float4 vB = normal.xyzz * normal.yzzx;
    x2.r = dot(unity_SHBr, vB);
    x2.g = dot(unity_SHBg, vB);
    x2.b = dot(unity_SHBb, vB);

    // Final (5th) quadratic (L2) polynomial
    float vC = normal.x * normal.x - normal.y * normal.y;
    x2 += unity_SHC.rgb * vC;

    return x0 + x1 * conv.y + x2 * conv.z;
}

bool isReflectionProbeActive()
{
#ifndef SHADER_TARGET_SURFACE_ANALYSIS // Required to use GetDimensions
    float height, width;
    unity_SpecCube0.GetDimensions(width, height);
    return !(height * width < 32);
#endif
    return 1;
}

inline UnityGI UnityGlobalIllumination_SCSS (UnityGIInput data, half occlusion, half3 normalWorld, Unity_GlossyEnvironmentData glossIn)
{
    UnityGI o_gi = UnityGI_Base(data, occlusion, normalWorld);
	#if defined(SAMPLE_SH_NONLINEAR) 
	    float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w)
        + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
	    float3 nonLinearSH = float3(0,0,0); 
	    nonLinearSH.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normalWorld);
	    nonLinearSH.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normalWorld);
	    nonLinearSH.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normalWorld);
	    nonLinearSH = max(nonLinearSH, 0);
	    o_gi.indirect.diffuse += nonLinearSH * occlusion;
    #endif
    o_gi.indirect.specular = isReflectionProbeActive()
    ? UnityGI_IndirectSpecular(data, occlusion, glossIn)
    : o_gi.indirect.diffuse;
    return o_gi;
}

UnityGI GetUnityGI(float3 lightColor, float3 lightDirection, float3 normalDirection,float3 viewDirection, 
float3 viewReflectDirection, float attenuation, float occlusion, float roughness, float3 worldPos){
    UnityLight light;
    light.color = lightColor;
    light.dir = lightDirection;
    light.ndotl = max(0.0h,dot( normalDirection, lightDirection));
    UnityGIInput d = (UnityGIInput) 0;
    d.light = light;
    d.worldPos = worldPos;
    d.worldViewDir = viewDirection;
    d.atten = attenuation;
    d.ambient = 0.0h;
    d.boxMax[0] = unity_SpecCube0_BoxMax;
    d.boxMin[0] = unity_SpecCube0_BoxMin;
    d.probePosition[0] = unity_SpecCube0_ProbePosition;
    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.boxMax[1] = unity_SpecCube1_BoxMax;
    d.boxMin[1] = unity_SpecCube1_BoxMin;
    d.probePosition[1] = unity_SpecCube1_ProbePosition;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    Unity_GlossyEnvironmentData ugls_en_data;
    ugls_en_data.roughness = roughness;
    ugls_en_data.reflUVW = viewReflectDirection;
    UnityGI gi = UnityGlobalIllumination_SCSS(d, occlusion, normalDirection, ugls_en_data );
    return gi;
}

half3 BetterSH9 (half4 normal) {
	float3 indirect;
	float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w)
     + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
	indirect.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normal);
	indirect.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normal);
	indirect.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normal);
	indirect = max(0, indirect);
	indirect += SHEvalLinearL2(normal);
	return indirect;
}

float3 BetterSH9(float3 normal)
{
    return BetterSH9(float4(normal, 1));
}

#endif // SCSS_UNITYGI_INCLUDED