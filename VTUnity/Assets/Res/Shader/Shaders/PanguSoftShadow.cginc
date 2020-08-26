#ifndef PANGU_SOFT_SHADOW
#define PANGU_SOFT_SHADOW

#define ENABLE_PANGU_SHADOW defined(SHADOWS_SCREEN) && defined(UNITY_NO_SCREENSPACE_SHADOWS)

#if ENABLE_PANGU_SHADOW

uniform half _SoftShadowType = 0;
sampler2D_half _CustomShadowTex;
float4x4 _CustomShadowMatrix;
float _CustomShadowBias;

uniform half PANGU_CHARACTER_SOFT_SHADOW_SOFTNESS = 0.06;
uniform half PANGU_SCENE_SOFT_SHADOW_SOFTNESS = 0.06;
//uniform fixed PANGU_SOFT_SHADOW_HASH_SCALE = 0.001;
#if defined(SHADER_API_MOBILE) 
    #ifdef _LOW_SHADOW
        static const half2 ssDirPoissonDisks[4] =
        {
            half2(0.1f, 0.0f),
            half2(0.0f, 0.1f),
            half2(0.1, 0.1f),
            half2(-0.1, 0.1f),
        };
    #else
        static const half2 ssDirPoissonDisks[12] =
        {
            half2(0.0f, 0.0f),
            half2(0.2495298f, 0.732075f),
            half2(-0.3469206f, 0.6437836f),
            half2(-0.01878909f, 0.4827394f),
            half2(-0.2725213f, 0.896188f),
            half2(-0.6814336f, 0.6480481f),
            half2(0.4152045f, 0.2794172f),
            half2(0.1310554f, 0.2675925f),
            half2(0.5344744f, 0.5624411f),
            half2(0.8385689f, 0.5137348f),
            half2(0.6045052f, 0.08393857f),
            half2(0.4643163f, 0.8684642f),
        };
    #endif
#else
    static const half2 ssDirPoissonDisks[36] =
    {
        half2(0.0f, 0.0f),
        half2(0.2495298f, 0.732075f),
        half2(-0.3469206f, 0.6437836f),
        half2(-0.01878909f, 0.4827394f),
        half2(-0.2725213f, 0.896188f),
        half2(-0.6814336f, 0.6480481f),
        half2(0.4152045f, 0.2794172f),
        half2(0.1310554f, 0.2675925f),
        half2(0.5344744f, 0.5624411f),
        half2(0.8385689f, 0.5137348f),
        half2(0.6045052f, 0.08393857f),
        half2(0.4643163f, 0.8684642f),
        half2(0.335507f, -0.110113f),
        half2(0.03007669f, -0.0007075319f),
        half2(0.8077537f, 0.2551664f),
        half2(-0.1521498f, 0.2429521f),
        half2(-0.2997617f, 0.0234927f),
        half2(0.2587779f, -0.4226915f),
        half2(-0.01448214f, -0.2720358f),
        half2(-0.3937779f, -0.228529f),
        half2(-0.7833176f, 0.1737299f),
        half2(-0.4447537f, 0.2582748f),
        half2(-0.9030743f, 0.406874f),
        half2(-0.729588f, -0.2115215f),
        half2(-0.5383645f, -0.6681151f),
        half2(-0.07709587f, -0.5395499f),
        half2(-0.3402214f, -0.4782109f),
        half2(-0.5580465f, 0.01399586f),
        half2(-0.105644f, -0.9191031f),
        half2(-0.8343651f, -0.4750755f),
        half2(-0.9959937f, -0.0540134f),
        half2(0.1747736f, -0.936202f),
        half2(-0.3642297f, -0.926432f),
        half2(0.1719682f, -0.6798802f),
        half2(0.4424475f, -0.7744268f),
        half2(0.6849481f, -0.3031401f),
    };
#endif


inline half ssDirRandValue(half3 seed)
{
    half dt = dot(seed, half3(12.9898, 78.233, 45.5432));// project seed on random constant vector
    return frac(sin(dt) * 43758.5453h);// return only fractional part
}

