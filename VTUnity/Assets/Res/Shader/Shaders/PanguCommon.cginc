#ifndef PANGU_COMMON
#define PANGU_COMMON

#define IS_EQUAL(x, a) abs(x - a) < 0.00001 
#define IS_EQUAL_HALF(x, a) abs(x - a) < 0.00001h

#if defined(SHADER_API_GLES3) || defined(SHADER_API_VULKAN)
    #define GLES_FLOAT float
    #define GLES_FLOAT2 float2
    #define GLES_FLOAT3 float3
    #define GLES_FLOAT4 float4
    #define MALI_FLOAT float
    #define MALI_FLOAT2 float2
    #define MALI_FLOAT3 float3
    #define MALI_FLOAT4 float4
#else
    #define GLES_FLOAT half
    #define GLES_FLOAT2 half2
    #define GLES_FLOAT3 half3
    #define GLES_FLOAT4 half4
    #define MALI_FLOAT half
    #define MALI_FLOAT2 half2
    #define MALI_FLOAT3 half3
    #define MALI_FLOAT4 half4
#endif

struct PanguSurfaceOutput {
    fixed3 Albedo;
    fixed3 Normal;
    fixed3 VertexNormal;
    fixed3 Emission;
    half Specular;
    fixed Gloss;
    fixed Alpha;
};

struct PanguSurfaceOutputStandard {
    fixed3 Albedo;
    half3 Normal;
    half3 Emission;
    half Metallic;
#if defined(_SILK_ON)
    half3 TangentT;
    half SilkMask;
    half4 SilkTile;
#elif defined(_VELVET_ON)
    half VelvetMask;
#elif defined(_CARPAINT_ON)
    half3 Normal1;
    half3 Normal2;
#elif defined(_FABRIC_ON)
    half FabricMask;
#endif
#ifdef _LIGHTING_SSS
    fixed4 Pore;
    fixed3 PoreNormal;
    fixed4 SSSParam;
    fixed4 Sparkle;
    fixed4 SparkleMask;
    GLES_FLOAT2 tc2;
#endif
#if _SPARKLE_ON
    half sparkleDist;
#endif
    half Smoothness;
    half Occlusion;
    fixed Alpha;
    fixed3 VertexNormal;
    fixed2 tc;
#if defined(_SILKSTOCK_ON)
    half SilkNoise;
#endif
#if defined(_CAUSTICS) || (_CAUSTICS_GS)
    half3 WorldPos;
#endif
};

#ifndef RAINSNOW_PARAM
#define RAINSNOW_PARAM
half4 _RainSnowParams;
#define _RainAmount _RainSnowParams.x
#define _SnowAmount _RainSnowParams.y
#define _RainWetFactor _RainSnowParams.z
#define _SnowStrength _RainSnowParams.w
#endif

//// MovablePlatform
float3 _PlatformPosition;
float4 _PlatformRotation;
float4 _PlatformRotationInv;

float3 QuaternionMultiply(float4 q, float3 dir)
{
    return dir + cross(q.xyz, cross(q.xyz, dir) + dir * q.w) * 2.f;
}

float3 WorldToPlatformDir(float3 dir)
{
    return QuaternionMultiply(_PlatformRotationInv, dir);
}

float3 PlatformToWorldDir(float3 dir)
{
    return QuaternionMultiply(_PlatformRotation, dir);
}

float3 WorldToPlatformPos(float3 pos)
{
    return QuaternionMultiply(_PlatformRotationInv, pos - _PlatformPosition);
}

float3 PlatformToWorldPos(float3 pos)
{
    return QuaternionMultiply(_PlatformRotation, pos) + _PlatformPosition;
}


#if defined(REFL_ALBEDO) || defined(WATER_LIGHT)
sampler2D_half _SeaEnvTex;

half3 Refl2D(half3 viewDir, half3 normal)
{
    half3 reflDir = reflect(-viewDir, normal);
    half isPositive = half(reflDir.z > 0.0h);
    half positiveWeight = ((isPositive * 2.0h) - 1.0h);
    half2 envUV = (reflDir.xy / ((reflDir.z * positiveWeight) + 1.0h));
    half2 envOffset = half2(0.25h + 0.5h *  (1.h - isPositive), 0.25h + 0.5h * isPositive);
    envUV = (envUV * half2(-0.25h, 0.25h)) + envOffset;
    half4 envColorSample = tex2Dlod(_EnvTex, half4(envUV, 0, 0));
    half3 envColor = (envColorSample.xyz * ((envColorSample.w * envColorSample.w) * 16.0h));//
    return envColor;
}

