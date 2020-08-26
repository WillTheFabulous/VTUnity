#ifndef SCENEPROP_SHADER_EFFECT
#define SCENEPROP_SHADER_EFFECT

#if defined(_GSEFFECT_ON) || defined(_CAUSTICS_GS)

#define TRANS_GS_BIGNORMAL_UV(IN, o) o.UVBigNormal = IN.texcoord;

sampler2D_half _GSTex;
sampler2D_half _GSNormal;
sampler2D_half _BigNormal;
//fixed _BigNormalLerp;

void gsEffect(fixed2 texcoord, fixed3 worldNormal, fixed3 bigNormal, inout PanguSurfaceOutputStandard o)
{
    fixed4 gsColor = tex2D(_GSTex, texcoord);
    half4 gsParams = _GSParams;
#ifdef _LOW_GS
    fixed factor = saturate(((worldNormal.y + gsParams.x) * gsParams.y - 0.65h) * 5.0h);
    o.Albedo = lerp(o.Albedo, gsColor, factor);
    o.Normal = half3(0, 0, 1);
    o.Occlusion = 1;
    o.Smoothness = 0;
    o.Metallic = 0;
#else
    fixed factor = saturate(((worldNormal.y + gsParams.x) * gsParams.y - 0.5h) * 5.0h);
    o.Albedo = lerp(o.Albedo, gsColor, factor);
    fixed4 gsNormalTex = tex2D(_GSNormal, texcoord);
    fixed2 gsNormal = fixed2(gsNormalTex.x, gsNormalTex.y) * 2.0h - 1.0h;
    half bigNormalLerp = _BigNormalLerp;
    o.Normal.xy = bigNormal.xy + o.Normal.xy * (1 - factor) * bigNormalLerp;
    o.Normal.xy += gsNormal.xy * factor;
    o.Smoothness = lerp(o.Smoothness, gsColor.a * gsParams.z, factor);
    o.Occlusion = lerp(o.Occlusion, gsNormalTex.a, factor);
#endif
}

#ifdef _LOW_GS
#define APPLY_GS_EFFECT(IN, o, nTex, c) \
        fixed3 worldNormal = WorldNormalVector(IN, fixed3(0, 0, 1));\
        fixed3 bigNormal = 0;\
        o.Metallic = 0;\
        gsEffect(IN.uv_GSTex, worldNormal, bigNormal, o);
#else
#define APPLY_GS_EFFECT(IN, o, nTex, c) o.Normal.b = 1;\
        o.Occlusion = nTex.a;\
        o.Smoothness = c.a;\
        o.Metallic = 0;\
        half4 bigNormalTex = tex2D(_BigNormal, IN.UVBigNormal);\
        half3 bigNormal = half3(bigNormalTex.rg, 1) * 2 - 1;\
        o.Occlusion = bigNormalTex.a;\
        o.Normal.xy = bigNormal.xy + o.Normal.xy * _BigNormalLerp;\
        fixed3 worldNormal = WorldNormalVector(IN, o.Normal);\
        gsEffect(IN.uv_GSTex, worldNormal, bigNormal, o);
#endif

#define GS_INPUT half2 uv_GSTex; half2 UVBigNormal;
#else
#define TRANS_GS_BIGNORMAL_UV(IN, o)
#define APPLY_GS_EFFECT(IN, o, nTex, c)
#define GS_INPUT
#endif // _GSEFFECT_ON

half _RainSmoothnessFactor;

#if defined(_RAINMAP) || defined(_LOW_RAINSNOW)
void AddLODRain(inout PanguSurfaceOutputStandard o)
{
    o.Albedo *= (_RainWetFactor - 1.h) * _RainAmount * (1.h - o.Metallic) + 1.h;
    o.Smoothness = saturate(o.Smoothness + _RainAmount*(1.h - o.Metallic));
}
void AddLODSnow(half NormalY, inout PanguSurfaceOutputStandard o)
{
    half modifiedNormal = saturate(NormalY * 3.h);
    half snowFactor = saturate(modifiedNormal * _SnowAmount * _SnowStrength);
    o.Albedo = lerp(o.Albedo, 0.8h, snowFactor);
    o.Normal = lerp(o.Normal, half3(0.h, 0.h, 1.h), snowFactor * 0.65h);
    o.Smoothness = lerp(o.Smoothness, 0.4h, snowFactor);
    o.Metallic = lerp(o.Metallic, 0.h, snowFactor);
}
#define APPLY_LODRAIN_EFFECT(o) AddLODRain(o);
#define APPLY_LODSNOW_EFFECT(WorldNormal, o) AddLODSnow(WorldNormal.y, o);
#endif

