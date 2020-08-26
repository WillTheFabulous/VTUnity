#ifndef PANGU_GI
#define PANGU_GI

#include "UnityGlobalIllumination.cginc"
#include "Assets/Res/Shader/Shaders/PanguSoftShadow.cginc"
#include "Assets/Res/Shader/Shaders/PanguCommon.cginc"


#define pangu_Lightmap unity_Lightmap
#define pangu_LightmapInd unity_LightmapInd
#define pangu_ShadowMask unity_ShadowMask

#ifndef _SCENE_LIGHT_GLOBAL_PARAMS
half4 _SceneLightGlobalParams = half4(1, 1, 1, 0);
#define _SCENE_LIGHT_GLOBAL_PARAMS
#define _SceneEmissionScale _SceneLightGlobalParams.x
#define _SceneLightmapScale _SceneLightGlobalParams.y
#define _SceneBakedSpecularScale _SceneLightGlobalParams.z
#define _SpecularDirAdjust _SceneLightGlobalParams.w
#endif

#if defined(_CHARACTER_LIGHT_ON)
half _CharacterShadow;
#if !defined(UNITY_PASS_FORWARDADD)
half4 _CharacterLightColor;
half3 _CharacterLightDir;
#endif
#endif

// VLM
half _VlmScale;
CBUFFER_START(VlmSH)
half4 _VlmValues[7];
CBUFFER_END
#define _Vlm_SHAr _VlmValues[0]
#define _Vlm_SHBr _VlmValues[1]
#define _Vlm_SHAg _VlmValues[2]
#define _Vlm_SHBg _VlmValues[3]
#define _Vlm_SHAb _VlmValues[4]
#define _Vlm_SHBb _VlmValues[5]
#define _Vlm_SHC _VlmValues[6]

#ifdef _LOW_CHARACTER_SH
half3 ShadeSH(half3 normal)
{
    return 0;
}
#else
half3 ShadeSH(half3 normal)
{
    half3 n = half3(-normal.x, normal.z, normal.y);

    half4 c = half4(0.325735, 0.273137, 0.078848, 0.282095);
    half4 a = half4(-c.x * n.y, c.x * n.z, -c.x * n.x, c.y * n.x * n.y);
    half4 b = half4(-c.y * n.y * n.z, c.z * (3.0h * n.z * n.z - 1.0h), -c.y * n.x * n.z, 0.5h * c.y * (n.x * n.x - n.y * n.y));

    half3 sh = _Vlm_SHC.xyz * c.w;
    sh.r += dot(a, _Vlm_SHAr) + dot(b, _Vlm_SHBr);
    sh.g += dot(a, _Vlm_SHAg) + dot(b, _Vlm_SHBg);
    sh.b += dot(a, _Vlm_SHAb) + dot(b, _Vlm_SHBb);
    return max(half3(0, 0, 0), sh) * _VlmScale;
}
#endif

#if defined(_VLMLOW)

sampler2D_half _VLightmapTex;
float4 _VLightmapScale; // w -> half brick size
float4 _VLightmapOffset; // w -> uv scale
half4 _VLM_Strength;
#define _VLM_LightStrength max(_VLM_Strength.x, _VLM_Strength.z)
#define _VLM_ShadowStrength max(_VLM_Strength.y, _VLM_Strength.w)
half4 ShadeVLightmap(float3 pos)
{
    float4 offset = pos.xyzy - _VLightmapOffset.xyzy;
    offset.y -= _VLightmapScale.w;
    offset.w += _VLightmapScale.w;
    offset *= _VLightmapScale.xyzy;

    float4 grid;
    float2 ext = offset.yw * 8.f;
    grid.yw = floor(ext);
    ext = (ext - grid.yw) * 8.f;
    grid.xz = floor(ext);
    ext.x = ext.x - grid.x;
    float4 uv = (offset.xzxz + grid.xyzw) / 8.f;
#if defined(SHADER_API_METAL) && defined(SHADER_STAGE_FRAGMENT)
    float2 dfdx = ddx(offset.xz / 8.f);
    float2 dfdy = ddy(offset.yw / 8.f);
    half4 sh1 = tex2D(_VLightmapTex, uv.xy, dfdx, dfdy);
    half4 sh2 = tex2D(_VLightmapTex, uv.zw, dfdx, dfdy);
#else
    half4 sh1 = tex2Dlod(_VLightmapTex, float4(uv.xy, 0, 0));
    half4 sh2 = tex2Dlod(_VLightmapTex, float4(uv.zw, 0, 0));
#endif
    return lerp(sh1, sh2, ext.x);
}
#else
half4 ShadeVLightmap(float3 pos)
{
    return 0;
}
#endif
//