half3 ReflSea2D(half3 viewDir, half3 normal)
{
    half3 reflDir = reflect(-viewDir, normal);
    half isPositive = half(reflDir.z > 0.0h);
    half positiveWeight = ((isPositive * 2.0h) - 1.0h);
    half2 envUV = (reflDir.xy / ((reflDir.z * positiveWeight) + 1.0h));
    half2 envOffset = half2(0.25h + 0.5h *  (1.h - isPositive), 0.25h + 0.5h * isPositive);
    envUV = (envUV * half2(-0.25h, 0.25h)) + envOffset;
    half4 envColorSample = tex2Dlod(_SeaEnvTex, half4(envUV, 0, 0));
    half3 envColor = (envColorSample.xyz * ((envColorSample.w * envColorSample.w) * 16.0h));//
    return envColor;
}

half3 ReflSeaBack2D(half3 viewDir, half3 normal, sampler2D_half cubeTex)
{
    half3 reflDir = reflect(-viewDir, normal);
    half isPositive = half(reflDir.z > 0.0h);
    half positiveWeight = ((isPositive * 2.0h) - 1.0h);
    half2 envUV = (reflDir.xy / ((reflDir.z * positiveWeight) + 1.0h));
    half2 envOffset = half2(0.25h + 0.5h *  (1.h - isPositive), 0.25h + 0.5h * isPositive);
    envUV = (envUV * half2(-0.25h, 0.25h)) + envOffset;
    half4 envColorSample = tex2Dlod(cubeTex, half4(envUV, 0, 0));
    half3 envColor = (envColorSample.xyz * ((envColorSample.w * envColorSample.w) * 16.0h));//
    return envColor;
}
#endif

#ifdef _CHARACTER_LIGHT_ON
#define _DARKWORLD_CLOSE 1
#endif

//// DarkWorld
#ifdef _DARKWORLD_CLOSE
half _DarkCharacterScale = 0;
half _DarkCharacterCtrl;
#define APPLY_DARK_WORLD(color) color.rgb *= 1.h - _DarkCharacterScale * _DarkCharacterCtrl;
#define UNDO_APPLY_DARK_WORLD(color) color.rgb = min(color.rgb / max(1.h - _DarkCharacterScale, 0.01h), 5.h);
#else
half4 _DarkWorldScale = 0;
#define APPLY_DARK_WORLD(color) color.rgb *= 1.h - _DarkWorldScale.x;
#define APPLY_SKY_DARK_WORLD(color) color.rgb *= 1.h - _DarkWorldScale.y;
#define UNDO_APPLY_DARK_WORLD(color) color.rgb = min(color.rgb / max(1.h - _DarkWorldScale.x, 0.01h), 5.h);
#endif

//// Auto Exposure
half _AutoExposure = 1;
#define APPLY_AUTO_EXPOSURE(color) color.rgb *= _AutoExposure;

//// SimpleAlbedo
#if _DEBUG_SIMPLEALBEDO
fixed _DEBUG_AlbedoGrayScale;
#endif

#ifndef SHOW_TEXTURE
half3 NormalTex = 0;
half3 NormalTexVertex = 0;
#endif

half3 DebugAlbedo(half3 albedo, half metallic)
{
    if (metallic<0.1)
    {
        if(albedo.r<0.015*0.6)
            return half3(1,0,0);
        if(albedo.g<0.015*0.6)
            return half3(1,0,0);
        if(albedo.b<0.015*0.6)
            return half3(1,0,0);
        if(albedo.r>0.9)
            return half3(1,0,0);
        if(albedo.g>0.9)
            return half3(1,0,0);
        if(albedo.b>0.9)
            return half3(1,0,0);
        return albedo;
    }
    else
    {
        if(albedo.r<0.542*0.6)
            return half3(1,0,0);
        if(albedo.g<0.497*0.6)
            return half3(1,0,0);
        if(albedo.b<0.344*0.6)
            return half3(1,0,0);
        return albedo;
    }
}
half3 DebugMetallic(half3 albedo, half metallic)
{
    if (metallic>0.01)
    {
        if(metallic<0.99)
            return half3(1,0,0);
        return metallic;
    }
    else
    {
        return metallic;
    }
}

#if defined(_DEBUG_SIMPLEALBEDO) || defined(_DEBUG_ALBEDO) || defined(_DEBUG_ERROR_ALBEDO) || defined(_DEBUG_ERROR_METALLIC) || defined(_DEBUG_SMOOTHNESS) || defined(_DEBUG_METALLIC) || defined(_DEBUG_NORMAL) || defined(_DEBUG_NORMAL_VERTEX)
    #define _DEBUG_TEXTURE_OPEN 1
