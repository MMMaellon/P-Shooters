// Common fake light code.

// Handle missing definitions
#ifndef LIGHT_COLOUR
	#define LIGHT_COLOUR 1.0
#endif
#ifndef FALLOFF_POWER
	#define FALLOFF_POWER  0.8
#endif

struct Ray
{
    float3 o;
    float3 d;
};

// Original Volumetric Sphere Density by iq
// http://www.iquilezles.org/www/articles/spheredensity/spheredensity.htm
// Unity version by 1001

float sphDensity( Ray ray, float3 sc, float sr, float dbuffer )
{
	float3 ro = ray.o;
	float3 rd = ray.d;
	// normalize the problem to the canonical sphere
	float ndbuffer = dbuffer / sr;
	float3  rc = (ro - sc)/sr;
	
	// find intersection with sphere
	float b = dot(rd,rc);
	float c = dot(rc,rc) - 1.0;
	float h = b*b - c;

	// not intersecting
	if( h<0.0 ) return 0.0;
	
	h = sqrt( h );

	float t1 = -b - h;
	float t2 = -b + h;

	// not visible (behind camera or behind ndbuffer)
	if( t2<0.0 || t1>ndbuffer ) return 0.0;

	// clip integration segment from camera to ndbuffer
	t1 = max( t1, 0.0 );
	t2 = min( t2, ndbuffer );

	// analytical integration of an inverse squared density
	float i1 = -(c*t1 + b*t1*t1 + t1*t1*t1/3.0);
	float i2 = -(c*t2 + b*t2*t2 + t2*t2*t2/3.0);
	return (i2-i1)*(3.0/4.0);
}

float renderVolumetricSphere(fixed3 baseWorldPos, fixed3 rayDir, in out float3 depthPosition, 
	in float scale = 1.0, in float minimumSize = 0.01) {
	const float zDepth = length(_WorldSpaceCameraPos-depthPosition);
	const float worldScale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
	scale *= worldScale;

	Ray ray = (Ray)0;
	ray.o = _WorldSpaceCameraPos;
	ray.d = rayDir;

	float sum = sphDensity(ray, baseWorldPos, scale, zDepth);

	#if 0 // Old falloff
	sum = sum + sum*sum*sum;
	sum = sum + sum*sum;
	#else
	sum *= 100; 
	minimumSize = lerp(0, 0.2, minimumSize);
	sum = min(sum, 1/minimumSize);
	#endif
	// Not sure if this is useful here
	//sum *= saturate((sum-1)*(1/FALLOFF_POWER));
	return sum;
}

// Fake point lights, Deus Ex: Human Revolution style
// Unity version by Silent, with lots of help from 1001. 

float renderFakeLight(fixed3 baseWorldPos, fixed3 rd, in out float3 depthPosition, 
	in float scale = 1.0, in float minimumSize = 0.01) {
	const float zDepth = length(_WorldSpaceCameraPos-depthPosition);
	const float worldScale = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
	scale *= worldScale;

	// Light falloff
	float lf = distance(baseWorldPos, depthPosition)* 1/scale;
	lf = 1/pow(max(lf, minimumSize), 2.0);

	// Hard sphere falloff
	float3 sf = depthPosition - baseWorldPos;
	sf *= 1/scale;
	float sum = lf; 
	sum *= saturate((1-length(sf))*(1/FALLOFF_POWER));
	return sum;
}