#if defined(_FOLIAGE_LIGHTING)
    #define _NO_DIRLIGHTMAP
    #if defined(LIGHTMAP_ON)
        #undef HANDLE_SHADOWS_BLENDING_IN_GI
    #endif
#endif

#define UNREAL_LIGHTMAP_ON

// Unreal colorDirScaleOffset Transfer {{{
half _NightScale;
int pangu_LightmapConfig = -1;
static half4 _vertLightmapExtraData;
// PG_CUSTOM_V2F is an additional piece of code appended to the generated surface vertex shader.
// To check what is going on, click the [Show generated code] button in shader inspector panel.
#if defined(LIGHTMAP_ON) && !defined(_NO_LIGHTMAP)
    // o.lmap.zw is usually used as dynamic lightmap UV, therefore in most situations it is zero.
    // The interpolated version of it can conveniently be accessed in GI function using data.lightmapUV
    #define PG_CUSTOM_V2F(o) o.lmap = _vertLightmapExtraData;
#endif
#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADERBATCH_INSTANCING_ON)
    #if !defined(LIGHTMAP_PARAM) 
        #define LIGHTMAP_PARAM
        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _LightmapScaleOffset)
            UNITY_DEFINE_INSTANCED_PROP(float4, _LightmapColorDir)
        UNITY_INSTANCING_BUFFER_END(Props)
    #endif
    #define UNREAL_LIGHTMAP_COLOR_DIR UNITY_ACCESS_INSTANCED_PROP(Props, _LightmapColorDir)
    #define UNREAL_LIGHTMAP_ST UNITY_ACCESS_INSTANCED_PROP(Props, _LightmapScaleOffset)
#else
    #define UNREAL_LIGHTMAP_COLOR_DIR unity_LightmapColorDir
    #define UNREAL_LIGHTMAP_ST unity_LightmapST
#endif
#define UNREAL_LIGHTMAP_INPUT
#define TRANSFER_UNREAL_LIGHTMAP_VERT(i, o) TransferUnrealLightmapVert(i.texcoord1, i.texcoord3);
#define TRANSFER_UNREAL_TREE_LIGHTMAP_VERT(i, o) TransferTreeUnrealLightmapVert(i.texcoord1, i.texcoord3);

#define APPLY_UNREAL_LIGHTMAP(i)

inline half3 UnpackHalf3(float f, float2 mad)
{
    half3 result = frac(f / float3(16777216, 65536, 256));
    return result * mad.x + mad.y;
}

inline half2 UnpackHalf2(float f, float2 mad)
{
    half2 result = half2(f / 32768.h, frac(f));
    return result * mad.x + mad.y;
}

half3 unquantize(half3 logRgb)
{
    half logScale = 16.h;
    half logBlackPoint = exp2(-0.5h * logScale);

    half logLuminance = clamp(dot(logRgb, half3(0.3, 0.59, 0.11)), 0.00007h, 1.h);
    half3 unitRgb = logRgb / logLuminance;
    half luminance = exp2((logLuminance - 0.5h) * logScale) - logBlackPoint;
    half3 rgb = unitRgb * luminance;

    return rgb;
}

half3 quantize(half3 rgb)
{
    half logScale = 16.h;
    half logBlackPoint = exp2(-0.5h * logScale);

    half luminance = dot(rgb, half3(0.3, 0.59, 0.11));
    half3 unitRgb = rgb / luminance;
    half logLuminance = log2(luminance + logBlackPoint) / logScale + 0.5h;
    half3 logRgb = unitRgb * logLuminance;

    return logRgb;
}