#endif

#if defined(POINT) || defined(SPOT)
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #if _DEBUG_TEXTURE_OPEN
        #define TEXTURE_DEBUG(o,color) color.rgb = 0;
    #else
        #define TEXTURE_DEBUG(o,color)
    #endif
#else
    #if _DEBUG_SIMPLEALBEDO
    #define APPLY_SIMPLE_ALBEDO(o) o.Albedo = _DEBUG_AlbedoGrayScale;
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color)
    #elif _DEBUG_TEX_AO
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = o.Occlusion;
    #elif _DEBUG_ALBEDO
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = o.Albedo;
    #elif _DEBUG_ERROR_ALBEDO
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = DebugAlbedo(o.Albedo,o.Metallic);
    #elif _DEBUG_ERROR_METALLIC
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = DebugMetallic(o.Albedo,o.Metallic);
    #elif _DEBUG_SMOOTHNESS
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = o.Smoothness;
    #elif _DEBUG_METALLIC
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = o.Metallic;
    #elif _DEBUG_NORMAL
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = NormalTex;
    #elif _DEBUG_NORMAL_VERTEX
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color) color.rgb = NormalTexVertex;
    #else
    #define APPLY_SIMPLE_ALBEDO(o)
    #define APPLY_SIMPLE_ALBEDO_FRAG(col)
    #define TEXTURE_DEBUG(o,color)
    #endif
#endif

#define _CUBE_BASE 0.4h

// Output Alpha
half _OutputAlpha;
#define APPLY_OUTPUT_ALPHA(color) color.r = lerp(color.r, color.a, _OutputAlpha);

//// Fog
#if !defined (_DISABLE_FOG)
#define _FOG
#endif

half4 _AmbientColor;
half3 _ViewDir;

#define FOG_INPUT half4 fogCoord;
#define FOG_INPUT_V2F(idx1) half4 fogCoord : TEXCOORD##idx1;

inline half3 GetAmbient(half3 normalWorld)
{
    half3 ambient = _AmbientColor.rgb;
    return ambient;
}

#if defined(_TRANSFORM_NORMAL)
#define TRANSFER_NORMAL(o,normal) //o.VertexNormal = normalize(UnityObjectToWorldNormal(normal))
#else
#define TRANSFER_NORMAL(o,normal)
#endif

#if defined(_MOON_SCATTERING_ON)
half4 _SkyboxSunDir;
half4 _SkyboxMoonDir;
#endif

#if !defined(UNITY_PASS_FORWARDADD) && defined(_FOG)

half4 _FogInfo;
half4 _FogInfo2;
half4 _FogInfo3;
half4 _FogColor1;
half4 _FogColor2;
half4 _FogColor3;

half4 FogVertex(float3 localPos)
{
    float3 worldPos = mul(unity_ObjectToWorld, float4(localPos, 1.0)).xyz;
#ifndef _DISABLE_PLATFORM_SPACE_FOG
    worldPos = PlatformToWorldPos(worldPos);
#endif
    half4 fogParams = 0;
    MALI_FLOAT3 viewDir = UnityWorldSpaceViewDir(worldPos);
    MALI_FLOAT dotViewDir = dot(viewDir,viewDir);

    MALI_FLOAT fHeightCoef = clamp(-worldPos.y * _FogInfo2.y + _FogInfo2.x, 0.0h, 1.0h);
    fHeightCoef = fHeightCoef * fHeightCoef;
    fHeightCoef = fHeightCoef * fHeightCoef;
    MALI_FLOAT tmpvar_16 = 1.0h - exp(-max(0.0h, sqrt(dotViewDir) - _FogInfo.x) * max(_FogInfo.y * fHeightCoef, _FogInfo.z * _FogInfo.y));
    fogParams.xyz = normalize(viewDir);
    MALI_FLOAT a = _FogInfo.w;
    fogParams.w = tmpvar_16 / (tmpvar_16 * a + (1.h - a));
    fogParams.w = min(fogParams.w, _FogInfo3.w);

#ifdef _VFOG
    fixed eyeCos = saturate(dot(-_WorldSpaceLightPos0.xyz, fogParams.xyz));
    fixed eyeCos2 = eyeCos * eyeCos;

    half3 fogColor = lerp(_FogColor1.rgb, _FogColor2.rgb, saturate(-fogParams.y * _FogInfo2.w + _FogInfo2.z));
    fogColor += _FogInfo3.z * _FogColor3.rgb * eyeCos2;
    fogParams.xyz = fogColor.rgb;
#endif

    return fogParams;
}

