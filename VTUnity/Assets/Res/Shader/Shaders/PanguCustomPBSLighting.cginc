#ifndef PANGU_CUSTOM_PBS_LIGHTING
#define PANGU_CUSTOM_PBS_LIGHTING

#include "UnityPBSLighting.cginc"
#include "Assets/Res/Shader/Shaders/Scene/Prop/ScenePropEffect.cginc"

#ifndef _SCENE_LIGHT_GLOBAL_PARAMS
half4 _SceneLightGlobalParams = half4(1, 1, 1, 0);
#define _SCENE_LIGHT_GLOBAL_PARAMS
#define _SceneEmissionScale _SceneLightGlobalParams.x
#define _SceneLightmapScale _SceneLightGlobalParams.y
#define _SceneBakedSpecularScale _SceneLightGlobalParams.z
#define _SpecularDirAdjust _SceneLightGlobalParams.w
#endif

#ifndef LIGHTMAP_ON
    #define _LOD_GI_SHADOW //对于没有lightmap GI的补偿光(在场景未烘焙状态也会有这个光照补偿
#endif

sampler2D_half _EnvTexPBR;
half _EnvStrength;
half3 _IBLColor;
half3 _LOD_GI_Color;
half _LOD_GI_Shadow;
half _IBLLerp;

half _SpecIdentity;
half _Shininess;

half4 _TreeLeafCorrection;

#ifdef UNITY_COLORSPACE_GAMMA
#define pangu_ColorSpaceDielectricSpec half4(0.220916301h, 0.220916301h, 0.220916301h, 1.0h - 0.220916301h)
#else
#define pangu_ColorSpaceDielectricSpec half4(0.04h, 0.04h, 0.04h, 1.0h - 0.04h) // standard dielectric reflectivity coef at incident angle (= 4%)
#endif

inline half3 PanguDiffuseAndSpecularFromMetallic (half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)
{
    specColor = lerp (pangu_ColorSpaceDielectricSpec.rgb, albedo, metallic);
    half oneMinusDielectricSpec = pangu_ColorSpaceDielectricSpec.a;
    oneMinusReflectivity = oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
    return albedo * oneMinusReflectivity;
}

inline half3 FresnelLerpFastCustom(half3 F0, half3 F90, half cosA)
{
    half t = Pow4 (1.h - cosA);
    return lerp (F0, F90, t);
}

half ambientOcclusion;
#define MAX_SMOOTHNESS 0.85h
#define MAX_SMOOTHNESS_LOW 0.5h
#define SPEC_MOBILE_TERM 1e-4

inline half4 CustomBRDF(
    half3 diffColor, half3 specColor, half metallic, half smoothness,
    half3 normal, half3 viewDir, half3 ambient,
    UnityLight light, UnityIndirect gi, half3 specularDir, half oneMinusReflectivity, half lodGIShadow)
{
#ifdef _DEBUG_AO
    return half4(ambientOcclusion + 0.00001h * gi.diffuse, 1);
#endif
#ifdef _DEBUG_LIGHTMAP
    return half4(gi.diffuse, 1);
#endif
#ifdef _DEBUG_LIGHTMAP_SHADOW
    return half4(saturate(light.color), 1);
#endif

    half3 halfLightDir = Unity_SafeNormalize (light.dir + viewDir);
#ifndef _NO_DIRLIGHTMAP
    half3 halfGIDir = Unity_SafeNormalize (specularDir + viewDir);
#else
    half3 halfGIDir = viewDir;
#endif
    half3 halfDir = lerp(halfGIDir, halfLightDir, _SceneShadow);
    half nl = saturate(dot(normal, light.dir));
    half nh = saturate(dot(normal, halfDir));
    half nv = saturate(dot(normal, viewDir));
    half lh = saturate(dot(light.dir, halfDir));
    smoothness = min(smoothness,MAX_SMOOTHNESS);
    // Specular term
    float perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
    float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    float a = roughness;
    float a2 = a * a;
    float d = nh * nh * (a2 - 1.0) + 1.00001;

    float specularMobileTerm = SPEC_MOBILE_TERM;
#ifdef _RAINMAP
    specularMobileTerm = 1e-3;
#endif
    float lh2Max = max(0.1h, lh*lh);
    half specularTerm = a2 / (lh2Max * (roughness + 0.5) * (d * d) * 4 + specularMobileTerm);

#if defined (SHADER_API_MOBILE)
    specularTerm = specularTerm - 1e-4h;
#endif
#if defined (SHADER_API_MOBILE)
    specularTerm = clamp(specularTerm, 0.0, 100.0h); // Prevent FP16 overflow on mobiles
#endif
#if defined(_SPECULARHIGHLIGHTS_OFF)
    specularTerm = 0.0;
#endif

    half surfaceReduction = (0.6h - 0.08h * perceptualRoughness);
    surfaceReduction = 1.0h - roughness * perceptualRoughness * surfaceReduction;
    half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));