#ifndef UNITY_POINT_SAMPLE_TEX2D_LOD
    SamplerState lightmapSampler_Point_Clamp; // sampler literal
    #define UNITY_POINT_SAMPLE_TEX2D_LOD(tex,coord) tex.SampleLevel(lightmapSampler_Point_Clamp, (coord).xy, (coord).w)
#endif

#ifdef _TREE_LEAF_VS_LIGHT
void TransferTreeUnrealLightmapVert(float2 texcoord1, float2 texcoord3)
{
#if defined(_NO_LIGHTMAP) || !defined(LIGHTMAP_ON)
    _vertLightmapExtraData = half4(0, 0, 0, 1);
#else
    float2 lightmapUV = texcoord1 * UNREAL_LIGHTMAP_ST.xy + UNREAL_LIGHTMAP_ST.zw;
    half2 unrealLightmapColorDir = UnpackHalf2(UNREAL_LIGHTMAP_COLOR_DIR[pangu_LightmapConfig], float2(1, 0));
    half4 vertexColor = UNITY_POINT_SAMPLE_TEX2D_LOD(pangu_Lightmap, float4(lightmapUV, 0, 0));
    vertexColor.rgb = vertexColor.rgb * unrealLightmapColorDir.xxx + unrealLightmapColorDir.yyy;
    _vertLightmapExtraData.rgb = unquantize(vertexColor.rgb);
    _vertLightmapExtraData.a = vertexColor.a * vertexColor.a;
#endif
}
#endif // _TREE_LEAF_VS_LIGHT

void TransferUnrealLightmapVert(float2 texcoord1, float2 texcoord3)
{
#if defined(_NO_LIGHTMAP) || !defined(LIGHTMAP_ON)
    _vertLightmapExtraData = half4(1, 0, 1, 0);
#else
    float2 lightmapUV = texcoord1 * UNREAL_LIGHTMAP_ST.xy + UNREAL_LIGHTMAP_ST.zw;
    half2 unrealLightmapColorDir = UnpackHalf2(UNREAL_LIGHTMAP_COLOR_DIR[pangu_LightmapConfig], float2(1, 0));
    _vertLightmapExtraData = half4(lightmapUV, unrealLightmapColorDir);
#endif
}

// Unreal colorDirScaleOffset Transfer }}}

fixed PanguSampleBakedOcclusion (half2 lightmapUV, float3 worldPos)
{
    #if defined (SHADOWS_SHADOWMASK) || defined(LIGHTMAP_ON_ALL)
        #if defined(LIGHTMAP_ON) || defined(LIGHTMAP_ON_ALL)
            fixed4 rawOcclusionMask = UNITY_SAMPLE_TEX2D_SAMPLER(pangu_ShadowMask, pangu_Lightmap, lightmapUV.xy);
        #else
            fixed4 rawOcclusionMask = fixed4(1.0, 1.0, 1.0, 1.0);
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                if (IS_EQUAL(unity_ProbeVolumeParams.x, 1.0))
                    rawOcclusionMask = LPPV_SampleProbeOcclusion(worldPos);
                else
                    rawOcclusionMask = UNITY_SAMPLE_TEX2D(pangu_ShadowMask, lightmapUV.xy);
            #else
                rawOcclusionMask = UNITY_SAMPLE_TEX2D(pangu_ShadowMask, lightmapUV.xy);
            #endif
        #endif
        return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));

    #else
        //Handle LPPV baked occlusion for subtractive mode
        #if UNITY_LIGHT_PROBE_PROXY_VOLUME && !defined(LIGHTMAP_ON) && !UNITY_STANDARD_SIMPLE
            fixed4 rawOcclusionMask = fixed4(1.0, 1.0, 1.0, 1.0);
            if (IS_EQUAL(unity_ProbeVolumeParams.x, 1.0))
                rawOcclusionMask = LPPV_SampleProbeOcclusion(worldPos);
            return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));
        #endif

        return 1;
    #endif
}

inline half3 PanguDecodeDirectionalLightmap (half3 color, fixed4 normalizeBakedDir,half3 normalWorld)
{
    half dotNormal = max(dot(normalWorld, normalizeBakedDir.xyz) + 0.5h, 0);//会出现负值
    //Baked GI&SpotLight&PointLight : Lambert
    half directWeight = dotNormal;///max(1e-4h, normalizeBakedDir.w);

    return color * directWeight;
}

