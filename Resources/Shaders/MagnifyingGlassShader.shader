Shader "Custom/MagnifyingShader"
{
	Properties
	{
		_Magnification("Magnification", Float) = 2
        _MainTex("Albedo", 2D) = "white" {}
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "PreviewType" = "Plane" }
		LOD 100

		// GrabPass{ "_GrabTexture" }

		Pass
	        {
		        ZTest On
		        ZWrite Off
		        Blend One Zero
		        Lighting Off
		        Fog{ Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				//our UV coordinate on the GrabTexture
				float4 uv : TEXCOORD0;
				//our vertex position after projection
				float4 vertex : SV_POSITION;
			};

			// sampler2D _GrabTexture;
			sampler2D _MainTex;
			half _Magnification;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//the UV coordinate of our object's center on the GrabTexture
				float4 uv_center = ComputeGrabScreenPos(UnityObjectToClipPos(float4(0, 0, 0, 1)));
				//the vector from uv_center to our UV coordinate on the GrabTexture
				float4 uv_diff = ComputeGrabScreenPos(o.vertex) - uv_center;
				//apply magnification
				uv_diff /= _Magnification;
				//save result
				o.uv = uv_center + uv_diff;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				return tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uv));
			}
			ENDCG
		}
	}
}