half4 FogVertexVFog(float3 localPos)
{
    half4 fogParams = FogVertex(localPos);
    return fogParams;
}

void FogPixelSimple(half4 fogParams, inout fixed4 color)
{
    half3 fogColor = lerp(_FogColor1.rgb, _FogColor2.rgb, saturate(-fogParams.y * _FogInfo2.w + _FogInfo2.z));
    color.rgb = lerp(color.rgb, color.rgb * (1.0h - fogParams.w) + fogColor, fogParams.w);
}

void FogPixel(half4 fogParams, inout fixed4 color)
{
#ifdef _VFOG
    color.rgb = lerp(color.rgb, color.rgb * (1.0h - fogParams.w) + fogParams.rgb, fogParams.w);
#else
    half eyeCos = saturate(dot(-_WorldSpaceLightPos0.xyz, fogParams.xyz));
    half eyeCos2 = eyeCos * eyeCos;

    half3 fogColor = lerp(_FogColor1.rgb, _FogColor2.rgb, saturate(-fogParams.y * _FogInfo2.w + _FogInfo2.z));
    fogColor += _FogInfo3.z * _FogColor3.rgb * eyeCos2;
    color.rgb = lerp(color.rgb, color.rgb * (1.0h - fogParams.w) + fogColor, fogParams.w);
#endif
}

void FogPixelVFog(half4 fogParams, inout half4 color)
{
    color.rgb = lerp(color.rgb, color.rgb * (1.0h - fogParams.w) + fogParams.rgb, fogParams.w);
}

void ColorFogPixel(half4 fogParams, inout half4 color, half3 backColor, half3 dyeColor, half dyeLerp, half alpha)
{
    half eyeCos = saturate(dot(-_WorldSpaceLightPos0.xyz, fogParams.xyz));
    half eyeCos2 = eyeCos * eyeCos;

    half3 fogColor = lerp(_FogColor1.rgb, _FogColor2.rgb, saturate(-fogParams.y * _FogInfo2.w + _FogInfo2.z));
    fogColor += _FogInfo3.z * _FogColor3.rgb * eyeCos2;
    //defog
    fogParams.w = min(0.99h, fogParams.w);
    UNDO_APPLY_DARK_WORLD(backColor);
    half3 oriBackColor = (backColor - fogParams.w * fogColor) / ((1.0h - fogParams.w) * (1.0h + fogParams.w));
    //dye
    half3 dyeOriBackColor = lerp(oriBackColor, oriBackColor * dyeColor, dyeLerp);
    //apply alpha
    color.rgb = lerp(dyeOriBackColor, color.rgb, alpha);
    //fog
    color.rgb = lerp(color.rgb, color.rgb * (1.0h - fogParams.w) + fogColor, fogParams.w);
}

void AlphaFogPixel(half4 fogParams, inout half4 color, half alpha)
{
    half eyeCos = saturate(dot(-_WorldSpaceLightPos0.xyz, fogParams.xyz));
    half eyeCos2 = eyeCos * eyeCos;

    half3 fogColor = lerp(_FogColor1.rgb, _FogColor2.rgb, saturate(-fogParams.y * _FogInfo2.w + _FogInfo2.z));
    fogColor += _FogInfo3.z * _FogColor3.rgb * eyeCos2;
    fogParams.w *= alpha;
    color.rgb = lerp(color.rgb, color.rgb * (1.0h - fogParams.w) + fogColor, fogParams.w);
}

void SkyFogPixel(half4 fogParams, inout half4 color)
{
    half tmpvar_fog = saturate (fogParams.y / max(_FogInfo3.y, 0.001h) + _FogInfo3.x);
    tmpvar_fog = tmpvar_fog * tmpvar_fog * (4.h - 3.h * tmpvar_fog);
    fogParams.w = tmpvar_fog;
    fogParams.w = min(0.99h, fogParams.w);

    half fogEyeCos = saturate(dot (-_WorldSpaceLightPos0.xyz, fogParams.xyz));
    half fogEyeCos2 = fogEyeCos * fogEyeCos;

    half3 fogColor = lerp(_FogColor1.rgb, _FogColor2.rgb, saturate (-fogParams.y * _FogInfo2.w + _FogInfo2.z));
    fogColor += _FogInfo3.z * _FogColor3.rgb * fogEyeCos2;
    color.rgb = lerp(color.rgb, fogColor, fogParams.w);
}