#if _RAINMAP
sampler2D_half _RainRippleNormalTex;
half _RainRippleStrength;
half _RainRippleTilling;
void RainEffect(float3 worldPosXYZ, half VertexNormalY, inout PanguSurfaceOutputStandard o, out half4 snowParamValues)
{
    fixed cosTheta = VertexNormalY;
    half isGround = floor(saturate(cosTheta + 0.2h));
    float2 worldUV = lerp((worldPosXYZ.xy + worldPosXYZ.zy),worldPosXYZ.xz,isGround);
    worldUV *= _RainRippleTilling;
    worldUV = frac(worldUV);
    fixed4 rainNormal = tex2D(_RainRippleNormalTex, worldUV);
    snowParamValues = rainNormal;
    fixed2 disturb = rainNormal.ba * 2.h-1.h;
    rainNormal.ba = half2(1.h, 1.h);
    // o.Normal.rg = o.Normal.rg*lerp(1.h, 0.5h, _RainAmount);
    o.Normal = o.Normal + (rainNormal * 2.h - 1.h) * _RainRippleStrength * isGround * _RainAmount;// *  floor(saturate(cosTheta) + 0.5));
    o.Albedo *= (_RainWetFactor - 1.h) * _RainAmount * (1.h - o.Metallic) + 1.h;
    o.Normal.rg += disturb * 0.14h * (1.6h - isGround) * _RainAmount;
    o.Smoothness = saturate(o.Smoothness + _RainAmount * (1.h - o.Metallic));
}
void SnowEffect(half NormalY, inout PanguSurfaceOutputStandard o, half whiteFactor, half4 snowParamValues, half transformFactor)
{
#ifdef _SIMPLE_SNOW
    half modifiedNormal = saturate(NormalY * 3.h);
    half snowFactor = saturate(modifiedNormal * _SnowAmount * _SnowStrength * whiteFactor);
#else
    half modifiedNormal = saturate(NormalY * 3.h);
    half transformCoefficient = pow(_SnowAmount, transformFactor);
    half snowFactor = saturate(modifiedNormal * transformCoefficient * _SnowStrength * whiteFactor);
#endif
    o.Albedo = lerp(o.Albedo, 0.8h, snowFactor);
    o.Metallic = lerp(o.Metallic, 0.h, snowFactor);
    o.Smoothness = lerp(o.Smoothness, snowParamValues.a, snowFactor);
    o.Normal = lerp(o.Normal, half3(snowParamValues.rg * 2.h - 1.h, 1.h), snowFactor);
    o.Occlusion = 1.h;
}
#define APPLY_RAIN_EFFECT(IN, WorldNormal, o, snowParamValues) RainEffect(IN.worldPos.xyz, WorldNormal.y, o, snowParamValues);
#define APPLY_SNOW_EFFECT(WorldNormal, o, whiteFactor, snowParamValues, transformFactor) SnowEffect(WorldNormal.y, o, whiteFactor, snowParamValues, transformFactor);
#endif // _RainMAP

#define APPLY_SMOOTH(o) AddSmooth(o);

// Bright part with snow: (brightness - min) / (max - min) -> a * brightness + b, a = 1/(max - min), b = -min/(max - min), calculated in material GUI
//   Dark part with snow: 1 - (brightness - min) / (max - min) -> a * brightness + b, a = -1/(max - min), b = 1 + min/(max - min), calculated in material GUI
#define APPLY_SNOW_EFFECT_BY_BRIGHTNESS(albedo, snowAmount, snowParams1, snowParams2, whiteFactor) \
    half brightness = albedo.r * .3h + albedo.g * .6h; \
    half snowBrightnessFactor = saturate(brightness * snowParams1 + snowParams2); \
    albedo = lerp(albedo, 1, snowBrightnessFactor * snowAmount * whiteFactor * _SnowStrength);

#define APPLY_SNOW_EFFECT_SIMPLE(albedo, snowAmount, whiteFactor) albedo = lerp(albedo, 1.0h, snowAmount * whiteFactor * _SnowStrength);

#ifdef _ALPHATEST_FADE_ON

void ApplyDitherCrossFade(float2 vpos, float fade)
{
    vpos /= 4; // the dither mask texture is 4x4
    // vpos.y = frac(vpos.y) * 0.0625 /* 1/16 */ + unity_LODFade.y; // quantized lod fade by 16 levels
    vpos.y = frac(vpos.y) * 0.0625 + fade * 0.0625;
    clip(tex2D(_DitherMaskLOD2D, vpos).a - 0.5h);
    // clip(CROSS_FADE_DATA / 15.0 - thresholdMatrix[fmod(vpos.x, 4)][fmod(vpos.y, 4)]);
}