// inline float HashXYZ(float3 seed)
// {
//     float maxDeriv = max(length(ddx(seed.xyz)), length(ddy(seed.xyz)));
//     float pixScale = 1 / (PANGU_SOFT_SHADOW_HASH_SCALE * maxDeriv);
//     float2 pixScales = float2(exp2(floor(log(pixScale))), exp2(ceil(log(pixScale))));
//     float2 hashs = float2(ssDirRandValue(floor(pixScales.x * seed.xyz)), ssDirRandValue(floor(pixScales.y * seed.xyz)));
//     float lerpFactor = frac(log2(pixScale));
//     float x = (1 - lerpFactor) * hashs.x + lerpFactor * hashs.y;
//     float minLerp = min(lerpFactor, 1 - lerpFactor);
//     float3 cases = float3(x * x / (2 * minLerp * (1 - minLerp)),
//                          (x - 0.5 * minLerp) / (1 - minLerp),
//                          1-((1-x)*(1-x)/(2*minLerp*(1-minLerp))) );

//     float result = (x < (1- minLerp)) ?
//                         ((x < minLerp)? cases.x : cases.y) :
//                         cases.z;
//     return result;
// }

half SampleShadowWithNoise(unityShadowCoord4 shadowCoord, float3 worldPos, int sampler_Number)
{
    half shadow = 0;
    half diskRadius = PANGU_CHARACTER_SOFT_SHADOW_SOFTNESS;
    half randAngle = ssDirRandValue(worldPos.xyz);
    half c = cos(randAngle);
    half s = sin(randAngle);

    UNITY_LOOP
    for (half j = 0; j < sampler_Number; j++)
    {
        //int index = int(sampler_Number * ssDirRandValue(shadowCoord.xyz)) % sampler_Number;
        half2 offset = ssDirPoissonDisks[j] * diskRadius;
        offset = half2(offset.x * c - offset.y * s, offset.y * c + offset.x * s);
        shadowCoord.z = shadowCoord.z < 0.01 ? 1: shadowCoord.z;
        half value = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, half4(shadowCoord.xy + offset, shadowCoord.z, 0.0));
        shadow += value;
    }
    shadow /= sampler_Number;

    return shadow;
}

half SampleShadowNoNoise(unityShadowCoord4 shadowCoord, float3 worldPos, half sampler_Number, float softness)
{
    half shadow = 0;
    half diskRadius = softness;

    UNITY_LOOP
    for (half j = 0; j < sampler_Number; j++)
    {
        half2 offset = ssDirPoissonDisks[j] * diskRadius;
        shadowCoord.z = shadowCoord.z < 0.01 ? 1: shadowCoord.z;
        half value = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, half4(shadowCoord.xy + offset, shadowCoord.z, 0.0));
        shadow += value;
    }
    shadow /= sampler_Number;

    return shadow;
}

half SampleCustomShadow(unityShadowCoord4 shadowCoord, float3 worldPos, half sampler_Number)
{
    half shadow = 0;
    half diskRadius = PANGU_CHARACTER_SOFT_SHADOW_SOFTNESS;// unity_LightShadowBias.x;//根据距离进行采图的偏移 //

    [unroll(sampler_Number)]
    for (half j = 0; j < sampler_Number; j++)
    {
        half2 offset = ssDirPoissonDisks[j] * diskRadius;
        shadowCoord = UNITY_PROJ_COORD(shadowCoord);
        shadowCoord.xy = shadowCoord.xy + offset;
        #if defined(UNITY_REVERSED_Z)
            half lightDepth = 1 - SAMPLE_DEPTH_TEXTURE_PROJ(_CustomShadowTex, shadowCoord);
        #else
            half lightDepth = SAMPLE_DEPTH_TEXTURE_PROJ(_CustomShadowTex, shadowCoord);
        #endif
        half value = (shadowCoord.z - _CustomShadowBias) < lightDepth ? 1.0h : 0.0h;
        shadow += value;
    }
    shadow /= sampler_Number;

    return shadow;
}