float boxDensity( Ray ray,    			// ray origin, ray direction
                  float4x4 txx, float3 r,     // box center and orientation, box radius
                  float dbuffer )       // depth buffer
{
    float3 d = mul(txx, float4(ray.d,0.0)).xyz;
	float3 o = mul(txx, float4(ray.o,1.0)).xyz;

	// ray-box intersection in box space
    float3 m = 1.0/d;
    float3 n = m*o;
    float3 k = abs(m)*r;
    float3 ta = -n - k;
    float3 tb = -n + k;
	float tN = max( max( ta.x, ta.y ), ta.z );
	float tF = min( min( tb.x, tb.y ), tb.z );
	if( tN > tF || tF < 0.0) return 0.0;

    // not visible (behind camera or behind dbuffer)
    if( tF<0.0 || tN>dbuffer ) return 0.0;

    // clip integration segment from camera to dbuffer
    tN = max( tN, 0.0 );
    tF = min( tF, dbuffer );

    // move ray to the intersection point
    o += tN*d; tF=tF-tN; tN=0.0;

    // density calculation. density is of the form
    //
    // d(x,y,z) = [1-(x/rx)^2] * [1-(y/ry)^2] * [1-(z/rz)^2];
    //
    // this can be analytically integrable (it's a degree 6 polynomial):
    
    float3 a = 1.0 -     (o*o)/(r*r);
    float3 b =     - 2.0*(o*d)/(r*r);
    float3 c =     -     (d*d)/(r*r);
    
    float t1 = tF;
    float t2 = t1*t1;
    float t3 = t2*t1;
    float t4 = t2*t2;
    float t5 = t2*t3;
    float t6 = t3*t3;
    float t7 = t3*t4;

    float f = (t1/1.0) *(a.x*a.y*a.z) + 
              (t2/2.0) *(a.x*a.y*b.z + a.x*b.y*a.z + b.x*a.y*a.z) + 
              (t3/3.0) *(a.x*a.y*c.z + a.x*b.y*b.z + a.x*c.y*a.z + b.x*a.y*b.z + b.x*b.y*a.z + c.x*a.y*a.z) +
              (t4/4.0) *(a.x*b.y*c.z + a.x*c.y*b.z + b.x*a.y*c.z + b.x*b.y*b.z + b.x*c.y*a.z + c.x*a.y*b.z + c.x*b.y*a.z) + 
              (t5/5.0) *(a.x*c.y*c.z + b.x*b.y*c.z + b.x*c.y*b.z + c.x*a.y*c.z + c.x*b.y*b.z + c.x*c.y*a.z) + 
              (t6/6.0) *(b.x*c.y*c.z + c.x*b.y*c.z + c.x*c.y*b.z) + 
              (t7/7.0) *(c.x*c.y*c.z);
      
    return f;
    
}

float boxLight	( Ray ray,    			// ray origin, ray direction
                  float4x4 txx, float3 r,     // box center and orientation, box radius
                  float dbuffer )       // depth buffer
{
    float3 d = mul(txx, float4(ray.d,0.0)).xyz;
	float3 o = mul(txx, float4(ray.o,1.0)).xyz;

	// ray-box intersection in box space
    float3 m = 1.0/d;
    float3 n = m*o;
    float3 k = abs(m)*r;
    float3 ta = -n - k;
    float3 tb = -n + k;
	float tN = max( max( ta.x, ta.y ), ta.z ); // nearRayHitDistance 
	float tF = min( min( tb.x, tb.y ), tb.z ); // farRayHitDistance
	if( tN > tF || tF < 0.0) return 0.0;

    // not visible (behind camera or behind dbuffer)
    if( tF<0.0 || tN>dbuffer ) return 0.0;

    // clip integration segment from camera to dbuffer
    tN = max( tN, 0.0 );

    tF = max(-(dbuffer-tF), 0);
    tF = min( tF, dbuffer );

    // move ray to the intersection point
    o += tN*d; 
    //tF=tF-tN; 
    //tN=0.0;

    // density calculation. density is of the form
    //
    // d(x,y,z) = [1-(x/rx)^2] * [1-(y/ry)^2] * [1-(z/rz)^2];
    //
    // this can be analytically integrable (it's a degree 6 polynomial):
    
    float3 a = 1.0 -     (o*o)/(r*r);
    float3 b =     - 2.0*(o*d)/(r*r);
    float3 c =     -     (d*d)/(r*r);
    
    float t1 = tF;
    float t2 = t1*t1;
    float t3 = t2*t1;
    float t4 = t2*t2;
    float t5 = t2*t3;
    float t6 = t3*t3;
    float t7 = t3*t4;

    float f = (t1/1.0) *(a.x*a.y*a.z) + 
              (t2/2.0) *(a.x*a.y*b.z + a.x*b.y*a.z + b.x*a.y*a.z) + 
              (t3/3.0) *(a.x*a.y*c.z + a.x*b.y*b.z + a.x*c.y*a.z + b.x*a.y*b.z + b.x*b.y*a.z + c.x*a.y*a.z) +
              (t4/4.0) *(a.x*b.y*c.z + a.x*c.y*b.z + b.x*a.y*c.z + b.x*b.y*b.z + b.x*c.y*a.z + c.x*a.y*b.z + c.x*b.y*a.z) + 
              (t5/5.0) *(a.x*c.y*c.z + b.x*b.y*c.z + b.x*c.y*b.z + c.x*a.y*c.z + c.x*b.y*b.z + c.x*c.y*a.z) + 
              (t6/6.0) *(b.x*c.y*c.z + c.x*b.y*c.z + c.x*c.y*b.z) + 
              (t7/7.0) *(c.x*c.y*c.z);
      
    return f;
    
}