fixed GetDitherCrossFadeCutoff(float2 vpos, float fade)
{
    vpos /= 4; 
    vpos.y = frac(vpos.y) * 0.0625 + fade * 0.0625;
    return tex2D(_DitherMaskLOD2D, vpos).a;
}

#endif // _ALPHATEST_ON

#define APPLY_LOD_CROSSFADE(screenPos, fade) ApplyDitherCrossFade(screenPos, fade)

#if defined(_CAUSTICS) || defined(_CAUSTICS_GS)
    sampler2D_half _CausticsTex;
    // #define _CausticsUseLight _CausticsParam3.x
    // #define _CausticsFade _CausticsParam3.y
    // #define _CausticsUseGI _CausticsParam3.z
    // #define _CausticsRimIntensity _CausticsParam3.w

    half4 _CausticsGlobalParams1;
    half4 _CausticsGlobalParams2;
    half4 _CausticsGlobalParams3;

    half4 _CausticsCustomParams;

    #define _CausticsScale          _CausticsGlobalParams1.x
    #define _CausticsIntensity      _CausticsGlobalParams1.y
    #define _CausticsColor          _CausticsGlobalParams3.rgb
    #define _CausticsFadeDistance   _CausticsGlobalParams1.z
    #define _CausticsUsesMainLight  _CausticsGlobalParams2.x
    #define _CausticsLightDir       half3(_CausticsGlobalParams1.w, _CausticsGlobalParams2.w, _CausticsGlobalParams3.w)
    #define _CausticsRim            _CausticsGlobalParams2.y
    #define _CausticsShadow         _CausticsGlobalParams2.z

    #define _CausticsUsesCustom     _CausticsCustomParams.x
    #define _CausticsCustomScale    _CausticsCustomParams.y // multiplied with the original scale to preserve ratio

    #define CAUSTICS_SURF(IN, o, uv) \
        o.WorldPos = IN.worldPos; \
        o.tc = uv
    
    half3 ApplyCaustics(half2 tc, half3 worldPos, half3 normalW, half3 gi, half3 lightDir, half3 c)
    {
        lightDir = _CausticsUsesMainLight ? lightDir : _CausticsLightDir;
        half3 e1 = cross(lightDir, half3(1, 0, 0));
        half3 e2 = cross(e1, lightDir);
        half2x3 lightMatrix = {e1, e2};
        half2 luv = mul(lightMatrix, worldPos);

        half distRange = max(_CausticsFadeDistance, 0.1h);
        half dist = clamp(distance(worldPos, _WorldSpaceCameraPos), 0, distRange) / distRange;
        dist = 2 / (1 + dist) - 1;

        half scale = _CausticsUsesCustom > 0.h ? _CausticsScale * _CausticsCustomScale : _CausticsScale;
        half4 b = tex2D (_CausticsTex, luv * scale) * _CausticsIntensity;
        // fixed3 caustics = min(c1, c2) * _CausticsColor.rgb;
        half3 caustics = b.rgb * _CausticsColor * dist;
        half NoL = saturate(dot(normalW, lightDir));
        half rim = NoL * NoL;
        caustics *= rim;
        caustics = lerp(caustics, caustics * gi, _CausticsShadow);
        // return lerp(c, c + caustics, b.a);
        return caustics * b.a;
    }

        // gi.indirect.diffuse = 1
    #define APPLY_CAUSTICS(tc, worldPos, c) \
        half3 normalW = WorldNormalVector(IN, o.Normal); \
        half3 lightDir = normalize(_WorldSpaceLightPos0.xyz); \
        o.Albedo = ApplyCaustics(tc, worldPos, normalW, (half3)1, lightDir, o.Albedo)

    #define APPLY_CAUSTICS_LIGHTING(tc, worldPos, normalW, lightColor, lightDir) \
        half3 caustics = ApplyCaustics(tc, worldPos, normalW, lightColor, lightDir, (half3)0); \
        lightColor += caustics

#else // _CAUSTICS
    #define CAUSTICS_SURF(IN, o, uv)
    #define APPLY_CAUSTICS(tc, worldPos, c)
    #define APPLY_CAUSTICS_LIGHTING(tc, worldPos, normalW, gi, lightDir)
#endif // _CAUSTICS

#endif // SCENEPROP_SHADER_EFFECT