half3 _CharacterShadowDir;
half _SceneShadow;
half _BakedShadowBase;
inline UnityGI PanguGI_Unreal(inout UnityGIInput data, inout half occlusion, inout half3 normalWorld, inout fixed atten, inout UnityGI o_gi, inout fixed bakedAO)
{
#if !defined(_NO_LIGHTMAP) && defined(LIGHTMAP_ON)
    atten = 1;

    #ifdef _TREE_LEAF_VS_LIGHT
    {
        _vertLightmapExtraData = data.lightmapUV;
        o_gi.indirect.diffuse = data.lightmapUV.rgb;
        data.atten = data.lightmapUV.a;
        return o_gi;
    }
    #endif // _TREE_LEAF_VS_LIGHT

    /////////////////////////////////////////////////////////////////////////////////////
    fixed4 bakedIndirect = UNITY_SAMPLE_TEX2D(pangu_Lightmap, data.lightmapUV.xy);
    fixed4 bakedIndirectDir = UNITY_SAMPLE_TEX2D_SAMPLER (pangu_LightmapInd, pangu_Lightmap, data.lightmapUV.xy);
    fixed shadowValue = bakedIndirect.a * bakedIndirect.a;
    _SceneShadow = bakedIndirect.a;

#ifndef _NO_DIRLIGHTMAP
    half aoValue = bakedIndirectDir.a;
#endif

    shadowValue = saturate(shadowValue + _BakedShadowBase);

    #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
        #if ENABLE_PANGU_SHADOW
            data.atten = PanguSampleSceneShadow(data.worldPos);
        #endif
        half realTimeAtten = data.atten;
        half normalDot = dot(_CharacterShadowDir, normalWorld);
        realTimeAtten = normalDot < 0 ? 1 : realTimeAtten;
        data.atten = min(saturate(realTimeAtten + 0.2h), shadowValue);
    #else
        data.atten = shadowValue;
    #endif

    half4 unreal_DirScale = half4(2, 2, 2, 0);
    half4 unreal_DirBias = half4(-1, -1, -1, 0.282095); // 0.282095: ambient part of indirect light
    half4 unreal_ColorScale = half4(data.lightmapUV.zzz, 0);
    half4 unreal_ColorBias = half4(data.lightmapUV.www, 0);

    //// Unreal Lightmap Directional Decode
    // R:Z, G:Y, B:-X, A: Exposure Compensation
    bakedIndirectDir.a = 1;
    bakedIndirectDir = bakedIndirectDir * unreal_DirScale + unreal_DirBias;
    bakedIndirectDir = fixed4(-bakedIndirectDir.b, bakedIndirectDir.g,
                               bakedIndirectDir.r, bakedIndirectDir.a);

    //// Unreal Lightmap Decode
    fixed3 logRGB = bakedIndirect.rgb;
    logRGB = logRGB * unreal_ColorScale.rgb + unreal_ColorBias.rgb;
    half3 baked = unquantize(logRGB);

    _vertLightmapExtraData.rgb = baked;

    bakedAO = 1;
#ifndef _NO_DIRLIGHTMAP
    half directionality = saturate(dot(bakedIndirectDir.xyz, normalWorld)) + bakedIndirectDir.a;
    //bakedAO = dirLength;
    bakedAO = aoValue * aoValue;
#else
    half directionality = 0.6h;
#endif

    baked *= directionality;

    //// Fill into UnityGI Struct
    o_gi.indirect.diffuse = baked * _SceneLightmapScale;
#ifndef _NO_DIRLIGHTMAP
    // too expensive to normalize. multiply it by 2 to approach normalization.
    bakedIndirectDir.xyz *= 2.h;
    o_gi.indirect.specular = bakedIndirectDir;
#else
    o_gi.indirect.specular = 0;
#endif
    /////////////////////////////////////////////////////////////////////////////////////

    #if defined(HANDLE_SHADOWS_BLENDING_IN_GI) //&& !defined(UNITY_INSTANCING_ENABLED)
        o_gi.indirect.diffuse *= saturate(realTimeAtten+_CUBE_BASE);
    #endif

#endif
    return o_gi;
}

