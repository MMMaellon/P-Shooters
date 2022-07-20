#ifndef SCSS_AUDIOLINK_INCLUDED
#define SCSS_AUDIOLINK_INCLUDED
// Reference the documentation at 
// https://github.com/llealloo/vrc-udon-audio-link
// for more info.

#ifdef SHADER_TARGET_SURFACE_ANALYSIS
#define AUDIOLINK_COMPILE_COMPATIBILITY
#endif

#ifdef AUDIOLINK_COMPILE_COMPATIBILITY
sampler2D  _AudioTexture;
#else
Texture2D<float4> _AudioTexture;
SamplerState sampler_AudioGraph_Linear_Clamp;
#endif

uniform float _alModeR;
uniform float _alModeG;
uniform float _alModeB;
uniform float _alModeA;

uniform float _alBandR;
uniform float _alBandG;
uniform float _alBandB;
uniform float _alBandA;

uniform half4 _alColorR;
uniform half4 _alColorG;
uniform half4 _alColorB;
uniform half4 _alColorA;

uniform float _alTimeRangeR;
uniform float _alTimeRangeG;
uniform float _alTimeRangeB;
uniform float _alTimeRangeA;

uniform float _alUseFallback;
uniform float _alFallbackBPM;


float al_lerpstep( float a, float b, float t)
{
    return saturate( ( t - a ) / ( b - a ) );
}

float2 audioLinkModifyTexcoord(float4 texelSize , float2 p)
{
    p = p*texelSize.zw;
    // Instead of a hard clamp, sharpen to a pixel width for glancing angles
    float2 c = max(0.0, abs(fwidth(p)));
    c.x = 1;
    p = p + abs(c);
    p = floor(p) + saturate(frac(p) / c);
    p = (p - 0.5)*texelSize.xy;
    return p;
}

float audioLinkRenderBar(float grad, float pulse)
{
    float2 deriv = abs(fwidth(grad));
    float step = deriv*0.5;
    return al_lerpstep(pulse, pulse + step, grad);
}

float al_expImpulse( float x, float k )
{
    const float h = k*x;
    return h*exp(1.0-h);
}
float al_parabola( float x, float k )
{
    return pow( 4.0*x*(1.0-x), k );
}

// Samples the AudioLink texture. 
float sampleAudioTexture(float band, float delay, float range)
{
    // Initialisation. 
    float2 audioLinkRes = 0;
    _AudioTexture.GetDimensions(audioLinkRes.x, audioLinkRes.y);

    if (audioLinkRes.x >= 128.0 && _alUseFallback != 2)
    {
        float2 params = float2(delay, band / 4.0);
        // We only want the bottom 4 bands.
        // When reading the texture, we want the bands to be thickly seperated.
        float2 alUV = params*float2(range,0.0625);
        alUV = audioLinkModifyTexcoord(float4(1.0/audioLinkRes, audioLinkRes), alUV);
        // sample the texture
        #ifdef AUDIOLINK_COMPILE_COMPATIBILITY
        return tex2Dlod(_AudioTexture, float4(alUV, 0, 0));
        #else
        return _AudioTexture.SampleLevel(sampler_AudioGraph_Linear_Clamp, alUV, 0);
        #endif
    } else {
        if (_alUseFallback != 0) 
        {
        // If not available, fake one.
        float beat = _alFallbackBPM / 60;
        float rowTiming = (4-band)/4.0;
        delay *= range;
        beat = (delay-_Time.y)*rowTiming*beat;
        beat = frac(-beat);
        beat = al_expImpulse(beat, 8.0);
        float s; float c;
        sincos(beat, s, c);
        float final = saturate(s+(0.5+c));
        // 
        return final*beat;
        }
    }

    return 0;
}

float audioLinkGetLayer(float weight, const float range, const float band, const float mode)
{
    if (mode == 0) return weight * pow(sampleAudioTexture(band-1, 1-weight, range ), 2.0) * 2.0;
    if (mode == 1) return audioLinkRenderBar(weight, 1-sampleAudioTexture(band-1, 1-weight, range ));
    return 0;
}

#endif //SCSS_AUDIOLINK_INCLUDED