#if defined(_NO_IBL)
    half3 envBRDF = surfaceReduction * specColor;
    half3 IBL = _IBLColor * envBRDF;
#else
    //替代增强非金属地面反射 + 0.05h
    #ifdef _RAINMAP
        specColor += 0.05h;
        specColor = FresnelLerpFastCustom (specColor, grazingTerm * 0.2h, nv);
    #endif
    half3 envBRDF = surfaceReduction * specColor;
    #if defined(_LERP_IBL) && defined(_RAINMAP)
        half3 IBL = _IBLColor * lerp(specColor, envBRDF, _IBLLerp);
    #else
        half3 IBL = _IBLColor * envBRDF;
    #endif
#endif

    half3 lightTrans = light.color * nl;
    half3 diffLightTrans = lightTrans;
#ifdef _LOD_GI_SHADOW
#ifdef _VLMLOW
    lodGIShadow *= _VShadow;
#endif
    diffLightTrans *= lodGIShadow;
#if !defined(POINT) && !defined(SPOT)
    diffLightTrans += _LOD_GI_Color * saturate(1.0h - nl * lodGIShadow);
#endif
#endif
    half sata2 = saturate(a2 + 0.1h);
    half3 directSpecular = specularTerm * specColor * (lightTrans + gi.diffuse * sata2);
    half3 specularLighting = (IBL + directSpecular);
    // punctual specular lighting
    half3 diffuseLighting = diffColor * (diffLightTrans + ambient) + gi.diffuse * (diffColor + metallic * specColor * _SceneShadow);

#ifdef _DEBUG_SPECULAR
    return half4(specularLighting, 1);
#endif
#ifdef _DEBUG_DIFFUSE
    return half4(diffuseLighting, 1);
#endif

#ifdef _NO_SPECULAR
    return half4(diffuseLighting + IBL, 1);
#else
#ifdef _VLMLOW
    return half4(specularLighting + diffuseLighting, 1);
#else
    return half4(specularLighting + diffuseLighting, 1);
#endif
#endif
}

