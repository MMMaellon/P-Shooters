#ifndef SCSS_FORWARD_INCLUDED
#define SCSS_FORWARD_INCLUDED

#include "SCSS_Attributes.cginc"

VertexOutput vert(appdata_full v) {
	VertexOutput o = (VertexOutput)0;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    // Simple inventory
    float inventoryMask = getInventoryMask(v.texcoord);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv0 = AnimateTexcoords(v.texcoord);
	o.uv1 = v.texcoord1;
	o.normal = v.normal;
	o.normalDir = UnityObjectToWorldNormal(v.normal);
	o.tangentDir = UnityObjectToWorldDir(v.tangent.xyz);
    half sign = v.tangent.w * unity_WorldTransformParams.w;
	o.bitangentDir = cross(o.normalDir, o.tangentDir) * sign;
	float4 objPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
	o.posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.vertex = v.vertex;

	// This is mainly needed when Blender mangles vertex colour values.
	// If you notice major differences in vertex colour behaviour to expectations, try this.
	if (false) v.color.rgb = GammaToLinearSpace(v.color.rgb);

	// Extra data handling
	// X: Outline width | Y: Ramp softness
	// Z: Outline Z offset | 
	switch (_VertexColorType) 
	{
		case 2: // Additional data
		o.color = 1.0; // Reset
		o.extraData = v.color;
		break;

		case 3: // None
		o.color = 1.0; 
		o.extraData = float4(1.0, 0.0, 1.0, 1.0); 
		break;

		default:
		o.color = v.color;
		o.extraData = float4(0.0, 0.0, 1.0, 1.0); 
		o.extraData.x = v.color.a;
		break;
	}

	#if defined(SCSS_OUTLINE)
	#if defined(SCSS_USE_OUTLINE_TEXTURE)
	o.extraData.x *= OutlineMask(v.texcoord.xy);
	#endif

	o.extraData.x *= _outline_width * .01; // Apply outline width and convert to cm
	o.extraData.z *= (1 - _OutlineZPush * 0.1); // Apply outline push parameter.
	
	// Scale outlines relative to the distance from the camera. Outlines close up look ugly in VR because
	// they can have holes, being shells. This is also why it is clamped to not make them bigger.
	// That looks good at a distance, but not perfect. 
	o.extraData.x *= min(distance(o.posWorld,_WorldSpaceCameraPos)*4, 1); 
	#else
	// Remove outline data when no outline present.
	o.extraData.xz = 0.0;
	#endif

	// Apply the inventory mask.
    // Set the output variables based on the mask to completely remove it.
    // - Set the clip-space position to one that won't be rendered
    // - Set the vertex alpha to zero
    // - Disable outlines
    if (_UseInventory)
    {
		o.pos.z =     inventoryMask ? o.pos.z : 1e+9;
		o.posWorld =  inventoryMask ? o.posWorld : 0;
		o.vertex =    inventoryMask ? o.vertex : 1e+9;
		o.color.a =   inventoryMask ? o.color.a : -1;
		o.extraData.xz = inventoryMask ? o.extraData.xz : 0;
    }

#if (UNITY_VERSION<600)
	TRANSFER_SHADOW(o);
#else
	UNITY_TRANSFER_SHADOW(o, v.texcoord);
#endif

#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	UNITY_TRANSFER_FOG(o, o.pos);
#endif

#if defined(VERTEXLIGHT_ON)
	o.vertexLight = VertexLightContribution(o.posWorld, o.normalDir);
#endif

	return o;
}

VertexOutput vert_nogeom(appdata_full v) {
	VertexOutput o = (VertexOutput)0;

	o = vert(v);
	
	o.extraData.x = false;
	return o;
}

// Based on code from MToon
inline VertexOutput CalculateOutlineVertexClipPosition(VertexOutput v)
{
	const float outlineWidth = v.extraData.r;
	if (true)
	{
        const float3 positionWS = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
        const half3 normalWS = v.normalDir;

        v.posWorld = float4(positionWS + normalWS * outlineWidth, 1);
        v.pos = UnityWorldToClipPos(v.posWorld);
	} 
	if (false) 
	{
        const float3 positionWS = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
        const half aspect = getScreenAspectRatio();

        float4 positionCS = UnityObjectToClipPos(v.vertex.xyz);
        const half3 normalVS = getObjectToViewNormal(v.normal.xyz);
        const half3 normalCS = TransformViewToProjection(normalVS.xyz);
        half2 normalProjectedCS = normalize(normalCS.xy);
        normalProjectedCS *= positionCS.w;
        normalProjectedCS.x *= aspect;
        positionCS.xy += outlineWidth * normalProjectedCS.xy * saturate(1 - abs(normalVS.z)); // ignore offset when normal toward camera

        v.posWorld = float4(positionWS, 1);
        v.pos = positionCS;
	}
	return v;
}

