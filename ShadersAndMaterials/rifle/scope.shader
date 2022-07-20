// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:32719,y:32712,varname:node_2865,prsc:2|diff-6343-OUT,spec-358-OUT,gloss-1813-OUT,normal-5964-RGB;n:type:ShaderForge.SFN_Multiply,id:6343,x:32339,y:32534,varname:node_6343,prsc:2|A-7736-RGB,B-6071-OUT,C-748-OUT,D-7615-RGB;n:type:ShaderForge.SFN_Tex2d,id:7736,x:31894,y:32244,ptovrint:True,ptlb:Base Color,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1491-OUT;n:type:ShaderForge.SFN_Tex2d,id:5964,x:32407,y:32978,ptovrint:True,ptlb:Normal Map,ptin:_BumpMap,varname:_BumpMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Slider,id:358,x:32250,y:32766,ptovrint:False,ptlb:Metallic,ptin:_Metallic,varname:_Metallic,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_Slider,id:1813,x:32250,y:32882,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Gloss,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8,max:1;n:type:ShaderForge.SFN_Tex2d,id:108,x:31485,y:32598,ptovrint:False,ptlb:scope,ptin:_scope,varname:_scope,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1491-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:5012,x:31759,y:32778,varname:node_5012,prsc:2|IN-108-RGB,IMIN-6727-OUT,IMAX-7049-OUT,OMIN-5962-OUT,OMAX-4112-OUT;n:type:ShaderForge.SFN_Vector1,id:6727,x:30941,y:32780,varname:node_6727,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:7049,x:30941,y:32716,varname:node_7049,prsc:2,v1:1;n:type:ShaderForge.SFN_Slider,id:4112,x:31375,y:33150,ptovrint:False,ptlb:scope_max,ptin:_scope_max,varname:_scope_max,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:100;n:type:ShaderForge.SFN_Slider,id:70,x:30922,y:33290,ptovrint:False,ptlb:scope_min,ptin:_scope_min,varname:_scope_min,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:100;n:type:ShaderForge.SFN_Clamp01,id:6071,x:31919,y:32862,varname:node_6071,prsc:2|IN-5012-OUT;n:type:ShaderForge.SFN_ObjectPosition,id:978,x:30132,y:32525,varname:node_978,prsc:2;n:type:ShaderForge.SFN_ViewPosition,id:2776,x:30163,y:32803,varname:node_2776,prsc:2;n:type:ShaderForge.SFN_Distance,id:4620,x:30438,y:32795,varname:node_4620,prsc:2|A-978-XYZ,B-2776-XYZ;n:type:ShaderForge.SFN_TexCoord,id:8774,x:30527,y:32251,varname:node_8774,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:1159,x:30886,y:32341,varname:node_1159,prsc:2|IN-8774-U,IMIN-6727-OUT,IMAX-7049-OUT,OMIN-9178-OUT,OMAX-1979-OUT;n:type:ShaderForge.SFN_Append,id:1001,x:31158,y:32274,varname:node_1001,prsc:2|A-1159-OUT,B-5284-OUT;n:type:ShaderForge.SFN_Negate,id:1979,x:30545,y:32410,varname:node_1979,prsc:2|IN-9178-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:5284,x:30886,y:32194,varname:node_5284,prsc:2|IN-8774-V,IMIN-6727-OUT,IMAX-7049-OUT,OMIN-9178-OUT,OMAX-1979-OUT;n:type:ShaderForge.SFN_Parallax,id:5519,x:31409,y:32347,varname:node_5519,prsc:2|UVIN-1001-OUT,HEI-1586-OUT,DEP-9178-OUT;n:type:ShaderForge.SFN_Slider,id:1586,x:31044,y:32411,ptovrint:False,ptlb:height,ptin:_height,varname:_height,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-100,cur:0,max:100;n:type:ShaderForge.SFN_Tex2d,id:4403,x:31504,y:32805,ptovrint:False,ptlb:scope_2nd,ptin:_scope_2nd,varname:_scope_dist,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1902-UVOUT;n:type:ShaderForge.SFN_Negate,id:7471,x:30587,y:33059,varname:node_7471,prsc:2|IN-4620-OUT;n:type:ShaderForge.SFN_Add,id:5962,x:31148,y:33055,varname:node_5962,prsc:2|A-7939-OUT,B-70-OUT;n:type:ShaderForge.SFN_Add,id:8369,x:30785,y:33010,varname:node_8369,prsc:2|A-7471-OUT,B-7471-OUT;n:type:ShaderForge.SFN_Add,id:2638,x:30785,y:33137,varname:node_2638,prsc:2|A-8369-OUT,B-8369-OUT,C-8369-OUT,D-8369-OUT,E-8369-OUT;n:type:ShaderForge.SFN_TexCoord,id:2121,x:29816,y:32922,varname:node_2121,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Vector1,id:7438,x:36562,y:31693,varname:node_7438,prsc:2,v1:1;n:type:ShaderForge.SFN_Lerp,id:5661,x:36665,y:31578,varname:node_5661,prsc:2|A-7438-OUT,B-8557-RGB;n:type:ShaderForge.SFN_Multiply,id:7536,x:36881,y:31556,varname:node_7536,prsc:2|B-5661-OUT;n:type:ShaderForge.SFN_AmbientLight,id:8557,x:36491,y:31804,varname:node_8557,prsc:2;n:type:ShaderForge.SFN_Vector1,id:9101,x:31919,y:32745,varname:node_9101,prsc:2,v1:1;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:9178,x:30585,y:32571,varname:node_9178,prsc:2|IN-4620-OUT,IMIN-6727-OUT,IMAX-7049-OUT,OMIN-7816-OUT,OMAX-5062-OUT;n:type:ShaderForge.SFN_Slider,id:7816,x:29850,y:32616,ptovrint:False,ptlb:min,ptin:_min,varname:node_7816,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-50,cur:0,max:50;n:type:ShaderForge.SFN_Slider,id:5062,x:29816,y:32734,ptovrint:False,ptlb:max,ptin:_max,varname:_min_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-50,cur:0,max:50;n:type:ShaderForge.SFN_Add,id:7939,x:30949,y:33137,varname:node_7939,prsc:2|A-2638-OUT,B-8369-OUT,C-8369-OUT,D-8369-OUT;n:type:ShaderForge.SFN_Parallax,id:1902,x:31175,y:32831,varname:node_1902,prsc:2|UVIN-2121-UVOUT,HEI-3272-OUT;n:type:ShaderForge.SFN_Lerp,id:3061,x:31979,y:32497,varname:node_3061,prsc:2|A-7182-OUT,B-2109-OUT,T-4403-R;n:type:ShaderForge.SFN_Slider,id:2109,x:32022,y:32212,ptovrint:False,ptlb:2nd_scope_max,ptin:_2nd_scope_max,varname:node_2109,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:1,max:10;n:type:ShaderForge.SFN_Slider,id:3272,x:30628,y:32887,ptovrint:False,ptlb:height_2nd,ptin:_height_2nd,varname:_height_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-100,cur:0,max:100;n:type:ShaderForge.SFN_Clamp01,id:748,x:32147,y:32497,varname:node_748,prsc:2|IN-3061-OUT;n:type:ShaderForge.SFN_Slider,id:7182,x:32080,y:32383,ptovrint:False,ptlb:2nd_scope_sharp_min,ptin:_2nd_scope_sharp_min,varname:_2nd_scope_sharp_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:0,max:10;n:type:ShaderForge.SFN_Clamp,id:1491,x:31653,y:32294,varname:node_1491,prsc:2|IN-5519-UVOUT,MIN-1539-OUT,MAX-3357-OUT;n:type:ShaderForge.SFN_Slider,id:1539,x:31282,y:32161,ptovrint:False,ptlb:1,ptin:_1,varname:node_1539,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-100,cur:1,max:100;n:type:ShaderForge.SFN_Slider,id:3357,x:31300,y:32251,ptovrint:False,ptlb:2,ptin:_2,varname:_2,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-100,cur:1,max:100;n:type:ShaderForge.SFN_Tex2d,id:7615,x:31775,y:32630,ptovrint:False,ptlb:lines,ptin:_lines,varname:node_7615,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-1491-OUT;proporder:5964-7736-358-1813-108-4112-70-1586-4403-5062-7816-2109-3272-7182-1539-3357-7615;pass:END;sub:END;*/