inline half4 CustomBRDFLow(
    half3 diffColor, half3 specColor,
    half3 normal, half smoothness, half3 viewDir, half3 ambient,
    UnityLight light, UnityIndirect gi, half3 specularDir, half lodGIShadow)
{
    half3 halfLightDir = Unity_SafeNormalize (light.dir + viewDir);
#ifndef _NO_DIRLIGHTMAP
    half3 halfGIDir = Unity_SafeNormalize (specularDir + viewDir);
#else
    half3 halfGIDir = viewDir;
#endif
    half3 halfDir = lerp(halfGIDir, halfLightDir,  _SceneShadow);
    half nl = saturate(dot(normal, light.dir));
    half nh = saturate(dot(normal, halfDir));
    half lh = saturate(dot(light.dir, halfDir));
    half3 lightTrans = light.color * nl;
    half3 diffLightTrans = lightTrans;

#ifdef _LOD_GI_SHADOW
#ifdef _VLMLOW
    lodGIShadow *= _VShadow;
#endif
    diffLightTrans *= lodGIShadow;
    diffLightTrans += _LOD_GI_Color * saturate(1.0h - nl * lodGIShadow);
#endif

    half3 diffuseLighting = diffColor * (diffLightTrans + gi.diffuse + ambient);
    smoothness = min(smoothness, MAX_SMOOTHNESS_LOW);
    half perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    half a = roughness;
    half a2 = a * a;
    half d = nh * nh * (a2 - 1.h) + 1.00001h;
    half specularMobileTerm = SPEC_MOBILE_TERM;
#ifdef _RAINMAP
    specularMobileTerm = 1e-3h;
#endif
    half specularTerm = a2 / (max(0.1h, lh * lh) * (roughness + 0.5h) * (d * d) * 4 + specularMobileTerm);

    #if defined(_LERP_IBL) && defined(_RAINMAP)
        //替代fresnel 增强非金属地面反射 +0.05h
        half3 IBL = _IBLColor * (specColor + 0.05h);
    #else
        half3 IBL = _IBLColor * specColor;
    #endif

    half3 directSpecular = specularTerm * specColor * (lightTrans + gi.diffuse * saturate(a2 + 0.1h));
    half3 specularLighting = (IBL + directSpecular);

#ifdef _NO_SPECULAR
    specularLighting = IBL;
#endif

    return half4(diffuseLighting + specularLighting, 1);
}

half4 _EnvColor;
inline half3 CustomIBL(half smoothness, half3 viewDir, half3 normal, half occlusion, fixed atten)
{

#if defined(_NO_IBL)
    #if defined(_LOD_IBL)
        half3 envColor = _EnvColor.rgb * _EnvStrength;
    #else
        half3 envColor = _EnvColor.rgb * (_EnvStrength * occlusion) * saturate(atten + _CUBE_BASE);
    #endif
#else
    half3 reflDir = -reflect(viewDir, normal);
    // reflectDir to UV
    half isPositive = half(reflDir.z > 0.0h);
    half positiveWeight = ((isPositive * 2.0h) - 1.0h);
    half2 env_uv = (reflDir.xy / ((reflDir.z * positiveWeight) + 1.0h));
    half2 env_offset = half2(0.25h + 0.5h *  (1 - isPositive), 0.25h + 0.5h * isPositive);
    env_uv = (env_uv * half2(-0.25h, 0.25h)) + env_offset;

    half roughness = 1 - smoothness;
    half mipLevel = roughness / 0.17h;
    half4 envColorSample = tex2Dlod(_EnvTexPBR, half4(env_uv, 0, mipLevel));
    half3 envColor = (envColorSample.xyz * ((envColorSample.w * envColorSample.w) * 16.0h));
#if defined(_LERP_IBL) && defined(_RAINMAP)
    _IBLLerp = max(_RainAmount, _SnowAmount);
    envColor = lerp(_EnvColor.rgb, envColor, _IBLLerp);
#endif
    envColor = envColor * (_EnvStrength * occlusion) * saturate(atten + _CUBE_BASE);
#endif
    return envColor * ambientOcclusion;
}

inline half3 GetSceneAmbient()
{
    half3 ambient = _AmbientColor.rgb;
    return ambient;
}