[maxvertexcount(6)]
void geom(triangle VertexOutput IN[3], inout TriangleStream<VertexOutput> tristream)
{
    #if defined(UNITY_REVERSED_Z)
        const float far_clip_value_raw = 0.0;
    #else
        const float far_clip_value_raw = 1.0;
    #endif

	if ((IN[0].color.a + IN[1].color.a + IN[2].color.a) >= 0)
	{
		// Generate base vertex
		[unroll]
		for (int ii = 0; ii < 3; ii++)
		{
			VertexOutput o = IN[ii];
			o.extraData.x = false;

			tristream.Append(o);
		}

		tristream.RestartStrip();

		// Generate outline vertex
		// If the outline triangle is too small, don't emit it.
		if ((IN[0].extraData.r + IN[1].extraData.r + IN[2].extraData.r) >= 1.e-9)
		{
			[unroll]
			for (int i = 2; i >= 0; i--)
			{
				VertexOutput o = IN[i];

				// Single-pass instancing compatibility
	    		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(o); 
			    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				//o.pos = UnityObjectToClipPos(o.vertex + normalize(o.normal) * o.extraData.r);
				o = CalculateOutlineVertexClipPosition(o);

				// Possible future parameter depending on what people need
				float zPushLimit = lerp(far_clip_value_raw, o.pos.z, 0.9);
				o.pos.z = lerp(zPushLimit, o.pos.z, o.extraData.z);

				o.extraData.x = true;

				tristream.Append(o);
			}

			tristream.RestartStrip();
		}
	}
}
/*
void computeShadingParamsForward(inout ShadingParams shading, VertexOutput i)
{
    float3x3 tangentToWorld;
    tangentToWorld[0] = i.tangentToWorldAndPackedData[0].xyz;
    tangentToWorld[1] = i.tangentToWorldAndPackedData[1].xyz;
    tangentToWorld[2] = i.tangentToWorldAndPackedData[2].xyz;
    shading.tangentToWorld = transpose(tangentToWorld);
    shading.geometricNormal = normalize(i.tangentToWorldAndPackedData[2].xyz);

    shading.normalizedViewportCoord = i.pos.xy * (0.5 / i.pos.w) + 0.5;

    shading.normal = (shading.geometricNormal);
    shading.position = IN_WORLDPOS(i);
    shading.view = -NormalizePerPixelNormal(i.eyeVec);

    UNITY_LIGHT_ATTENUATION(atten, i, shading.position)
    shading.attenuation = atten;

    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    GetBakedAttenuation(atten, i.ambientOrLightmapUV.xy, shading.position);
    #endif

    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        shading.ambient = 0;
        shading.lightmapUV = i.ambientOrLightmapUV;
    #else
        shading.ambient = i.ambientOrLightmapUV.rgb;
        shading.lightmapUV = 0;
    #endif
}
*/