Shader "Shader Forge/scope" {
    Properties {
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _MainTex ("Base Color", 2D) = "white" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Gloss ("Gloss", Range(0, 1)) = 0.8
        _scope ("scope", 2D) = "white" {}
        _scope_max ("scope_max", Range(0, 100)) = 1
        _scope_min ("scope_min", Range(0, 100)) = 0
        _height ("height", Range(-100, 100)) = 0
        _scope_2nd ("scope_2nd", 2D) = "white" {}
        _max ("max", Range(-50, 50)) = 0
        _min ("min", Range(-50, 50)) = 0
        _2nd_scope_max ("2nd_scope_max", Range(-10, 10)) = 1
        _height_2nd ("height_2nd", Range(-100, 100)) = 0
        _2nd_scope_sharp_min ("2nd_scope_sharp_min", Range(-10, 10)) = 0
        _1 ("1", Range(-100, 100)) = 1
        _2 ("2", Range(-100, 100)) = 1
        _lines ("lines", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform sampler2D _scope; uniform float4 _scope_ST;
            uniform float _scope_max;
            uniform float _scope_min;
            uniform float _height;
            uniform sampler2D _scope_2nd; uniform float4 _scope_2nd_ST;
            uniform float _min;
            uniform float _max;
            uniform float _2nd_scope_max;
            uniform float _height_2nd;
            uniform float _2nd_scope_sharp_min;
            uniform float _1;
            uniform float _2;
            uniform sampler2D _lines; uniform float4 _lines_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float perceptualRoughness = 1.0 - _Gloss;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = _Metallic;
                float specularMonochrome;
                float node_6727 = 0.0;
                float node_7049 = 1.0;
                float node_4620 = distance(objPos.rgb,_WorldSpaceCameraPos);
                float node_9178 = (_min + ( (node_4620 - node_6727) * (_max - _min) ) / (node_7049 - node_6727));
                float node_1979 = (-1*node_9178);
                float2 node_1491 = clamp((node_9178*(_height - 0.5)*mul(tangentTransform, viewDirection).xy + float2((node_9178 + ( (i.uv0.r - node_6727) * (node_1979 - node_9178) ) / (node_7049 - node_6727)),(node_9178 + ( (i.uv0.g - node_6727) * (node_1979 - node_9178) ) / (node_7049 - node_6727)))).rg,_1,_2);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_1491, _MainTex));
                float4 _scope_var = tex2D(_scope,TRANSFORM_TEX(node_1491, _scope));
                float node_7471 = (-1*node_4620);
                float node_8369 = (node_7471+node_7471);
                float node_5962 = (((node_8369+node_8369+node_8369+node_8369+node_8369)+node_8369+node_8369+node_8369)+_scope_min);
                float2 node_1902 = (0.05*(_height_2nd - 0.5)*mul(tangentTransform, viewDirection).xy + i.uv0);
                float4 _scope_2nd_var = tex2D(_scope_2nd,TRANSFORM_TEX(node_1902.rg, _scope_2nd));
                float4 _lines_var = tex2D(_lines,TRANSFORM_TEX(node_1491, _lines));
                float3 diffuseColor = (_MainTex_var.rgb*saturate((node_5962 + ( (_scope_var.rgb - node_6727) * (_scope_max - node_5962) ) / (node_7049 - node_6727)))*saturate(lerp(_2nd_scope_sharp_min,_2nd_scope_max,_scope_2nd_var.r))*_lines_var.rgb); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular);
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform sampler2D _scope; uniform float4 _scope_ST;
            uniform float _scope_max;
            uniform float _scope_min;
            uniform float _height;
            uniform sampler2D _scope_2nd; uniform float4 _scope_2nd_ST;
            uniform float _min;
            uniform float _max;
            uniform float _2nd_scope_max;
            uniform float _height_2nd;
            uniform float _2nd_scope_sharp_min;
            uniform float _1;
            uniform float _2;
            uniform sampler2D _lines; uniform float4 _lines_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float perceptualRoughness = 1.0 - _Gloss;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = _Metallic;
                float specularMonochrome;
                float node_6727 = 0.0;
                float node_7049 = 1.0;
                float node_4620 = distance(objPos.rgb,_WorldSpaceCameraPos);
                float node_9178 = (_min + ( (node_4620 - node_6727) * (_max - _min) ) / (node_7049 - node_6727));
                float node_1979 = (-1*node_9178);
                float2 node_1491 = clamp((node_9178*(_height - 0.5)*mul(tangentTransform, viewDirection).xy + float2((node_9178 + ( (i.uv0.r - node_6727) * (node_1979 - node_9178) ) / (node_7049 - node_6727)),(node_9178 + ( (i.uv0.g - node_6727) * (node_1979 - node_9178) ) / (node_7049 - node_6727)))).rg,_1,_2);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_1491, _MainTex));
                float4 _scope_var = tex2D(_scope,TRANSFORM_TEX(node_1491, _scope));
                float node_7471 = (-1*node_4620);
                float node_8369 = (node_7471+node_7471);
                float node_5962 = (((node_8369+node_8369+node_8369+node_8369+node_8369)+node_8369+node_8369+node_8369)+_scope_min);
                float2 node_1902 = (0.05*(_height_2nd - 0.5)*mul(tangentTransform, viewDirection).xy + i.uv0);
                float4 _scope_2nd_var = tex2D(_scope_2nd,TRANSFORM_TEX(node_1902.rg, _scope_2nd));
                float4 _lines_var = tex2D(_lines,TRANSFORM_TEX(node_1491, _lines));
                float3 diffuseColor = (_MainTex_var.rgb*saturate((node_5962 + ( (_scope_var.rgb - node_6727) * (_scope_max - node_5962) ) / (node_7049 - node_6727)))*saturate(lerp(_2nd_scope_sharp_min,_2nd_scope_max,_scope_2nd_var.r))*_lines_var.rgb); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Metallic;
            uniform float _Gloss;
            uniform sampler2D _scope; uniform float4 _scope_ST;
            uniform float _scope_max;
            uniform float _scope_min;
            uniform float _height;
            uniform sampler2D _scope_2nd; uniform float4 _scope_2nd_ST;
            uniform float _min;
            uniform float _max;
            uniform float _2nd_scope_max;
            uniform float _height_2nd;
            uniform float _2nd_scope_sharp_min;
            uniform float _1;
            uniform float _2;
            uniform sampler2D _lines; uniform float4 _lines_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                float4 objPos = mul ( unity_ObjectToWorld, float4(0,0,0,1) );
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float node_6727 = 0.0;
                float node_7049 = 1.0;
                float node_4620 = distance(objPos.rgb,_WorldSpaceCameraPos);
                float node_9178 = (_min + ( (node_4620 - node_6727) * (_max - _min) ) / (node_7049 - node_6727));
                float node_1979 = (-1*node_9178);
                float2 node_1491 = clamp((node_9178*(_height - 0.5)*mul(tangentTransform, viewDirection).xy + float2((node_9178 + ( (i.uv0.r - node_6727) * (node_1979 - node_9178) ) / (node_7049 - node_6727)),(node_9178 + ( (i.uv0.g - node_6727) * (node_1979 - node_9178) ) / (node_7049 - node_6727)))).rg,_1,_2);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_1491, _MainTex));
                float4 _scope_var = tex2D(_scope,TRANSFORM_TEX(node_1491, _scope));
                float node_7471 = (-1*node_4620);
                float node_8369 = (node_7471+node_7471);
                float node_5962 = (((node_8369+node_8369+node_8369+node_8369+node_8369)+node_8369+node_8369+node_8369)+_scope_min);
                float2 node_1902 = (0.05*(_height_2nd - 0.5)*mul(tangentTransform, viewDirection).xy + i.uv0);
                float4 _scope_2nd_var = tex2D(_scope_2nd,TRANSFORM_TEX(node_1902.rg, _scope_2nd));
                float4 _lines_var = tex2D(_lines,TRANSFORM_TEX(node_1491, _lines));
                float3 diffColor = (_MainTex_var.rgb*saturate((node_5962 + ( (_scope_var.rgb - node_6727) * (_scope_max - node_5962) ) / (node_7049 - node_6727)))*saturate(lerp(_2nd_scope_sharp_min,_2nd_scope_max,_scope_2nd_var.r))*_lines_var.rgb);
                float specularMonochrome;
                float3 specColor;
                diffColor = DiffuseAndSpecularFromMetallic( diffColor, _Metallic, specColor, specularMonochrome );
                float roughness = 1.0 - _Gloss;
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