inline half4 LightingPanguPBS (PanguSurfaceOutputStandard s, half3 viewDir, UnityGI gi)
{
    s.Normal = normalize(s.Normal);

    #if defined(POINT) || defined(SPOT)
        //处理实时点光锥光
        half3 ambient = 0;
        half3 specularDir = gi.light.dir;
    #else
        half3 ambient = GetSceneAmbient() * ambientOcclusion;
        half3 specularDir = gi.indirect.specular;
    #endif
    //简化过的返照率
    half3 specColor = 0;
    half oneMinusReflectivity = 0;
    half3 diffColor = PanguDiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);

    #if !defined(POINT) && !defined(SPOT)
        APPLY_CAUSTICS_LIGHTING(s.tc, s.WorldPos, s.Normal, gi.light.color, gi.light.dir);
    #endif

    #ifdef _BAKESHADOW_LIGHT
        gi.light.color *= s.Occlusion * s.Occlusion;
        gi.indirect.diffuse = s.Alpha;
        half4 c = CustomBRDF(diffColor, specColor, s.Metallic, s.Smoothness, s.Normal, viewDir, ambient, gi.light, gi.indirect, specularDir, oneMinusReflectivity, s.Alpha);
    #else
        half4 c = CustomBRDF(diffColor, specColor, s.Metallic, s.Smoothness, s.Normal, viewDir, ambient * s.Occlusion, gi.light, gi.indirect, specularDir, oneMinusReflectivity, s.Alpha);
        c.a = s.Alpha;
    #endif

    #ifdef _BAKE_LOD2_ON
        c.rgb = c.rgb * _TreeLeafCorrection.rgb;
    #endif

    return c;
}

inline void LightingPanguPBS_GI (
    PanguSurfaceOutputStandard s,
    UnityGIInput data,
    inout UnityGI gi)
{
    fixed atten = 1;
    fixed bakedAO = 1;
    gi = PanguGI_Base(data, 1, s.Normal, atten, bakedAO);
    ambientOcclusion = bakedAO * 0.75h + 0.25h;
    _IBLColor = CustomIBL(s.Smoothness, data.worldViewDir, s.Normal, s.Occlusion, atten);
}

inline half4 LightingPanguPBSLow (PanguSurfaceOutputStandard s, half3 viewDir, UnityGI gi)
{
    s.Normal = normalize(s.Normal);

    #if defined(POINT) || defined(SPOT)
        //处理实时点光锥光
        half3 ambient = 0;
        half3 specularDir = gi.light.dir;
    #else
        half3 ambient = GetSceneAmbient() * ambientOcclusion;
        half3 specularDir = gi.indirect.specular;
    #endif

    half3 diffColor = s.Albedo;
    half3 specColor = 0;
    half oneMinusReflectivity = 0;
    diffColor = PanguDiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, specColor, oneMinusReflectivity);
    half4 c = CustomBRDFLow(diffColor, specColor, s.Normal, s.Smoothness, viewDir, ambient, gi.light, gi.indirect, specularDir, s.Alpha);

    return c;
}

inline void LightingPanguPBSLow_GI (
    PanguSurfaceOutputStandard s,
    UnityGIInput data,
    inout UnityGI gi)
{
    fixed atten = 1;
    fixed bakedAO = 1;
    gi = PanguGI_Base(data, 1, s.Normal, atten, bakedAO);
    ambientOcclusion = bakedAO;
    _IBLColor = CustomIBL(s.Smoothness, data.worldViewDir, s.Normal, 1, atten);
}

inline half UnpackPBSParams(half a, out half smoothness, out half metallic)
{
    metallic = saturate((a * 33.33h) - 23.33h);
    half smoothness1 = 1.4h - a * 2.0h; //  0.2 - 0.7 时，用该方式计算roughness
    half smoothness2 = (a * 3.0h) - 2.0h;// 0.7 - 1.0 时，用该方式计算roughness
    smoothness = lerp(smoothness1, smoothness2, metallic);
#ifdef _ALPHATEST_ON
    #if defined(_ALPHATEST_FADE_ON)
        half crossFadeY = CROSS_FADE_DATA.y;
        smoothness = min(saturate(0.3h + crossFadeY), smoothness);
    #else
        smoothness = min(0.3h, smoothness);
    #endif
#endif
    return saturate(a * 5.0h);
}

inline half PackPBSParams(half smoothness, half metallic, half alpha)
{
    half roughness = 1.0h - smoothness;
    half ret = lerp(0.7h - roughness * 0.5h, max((roughness + 2.0h) / 3.0h, 0.7h), metallic > 0.5h);
    return lerp(ret, alpha * 0.2h, alpha < 0.9h);
}

#endif //PANGU_CUSTOM_PBS_LIGHTING