inline half PanguSampleSceneShadow(float3 worldPos)
{
     float4 shadowCoord = mul(unity_WorldToShadow[0], unityShadowCoord4(worldPos, 1));
     #if defined(SHADOWS_NATIVE)
        fixed shadow = 0;
        #if _SCENE_SOFT_SHADOW
            shadow = SampleShadowNoNoise(shadowCoord, shadowCoord.xyz, 8.0h, PANGU_SCENE_SOFT_SHADOW_SOFTNESS);
        #else
            shadowCoord.z = shadowCoord.z < 0.01 ? 1: shadowCoord.z;
            shadow = UNITY_SAMPLE_SHADOW(_ShadowMapTexture, shadowCoord.xyz);
        #endif
        shadow = _LightShadowData.r + shadow * (1 - _LightShadowData.r);
        return shadow;
    #else
        unityShadowCoord dist = SAMPLE_DEPTH_TEXTURE(_ShadowMapTexture, shadowCoord.xy);
        // tegra is confused if we use _LightShadowData.x directly
        // with "ambiguous overloaded function reference max(mediump float, float)"
        unityShadowCoord lightShadowDataX = _LightShadowData.x;
        unityShadowCoord threshold = shadowCoord.z;
        return max(dist > threshold, lightShadowDataX);
    #endif
}

inline half GetPanguSampleShadowValue(float3 worldPos)
{
    float4 shadowCoord = float4(0.0, 0.0, 0.0, 0.0);
    #if defined(_LOW_SHADOW)
        shadowCoord = mul(unity_WorldToShadow[0], unityShadowCoord4(worldPos, 1));
    #else
        shadowCoord = _SoftShadowType > 1 ? mul(_CustomShadowMatrix, float4(worldPos,1)) : mul(unity_WorldToShadow[0], unityShadowCoord4(worldPos, 1));
    #endif
    half shadow = 0;

#if defined(SHADER_API_MOBILE) 
    #ifdef _LOW_SHADOW
        shadow = SampleShadowNoNoise(shadowCoord, worldPos, 4.0h, PANGU_SCENE_SOFT_SHADOW_SOFTNESS);
    #else
        if(_SoftShadowType > 2){
            shadow = SampleCustomShadow(shadowCoord, worldPos, 12);
        }else if(_SoftShadowType > 1){
            shadow = SampleCustomShadow(shadowCoord, worldPos, 8);
        }else{
            half sampleCount = _SoftShadowType > -0.5h? 12.0h : 8.0h;
            sampleCount = _SoftShadowType > -1.5h? sampleCount : 4.0h;
            shadow = SampleShadowNoNoise(shadowCoord, worldPos, sampleCount, PANGU_CHARACTER_SOFT_SHADOW_SOFTNESS);
        }
    #endif
#else
    if(_SoftShadowType > 3){
        shadow = SampleCustomShadow(shadowCoord, worldPos, 36);
    }else if(_SoftShadowType > 2){
        shadow = SampleCustomShadow(shadowCoord, worldPos, 16);
    }else if(_SoftShadowType > 1){
        shadow = SampleCustomShadow(shadowCoord, worldPos, 8);
    }else{
        half sampleCount = _SoftShadowType > 0.5h? 36.0h : 16.0h;
        sampleCount = _SoftShadowType > -0.5h? sampleCount : 8.0h;
        shadow = SampleShadowNoNoise(shadowCoord, worldPos, sampleCount, PANGU_CHARACTER_SOFT_SHADOW_SOFTNESS);
    }
#endif
    return shadow;
}

inline half PanguSampleShadow(float3 worldPos)
{ 
    half shadow =  GetPanguSampleShadowValue(worldPos);
    shadow = _LightShadowData.r + shadow * (1 - _LightShadowData.r);

    return shadow;
}

#endif //#if defined(SHADOWS_SCREEN) && defined(UNITY_NO_SCREENSPACE_SHADOWS)

#endif //PANGU_SOFT_SHADOW