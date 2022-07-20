#ifndef SCSS_ATTRIBUTES_INCLUDED
#define SCSS_ATTRIBUTES_INCLUDED

#include "UnityCG.cginc"
#include "AutoLight.cginc"

struct VertexOutput
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO

	UNITY_POSITION(pos);
	float3 normal : NORMAL;
	fixed4 color : COLOR0_centroid;
	float2 uv0 : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float4 posWorld : TEXCOORD2;
	float3 normalDir : TEXCOORD3;
	float3 tangentDir : TEXCOORD4;
	float3 bitangentDir : TEXCOORD5;
	float4 vertex : VERTEX;

	#if defined(VERTEXLIGHT_ON)
	half4 vertexLight : TEXCOORD6;
	#endif

	half4 extraData : EXTRA_DATA;

	// Pass-through the shadow coordinates if this pass has shadows.
	// Note the workaround for UNITY_SHADOW_COORDS issue. 
	#if defined(USING_SHADOWS_UNITY) && defined(UNITY_SHADOW_COORDS)
	UNITY_SHADOW_COORDS(8)
	#endif

	// Pass-through the fog coordinates if this pass has fog.
	#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	UNITY_FOG_COORDS(9)
	#endif
};

struct FragmentInput
{
	VertexOutput i; 
	uint facing : SV_IsFrontFace;
};

struct FragmentOutput
{
	float4 color : SV_Target;
	uint coverage : SV_Coverage;
};

#endif // SCSS_ATTRIBUTES_INCLUDED