half _VolumetricShadow;
#define _V_SHADOW_BASE 0.6h
#define _V_SHADOW_BASE_SCALE 1 - _VolumetricShadow * _V_SHADOW_BASE
#define _V_SHADOW_BASE_CUBE 0.35h
#define _V_SHADOW_BASE_CUBE_SCALE 1 - _VolumetricShadow * _V_SHADOW_BASE_CUBE
#define _V_SHADOW_BASE_SKIN 0.3h
#define _V_SHADOW_BASE_SKIN_SCALE 1 - _VolumetricShadow * _V_SHADOW_BASE_SKIN

#if defined(_CHARACTER_LIGHT_ON)
half3 _CharacterAmbient;
#endif
#ifdef _VLMLOW
    half _VShadow;
#endif

inline UnityGI PanguGI_Base(UnityGIInput data, half occlusion, half3 normalWorld ,out fixed atten, out fixed bakedAO)
{
    UnityGI o_gi;
    ResetUnityGI(o_gi);

    o_gi.light = data.light;

    //#if UNITY_SHOULD_SAMPLE_SH
    #if defined(_CHARACTER_LIGHT_ON)
        o_gi.indirect.diffuse = _CharacterAmbient;
    #endif
    //#endif
    o_gi.indirect.specular = data.light.dir;
    atten = 1;
    bakedAO = 1;
    _SceneShadow = 1;
    #if !defined(_NO_LIGHTMAP)
        #if defined(LIGHTMAP_ON)
            PanguGI_Unreal(data, occlusion, normalWorld, atten, o_gi, bakedAO);
        #else
            #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
                half bakedAtten = PanguSampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
                data.atten = min(data.atten, bakedAtten);
            #endif
            //没有lightmap时,清理相关值
            #if defined(POINT) || defined(SPOT)
                o_gi.indirect.diffuse = 0;
            #endif
        #endif
        o_gi.light.color *= data.atten;
        atten = data.atten;
    #endif

#ifdef _VLMLOW
    half4 vlm = ShadeVLightmap(data.worldPos);
    half shadow = smoothstep(0.3h, 0.7h, vlm.a) * _VLM_ShadowStrength;
    data.atten = saturate((1 - shadow) * _SceneLightmapScale);
    _VShadow = data.atten;
    atten = data.atten;
    o_gi.indirect.diffuse += vlm.rgb * (_SceneLightmapScale * _VLM_LightStrength);
#endif

#if defined(_CHARACTER_LIGHT_ON)
    #if ENABLE_PANGU_SHADOW
        _CharacterShadow = PanguSampleShadow(data.worldPos); //data.atten;
    #else
        _CharacterShadow = data.atten;
    #endif
    #if defined(_LIGHTING_SSS)
    #else
        o_gi.indirect.diffuse *= occlusion;
    #endif
#endif
    #if defined(_CHARACTER_LIGHT_ON) && !defined(UNITY_PASS_FORWARDADD)
        data.atten *= _V_SHADOW_BASE_SCALE;
        atten = data.atten;
        _CharacterShadow *= _V_SHADOW_BASE_SCALE;
        o_gi.light.color = (_CharacterLightColor.rgb + _LightColor0.rgb * _CharacterLightColor.a) * _CharacterShadow;
        o_gi.light.dir = _CharacterLightDir;
        o_gi.indirect.specular = 0;
    #endif

    return o_gi;
}

inline half3 PanguGI_IndirectSpecular(UnityGIInput data, half occlusion, Unity_GlossyEnvironmentData glossIn)
{
    half3 specular;

    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        // we will tweak reflUVW in glossIn directly (as we pass it to Unity_GlossyEnvironment twice for probe0 and probe1), so keep original to pass into BoxProjectedCubemapDirection
        half3 originalReflUVW = glossIn.reflUVW;
        glossIn.reflUVW = BoxProjectedCubemapDirection (originalReflUVW, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);
    #endif

    #ifdef _GLOSSYREFLECTIONS_OFF
        specular = unity_IndirectSpecColor.rgb;
    #else
        specular = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], glossIn);
    #endif

    specular *= _V_SHADOW_BASE_CUBE_SCALE;

    return specular * occlusion;
}


#endif //PANGU_GI