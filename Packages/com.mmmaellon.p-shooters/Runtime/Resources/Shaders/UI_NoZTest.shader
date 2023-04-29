// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "UI/Default_OverlayNoZTest"
 {
     Properties
     {
         _MainTex ("Sprite Texture", 2D) = "white" {}
         _Color ("Tint", Color) = (1,1,1,1)
         
         [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
         _Stencil ("Stencil ID", Float) = 0
         [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
         _StencilWriteMask ("Stencil Write Mask", Float) = 255
         _StencilReadMask ("Stencil Read Mask", Float) = 255
         _MinDistance ("Minimum Distance", float) = 1
         _MaxDistance ("Maximum Distance", float) = 100
         _ZOffset ("Z Offset", float) = -1
         _Scale ("Scale", float) = 1.0
         [MaterialToggle] _Billboard ("Billboard", int) = 0
        //  Property
         [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4 //"LessEqual"
         
         _ColorMask ("Color Mask", Float) = 15
     }
 
     SubShader
     {
         Tags
         { 
             "Queue"="Overlay" 
             "IgnoreProjector"="True" 
             "RenderType"="Transparent" 
             "PreviewType"="Plane"
             "CanUseSpriteAtlas"="True"
             "DisableBatching" = "True"
         }
         
         Stencil
         {
             Ref [_Stencil]
             Comp [_StencilComp]
             Pass [_StencilOp] 
             Fail [_StencilFail]
             ZFail [_StencilZFail]
             ReadMask [_StencilReadMask]
             WriteMask [_StencilWriteMask]
         }
 
         Cull Off
         Lighting Off
         ZWrite Off
         ZTest [_ZTest]
         Blend SrcAlpha OneMinusSrcAlpha
         ColorMask [_ColorMask]
 
         Pass
         {
         CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_instancing
             #include "UnityCG.cginc"
             
             struct appdata_t
             {
                 float4 vertex   : POSITION;
                 float4 color    : COLOR;
                 float2 texcoord : TEXCOORD0;
                 UNITY_VERTEX_INPUT_INSTANCE_ID
             };
 
             struct v2f
             {
                 float4 vertex   : SV_POSITION;
                 fixed4 color    : COLOR;
                 half2 texcoord  : TEXCOORD0;
             };
             
             fixed4 _Color;
             float _MinDistance;
             float _MaxDistance;
             float _ZOffset;
             float _Scale;
             fixed4 _TextureSampleAdd; //Added for font color support
             fixed _Billboard;
 
             v2f vert(appdata_t IN)
             {
                 v2f OUT;
                 UNITY_SETUP_INSTANCE_ID(IN);
                //  OUT.vertex = UnityObjectToClipPos(IN.vertex);
                 OUT.texcoord = IN.texcoord;
                // billboard mesh towards camera
                // center camera position
                #ifdef UNITY_SINGLE_PASS_STEREO
                float3 camPos = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
                #else
                float3 camPos = _WorldSpaceCameraPos;
                #endif
                // world space mesh pivot
                float3 worldPivot = unity_ObjectToWorld._m03_m13_m23;
                // x & y axis scales only
                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m01_m02) * _Scale,
                    length(unity_ObjectToWorld._m10_m11_m12) * _Scale,
                    1.0
                    );
                // calculate billboard rotation matrix
                float3 f = normalize(lerp(
                    -UNITY_MATRIX_V[2].xyz, // view forward dir
                    normalize(worldPivot - camPos), // camera to pivot dir
                    _Billboard));
                float3 u = float3(0.0,1.0,0.0);
                float3 r = normalize(cross(u, f));
                u = -normalize(cross(r, f));
                float3x3 billboardRotation = float3x3(r, u, f);
 
                // apply scale, rotation, and translation to billboard
                float3 worldPos = mul(IN.vertex.xyz * scale, billboardRotation) + worldPivot + normalize(worldPivot - camPos) * _ZOffset;
 
                // transform into clip space
                OUT.vertex = _Billboard == 0 ? UnityObjectToClipPos(IN.vertex) : UnityWorldToClipPos(worldPos);
                
				// float3 vpos = mul((float3x3)unity_ObjectToWorld, IN.vertex.xyz);
				// float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
				// float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
				// float4 outPos = mul(UNITY_MATRIX_P, viewPos);
                // OUT.vertex = _Billboard == 0 ? UnityObjectToClipPos(IN.vertex) : outPos;
                
 #ifdef UNITY_HALF_TEXEL_OFFSET
                 OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
 #endif
 
                 OUT.color = IN.color * _Color;
                 float4 worldpos = mul(unity_ObjectToWorld, IN.vertex);
                 float distanceFromCamera = distance(worldpos, _WorldSpaceCameraPos);
                 float fade = saturate((distanceFromCamera - _MinDistance) / (_MaxDistance - _MinDistance));
                 OUT.color.a = OUT.color.a * fade;
                 
                 return OUT;
             }
 
             sampler2D _MainTex;
 
             fixed4 frag(v2f IN) : SV_Target
             {
                 half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;  //Added for font color support
                 clip (color.a - 0.01);
                 return color;
             }
         ENDCG
         }
     }
 }