float renderVolumetricBox(fixed3 baseWorldPos, fixed3 rayDir, in out float3 depthPosition, 
	in float scale = 1.0, in float minimumSize = 0.01) {
	const float zDepth = length(_WorldSpaceCameraPos-depthPosition);
	scale *= 0.5;

	Ray ray = (Ray)0;
	ray.o = _WorldSpaceCameraPos;
	ray.d = rayDir;

	float sum = boxDensity(ray, unity_WorldToObject, scale, zDepth);

	#if 0 // Old falloff
	sum = sum + sum*sum*sum;
	sum = sum + sum*sum;
	#else
	sum *= 100; 
	minimumSize = lerp(0, 0.2, minimumSize);
	sum = min(sum, 1/minimumSize);
	#endif
	// Not sure if this is useful here
	//sum *= saturate((sum-1)*(1/FALLOFF_POWER));
	return sum;
}

float renderFakeLightBox(fixed3 baseWorldPos, fixed3 rayDir, in out float3 depthPosition, 
	in float scale = 1.0, in float minimumSize = 0.01) {
	const float zDepth = length(_WorldSpaceCameraPos-depthPosition);
	scale *= 0.5;

	Ray ray = (Ray)0;
	ray.o = _WorldSpaceCameraPos;
	ray.d = rayDir;

	float sum = boxLight(ray, unity_WorldToObject, scale, zDepth);

	#if 0 // Old falloff
	sum = sum + sum*sum*sum;
	sum = sum + sum*sum;
	#else
	sum *= 100; 
	minimumSize = lerp(0, 0.2, minimumSize);
	sum = min(sum, 1/minimumSize);
	#endif
	// Not sure if this is useful here
	//sum *= saturate((sum-1)*(1/FALLOFF_POWER));
	return sum;
}

float getVanishingFactor (float start, float end, float distance) {
    // Add near clip plane to start/end so that (0, 0.1) looks right
    start += _ProjectionParams.y;
    end += _ProjectionParams.y;
    float vanishing = saturate(smoothstep(start, end, distance));
    return vanishing;
}

// Dj Lukis LT's method for retrieving linear depth that's correct in mirrors.
// https://github.com/lukis101/VRCUnityStuffs
#define PM UNITY_MATRIX_P
inline float4 CalculateObliqueFrustumCorrection()
{
	float x1 = -PM._31/(PM._11*PM._34);
	float x2 = -PM._32/(PM._22*PM._34);
	return float4(x1, x2, 0, PM._33/PM._34 + x1*PM._13 + x2*PM._23);
}
static float4 ObliqueFrustumCorrection = CalculateObliqueFrustumCorrection();
inline float CorrectedLinearEyeDepth(float z, float B)
{
	return 1.0 / (z/PM._34 + B);
}
#undef PM

// Vertex shader

struct appdata_t {
    float4 vertex : POSITION;
    fixed4 color : COLOR;

    #if defined(PARTICLE_SHADER)
		float4 params : TEXCOORD0;
	#else
    	UNITY_VERTEX_INPUT_INSTANCE_ID
	#endif
};

struct v2f
{
	fixed4 screen_vertex : SV_POSITION;
	fixed4 world_vertex : TEXCOORD0;
	float4 scrPos : TEXCOORD1;
	float4 projPos : TEXCOORD2;
	float4 ray : TEXCOORD3;
    UNITY_FOG_COORDS(4)
    fixed4 color : COLOR;
    UNITY_VERTEX_OUTPUT_STEREO

    #if defined(PARTICLE_SHADER)
		float4 params : TEXCOORD5;
	#else
    	UNITY_VERTEX_INPUT_INSTANCE_ID
	#endif
};

v2f vertex_shader (appdata_t v)
{
	v2f o;
    #if !defined(PARTICLE_SHADER)
    UNITY_SETUP_INSTANCE_ID(v);
    #endif

    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.screen_vertex = UnityObjectToClipPos(v.vertex);
	o.world_vertex.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
	o.world_vertex.w = -UnityObjectToViewPos( float3(0,0,0) ).z;
	o.scrPos = ComputeScreenPos(o.screen_vertex);
	o.ray.xyz = o.world_vertex.xyz - _WorldSpaceCameraPos;
	o.ray.w = dot(o.screen_vertex, ObliqueFrustumCorrection); // oblique frustrum correction factor
	float4 wvertex = mul(UNITY_MATRIX_VP, float4(o.world_vertex.xyz, 1.0));
	o.projPos = ComputeScreenPos (wvertex);
	o.projPos.z = -mul(UNITY_MATRIX_V, float4(o.world_vertex.xyz, 1.0)).z;
	o.color = v.color * LIGHT_COLOUR * 0.01;

    #if defined(PARTICLE_SHADER)
		o.params = v.params;
	#endif

    UNITY_TRANSFER_FOG(o,o.screen_vertex);
    #if !defined(PARTICLE_SHADER)
		UNITY_TRANSFER_INSTANCE_ID(v, o);
    #endif
	return o;
}