#define FOG_VERTEX(o, localPos) o.fogCoord = FogVertex(localPos);
#define FOG_PIXEL(i, color) FogPixel(i.fogCoord, color);
#define FOG_PIXEL_SIMPLE(i, color) FogPixelSimple(i.fogCoord, color);
#define COLOR_FOG_PIXEL(i, color, backColor, dyeColor, dyeLerp, alpha) ColorFogPixel(i.fogCoord, color, backColor, dyeColor, dyeLerp, alpha);
#define ALPHA_FOG_PIXEL(i, color, alpha) AlphaFogPixel(i.fogCoord, color, alpha);
#define SKYBOX_FOG(i, color) SkyFogPixel(i.fogCoord, color);
#else
#define FOG_VERTEX(o, localPos)
#define FOG_PIXEL(i, color)
#define FOG_PIXEL_SIMPLE(i, color)
#define COLOR_FOG_PIXEL(i, color, backColor, dyeColor, dyeLerp, alpha)
#define ALPHA_FOG_PIXEL(i, color, alpha)
#define SKYBOX_FOG(i, color, y)
#endif

//// Vegetation Interaction
float3 _PlayerPos;
void VegetationInteraction(inout half3 localPos, float3 worldPos, half3 anchor, half strength, half range)
{
    half3 delta = worldPos.xyz - (_PlayerPos.xyz + half3(0.h, 0.8h, 0.h));
    half playerLen = length(delta.xyz);
    half offsetStrength = saturate(lerp(strength, 0, playerLen / max(range, 0.001h)));
    half3 offsetDir = normalize(mul(delta.xyz / max(playerLen, 0.001h), (float3x3)unity_ObjectToWorld));

#ifdef GEOM_TYPE_LEAF
    localPos.xyz -= anchor.xyz;
    localPos.xyz += half3(0.001h, 0.h, 0.h);
    half len = length(localPos.xyz);
#endif
    localPos.xz += offsetDir.xz * offsetStrength;
#ifdef GEOM_TYPE_LEAF
    localPos.xyz = normalize(localPos.xyz) * len;
    localPos.xyz += anchor.xyz;
#endif
}

void VegetationInteraction(inout half3 localPos, half3 anchor, half strength, half range)
{
    half3 worldPos = mul(unity_ObjectToWorld, half4(localPos, 1.h)).xyz;
    VegetationInteraction(localPos, worldPos, anchor, strength, range);
}

//// MICROS

#define PANGU_VERT(v, o) UNITY_TRANSFER_FOG(o, UnityObjectToClipPos(v.vertex));\
    FOG_VERTEX(o, v.vertex);\
    TRANSFER_NORMAL(o,v.normal);

#define PANGU_SURF(IN, o) APPLY_SIMPLE_ALBEDO(o);

#define PANGU_FINAL_SIMPLE(IN, o, color) APPLY_AUTO_EXPOSURE(color);\
    FOG_PIXEL_SIMPLE(IN, color);\
    APPLY_DARK_WORLD(color);\
    TEXTURE_DEBUG(o,color);

#define PANGU_FINAL(IN, o, color) APPLY_AUTO_EXPOSURE(color);\
    FOG_PIXEL(IN, color);\
    APPLY_DARK_WORLD(color);\
    TEXTURE_DEBUG(o,color);

#define PANGU_FRAG(IN, color) APPLY_AUTO_EXPOSURE(color);\
    APPLY_SIMPLE_ALBEDO_FRAG(color);\
    FOG_PIXEL(IN, color);\
    APPLY_DARK_WORLD(color);\

#define PANGU_COLOR_FRAG(IN, color, backColor, dyeColor, dyeLerp, alpha) APPLY_AUTO_EXPOSURE(color);\
    APPLY_SIMPLE_ALBEDO_FRAG(color);\
    COLOR_FOG_PIXEL(IN, color, backColor, dyeColor, dyeLerp, alpha);\
    APPLY_DARK_WORLD(color);\

#define PANGU_ALPHA_FRAG(IN, color, alpha) APPLY_AUTO_EXPOSURE(color);\
    APPLY_SIMPLE_ALBEDO_FRAG(color);\
    ALPHA_FOG_PIXEL(IN, color, alpha);\
    APPLY_DARK_WORLD(color);\

#define PANGU_OUTPUT_ALPHA(color) APPLY_OUTPUT_ALPHA(color);

#endif //PANGU_COMMON