float4 frag(VertexOutput i, uint facing : SV_IsFrontFace
    #if defined(USING_COVERAGE_OUTPUT)
	, out uint cov : SV_Coverage
	#endif
	) : SV_Target
{
	float isOutline = i.extraData.x;
	if (isOutline && !facing) discard;

    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

	// Backface correction. If a polygon is facing away from the camera, it's lit incorrectly.
	// This will light it as though it is facing the camera (which it visually is), unless
	// it's part of an outline, in which case it's invalid and deleted. 
	//facing = backfaceInMirror()? !facing : facing; // Only needed for older Unity versions.
	if (!facing) 
	{
		i.normalDir *= -1;
		i.tangentDir *= -1;
		i.bitangentDir *= -1;
	}

    float outlineDarken = 1-isOutline;

	float4 texcoords = TexCoords(i.uv0, i.uv1);

	// Ideally, we should pass all input to lighting functions through the 
	// material parameter struct. But there are some things that are
	// optional. Review this at a later date...

	SCSS_Input c = (SCSS_Input) 0;
	initMaterial(c);

	c.alpha = Alpha(texcoords.xy);

    #if defined(ALPHAFUNCTION)
    alphaFunction(c.alpha);
	#endif

	applyVanishing(c.alpha);
	
	applyAlphaClip(c.alpha, _Cutoff, i.pos.xy, _AlphaSharp);

	half detailMask = DetailMask(texcoords.xy);

    half3 normalTangent = NormalInTangentSpace(texcoords, detailMask);

    // Thanks, Xiexe!
    half3 tspace0 = half3(i.tangentDir.x, i.bitangentDir.x, i.normalDir.x);
    half3 tspace1 = half3(i.tangentDir.y, i.bitangentDir.y, i.normalDir.y);
    half3 tspace2 = half3(i.tangentDir.z, i.bitangentDir.z, i.normalDir.z);

    half3 calcedNormal;
    calcedNormal.x = dot(tspace0, normalTangent);
    calcedNormal.y = dot(tspace1, normalTangent);
    calcedNormal.z = dot(tspace2, normalTangent);
    
    calcedNormal = normalize(calcedNormal);
    half3 bumpedTangent = (cross(i.bitangentDir, calcedNormal));
    half3 bumpedBitangent = (cross(calcedNormal, bumpedTangent));

    // For our purposes, we'd like to keep the original normal in i, but warp the bi/tangents.
    c.normal = calcedNormal;
    i.tangentDir = bumpedTangent;
    i.bitangentDir = bumpedBitangent;

	c.albedo = Albedo(texcoords);

	#if !defined(SCSS_CROSSTONE)
	c.tone[0] = Tonemap(texcoords.xy, c.occlusion);
	#endif

	#if defined(SCSS_CROSSTONE)
	c.tone[0] = Tonemap1st(texcoords.xy);
	c.tone[1] = Tonemap2nd(texcoords.xy);
	c.occlusion = ShadingGradeMap(texcoords.xy);
	#endif

	c = applyDetail(c, texcoords);
	c = applyVertexColour(c, i.color, isOutline);

	c.emission = Emission(texcoords.xy);
	
	c.softness = i.extraData.g;

	c = applyOutline(c, isOutline);

    // Rim lighting parameters. 
	c.rim = initialiseRimParam();
	c.rim.alpha *= RimMask(texcoords.xy);
	c.rim.invAlpha *= RimMask(texcoords.xy);
	c.rim.tint *= outlineDarken;

	// Scattering parameters
	c.thickness = Thickness(texcoords.xy);

	// Specular variable setup

	// Disable PBR dielectric setup in cel specular mode.
	#if defined(_SPECGLOSSMAP)
	#undef unity_ColorSpaceDielectricSpec
	#define unity_ColorSpaceDielectricSpec half4(0, 0, 0, 1)
	#endif 

	//if (_SpecularType != 0 )
	#if defined(_SPECULAR)
	{
		half4 specGloss = SpecularGloss(texcoords, detailMask);

		c.specColor = specGloss.rgb;
		c.smoothness = specGloss.a;

		if (_UseMetallic == 1)
		{
			// In Metallic mode, ignore the other colour channels. 
			c.specColor = c.specColor.r;
		}

		// Because specular behaves poorly on backfaces, disable specular on outlines. 
		c.specColor  *= outlineDarken;
		c.smoothness *= outlineDarken;

		// Specular energy converservation. From EnergyConservationBetweenDiffuseAndSpecular in UnityStandardUtils.cginc
		c.oneMinusReflectivity = 1 - SpecularStrength(c.specColor); 

		if (_UseMetallic == 1)
		{
			// From DiffuseAndSpecularFromMetallic
			c.oneMinusReflectivity = OneMinusReflectivityFromMetallic(c.specColor);
			c.specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, c.albedo, c.specColor);
		}

		if (_UseEnergyConservation == 1)
		{
			c.albedo.xyz = c.albedo.xyz * (c.oneMinusReflectivity); 
			if (_CrosstoneToneSeparation) c.tone[0].col = c.tone[0].col * (c.oneMinusReflectivity); 
			if (_Crosstone2ndSeparation)  c.tone[1].col = c.tone[1].col * (c.oneMinusReflectivity); 
		}

	    i.tangentDir = ShiftTangent(normalize(i.tangentDir), c.normal, c.smoothness);
	    i.bitangentDir = normalize(i.bitangentDir);
	}
	#endif

	#if !defined(USING_TRANSPARENCY)
		c.alpha = 1.0;
	#endif

    // When premultiplied mode is set, this will multiply the diffuse by the alpha component,
    // allowing to handle transparency in physically correct way - only diffuse component gets affected by alpha
    half outputAlpha;
    c.albedo = PreMultiplyAlpha (c.albedo, c.alpha, c.oneMinusReflectivity, /*out*/ outputAlpha);

	// Lighting handling
	float3 finalColor = SCSS_ApplyLighting(c, i, texcoords);

	float3 lightmap = float4(1.0,1.0,1.0,1.0);
	#if defined(LIGHTMAP_ON)
		lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv1 * unity_LightmapST.xy + unity_LightmapST.zw));
	#endif

    #if defined(USING_COVERAGE_OUTPUT)
    // Get the amount of MSAA samples enabled
    uint samplecount = GetRenderTargetSampleCount();

    // center out the steps
    outputAlpha = saturate(outputAlpha) * samplecount + 0.5;

    // Shift and subtract to get the needed amount of positive bits
    cov = (1u << (uint)(outputAlpha)) - 1u;

    // Output 1 as alpha, otherwise result would be a^2
	outputAlpha = 1;
	#endif

	fixed4 finalRGBA = fixed4(finalColor * lightmap, outputAlpha);
	UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
	return finalRGBA;
}

#endif // SCSS_FORWARD_INCLUDED