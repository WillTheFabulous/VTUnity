//####COMMON Begin####
Shader "Tianyu Shaders/Terrain/TerrainBlendPBR"
//####COMMON End####
//####MOBILE Begin####
//Shader "Mobile/Tianyu Shaders/Terrain/TerrainBlendPBR"
//####MOBILE End####
//####LOW Begin####
//Shader "Low/Tianyu Shaders/Terrain/TerrainBlendPBR"
//####LOW End####

{
Properties{
        [Toggle]_VertexBlend ("_VertexBlendMode", Float) = 0

        [NoScaleOffset]_Control("SplatMap", 2D) = "black" {}

        [Toggle]_HeightBlend("HeightBlend", Float) = 0.2

        [NoScaleOffset]_BigNormal ("BigNormal", 2D) = "bump" {}

        _Splat0 ("Albedo0(RGB)", 2D) = "white" {}
        [Normal]_Normal0 ("NormalMap", 2D) = "bump" {}
        _EmissionTex0 ("Emission Texture 0", 2D) = "black" {}
        _Param0 ("ParamMap", 2D) = "white" {}
        [HideInInspector]_CombinedProps0 ("_CombinedProps0", vector) = (1.0, 0.0, 1., 0.2)

        _Splat1 ("Albedo1(RGB)", 2D) = "white" {}
        [Normal]_Normal1 ("NormalMap", 2D) = "bump" {}
        _EmissionTex1 ("Emission Texture 1", 2D) = "black" {}
        _Param1 ("ParamMap", 2D) = "white" {}
        [HideInInspector]_CombinedProps1 ("_CombinedProps1", vector) = (1.0, 0.0, 1., 0)

        _Splat2 ("Albedo2(RGB)", 2D) = "white" {}
        [Normal]_Normal2 ("NormalMap", 2D) = "bump" {}
        _EmissionTex2 ("Emission Texture 2", 2D) = "black" {}
        _Param2 ("ParamMap", 2D) = "white" {}
        
        [HideInInspector]_CombinedProps2 ("_CombinedProps2", vector) = (1.0, 0.0, 1., 0)

        _Splat3 ("Albedo2(RGB)", 2D) = "white" {}
        [Normal]_Normal3 ("NormalMap", 2D) = "bump" {}
        _Param3 ("ParamMap", 2D) = "white" {}
        [HideInInspector]_CombinedProps3 ("_CombinedProps3", vector) = (1.0, 0.0, 1., 0)
        [HideInInspector]_CombinedProps4 ("_CombinedProps4", vector) = (1, 1, 1, 1)
        [HideInInspector]_CombinedProps5 ("_CombinedProps5", vector) = (1, 1, 1, 1)
        [HideInInspector]_CombinedProps6 ("_CombinedProps6", vector) = (0, 0, 0, 0) // Emission Intensities

        //use for saving the blendType
        [HideInInspector]_BlendType ("BlendType", Float) = 3
        [HideInInspector]_NormalType ("NormalType", Float) = 0
    }

    CGINCLUDE
        #pragma optimizeperdraw
        
        CBUFFER_START(UnityPerMaterial)
        #ifdef SHADER_API_METAL
            float4 _CombinedProps0;
            float4 _CombinedProps1;
            float4 _CombinedProps2;
            float4 _CombinedProps3;
            float4 _CombinedProps4;
            float4 _CombinedProps5;
            float4 _CombinedProps6;
            float4 _Splat0_ST;
            float4 _Splat1_ST;
            float4 _Splat2_ST;
            float4 _Splat3_ST;
        #else
            half4 _CombinedProps0;
            half4 _CombinedProps1;
            half4 _CombinedProps2;
            half4 _CombinedProps3;
            half4 _CombinedProps4;
            half4 _CombinedProps5;
            half4 _CombinedProps6;
            half4 _Splat0_ST;
            half4 _Splat1_ST;
            half4 _Splat2_ST;
            half4 _Splat3_ST;
        #endif
        CBUFFER_END
        
        //----------------------------------------------
        #define _Smoothness0 _CombinedProps0.x
        #define _Metallic0 _CombinedProps0.y
        #define _Height0 _CombinedProps0.z
        #define _HeightWeight _CombinedProps0.w
        //----------------------------------------------
        
        #define _Smoothness1 _CombinedProps1.x
        #define _Metallic1 _CombinedProps1.y
        #define _Height1 _CombinedProps1.z
        //----------------------------------------------
        #define _Smoothness2 _CombinedProps2.x
        #define _Metallic2 _CombinedProps2.y
        #define _Height2 _CombinedProps2.z
        //----------------------------------------------
        #define _Smoothness3 _CombinedProps3.x
        #define _Metallic3 _CombinedProps3.y
        #define _Height3 _CombinedProps3.z
        //----------------------------------------------
        #define _SnowWhiteFactor0 _CombinedProps4.x
        #define _SnowWhiteFactor1 _CombinedProps4.y
        #define _SnowWhiteFactor2 _CombinedProps4.z
        #define _SnowWhiteFactor3 _CombinedProps4.w
        //----------------------------------------------
        #define _CausticsIntensity0 _CombinedProps5.x
        #define _CausticsIntensity1 _CombinedProps5.y
        #define _CausticsIntensity2 _CombinedProps5.z
        #define _CausticsIntensity3 _CombinedProps5.w
        //----------------------------------------------
        #define _EmissionIntensity0 _CombinedProps6.x
        #define _EmissionIntensity1 _CombinedProps6.y
        #define _EmissionIntensity2 _CombinedProps6.z
        //----------------------------------------------
    ENDCG



    SubShader{
        Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+200" "PassFlags "="OnlyDirectional" "ShaderBatcher"="Enable"}
        LOD 400

        CGPROGRAM
//####COMMON Begin####
        #pragma surface surf PanguPBS vertex:vert finalcolor:final nodynlightmap novertexlights fullforwardshadows noambient nofog nolppv noLightCoords nometa highprecisetbn
//####COMMON End####
//####LOW Begin####
        // #pragma surface surf PanguPBSLow vertex:vert finalcolor:final nodynlightmap novertexlights fullforwardshadows noambient nofog nolppv noLightCoords nometa highprecisetbn
//####LOW End####

        #pragma target 3.0
        #pragma multi_compile _ SHADERBATCH_INSTANCING_ON
        #pragma instancing_options nolightmap
        #pragma multi_compile _ _RAINMAP _CAUSTICS
        #pragma multi_compile _ _BIG_NORMAL _UNITY_TERRAIN _UNITY_TERRAIN_LOD
        #pragma multi_compile _ _EMISSIVE
        //#pragma shader_feature _ _DEBUG_SPECULAR _DEBUG_DIFFUSE _DEBUG_AO _DEBUG_SPECULAR _DEBUG_DIFFUSE _DEBUG_AO _DEBUG_LIGHTMAP _DEBUG_LIGHTMAP_SHADOW _DEBUG_SIMPLEALBEDO _DEBUG_ALBEDO _DEBUG_ERROR_ALBEDO _DEBUG_ERROR_METALLIC _DEBUG_SMOOTHNESS _DEBUG_METALLIC _DEBUG_NORMAL _BAKE_LOD2_ON

        #pragma force_include_variants LIGHTMAP_ON
        #pragma skip_combined_variants _EMISSIVE _CAUSTICS
        #pragma skip_combined_variants _EMISSIVE _UNITY_TERRAIN
        #pragma skip_combined_variants _EMISSIVE _UNITY_TERRAIN_LOD

#ifndef SHADER_API_MOBILE
        #define _SCENE_SOFT_SHADOW 1
#endif
//####COMMON Begin####
//####COMMON End####
//####MOBILE Begin####
        // #ifdef _UNITY_TERRAIN_LOD
        //     #define _NO_NORMAL
        // #endif
        // #define _NO_DIRLIGHTMAP
//####MOBILE End####
        #ifndef _ENABLE_IBL
            #define _NO_IBL 1
        #endif
        #ifdef _RAINMAP
            //下雨时关方向图、开可过渡的IBL
            #undef _NO_IBL
            #define _LERP_IBL 1
        #endif

        #define _SIMPLE_SNOW 1
//####LOW Begin####
//      #define _LOW_RAINSNOW
//      #define _NO_NORMAL
//      #ifdef _CAUSTICS
//          #undef _CAUSTICS
//      #endif
//####LOW End####
        #define _TRANSFORM_NORMAL 1 

        #ifdef _EMISSIVE
            #define _TRI_BLEND
        #endif

        #include "Assets/Res/Shader/Shaders/PanguCommon.cginc"
        #include "Assets/Res/Shader/Shaders/PanguGI.cginc"
        #include "Assets/Res/Shader/Shaders/PanguCustomPBSLighting.cginc"
        #include "Assets/Res/Shader/Shaders/Scene/Prop/ScenePropEffect.cginc"
        #include "Assets/Res/Shader/Shaders/Scene/Terrain/TerrainCommon.cginc"

        sampler2D_half _Control;
        sampler2D_half _BigNormal;
        sampler2D_half _Splat0, _Splat1, _Splat2, _Splat3;
        sampler2D_half _Normal0, _Normal1, _Normal2, _Normal3;
        sampler2D_half _EmissionTex0, _EmissionTex1, _EmissionTex2;
        sampler2D_half _Param0, _Param1, _Param2, _Param3;

        struct Input {
            half2 UVControl : TEXCOORD0;
    #if defined(_UNITY_TERRAIN) || defined(_UNITY_TERRAIN_LOD)
            float2 UVSplat0 : TEXCOORD1;
            float2 UVSplat1 : TEXCOORD2;
            float2 UVSplat2 : TEXCOORD3;
        #ifndef _TRI_BLEND
            float2 UVSplat3 : TEXCOORD4;
        #endif
    #else
            half2 uv_Splat0 : TEXCOORD1;
            half2 uv_Splat1 : TEXCOORD2;
            half2 uv_Splat2 : TEXCOORD3;
        #ifndef _TRI_BLEND
            half2 uv_Splat3 : TEXCOORD4;
        #endif
    #endif
            FOG_INPUT
            UNREAL_LIGHTMAP_INPUT
    #if defined(_RAINMAP) || defined(_CAUSTICS)
            float3 worldPos;
    #endif 
    #if defined(_RAINMAP) || defined(_CAUSTICS) || defined(_LOW_RAINSNOW)
            half3 worldNormal;
            INTERNAL_DATA
    #endif
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
        #if defined(_UNITY_TERRAIN) || defined(_UNITY_TERRAIN_LOD)
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
            o.UVSplat0 = worldPos.xz * _Splat0_ST.xy + _Splat0_ST.zw;
            o.UVSplat1 = worldPos.xz * _Splat1_ST.xy + _Splat1_ST.zw;
            o.UVSplat2 = worldPos.xz * _Splat2_ST.xy + _Splat2_ST.zw;
        #ifndef _TRI_BLEND
            o.UVSplat3 = worldPos.xz * _Splat3_ST.xy + _Splat3_ST.zw;
        #endif
            v.tangent.xyz = cross(v.normal, float3(0, 0, 1));
            v.tangent.w = -1;
        #endif
            o.UVControl = v.texcoord;
            PANGU_VERT(v, o);
            TRANSFER_UNREAL_LIGHTMAP_VERT(v, o);
        }

        void surf(Input IN, inout PanguSurfaceOutputStandard o)
        {
    #if defined(_UNITY_TERRAIN) || defined(_UNITY_TERRAIN_LOD)
            fixed4 albedo0 = tex2D(_Splat0, IN.UVSplat0);
            fixed4 albedo1 = tex2D(_Splat1, IN.UVSplat1);
            fixed4 albedo2 = tex2D(_Splat2, IN.UVSplat2);
        #ifndef _TRI_BLEND
            fixed4 albedo3 = tex2D(_Splat3, IN.UVSplat3);
        #endif
        #ifndef _NO_NORMAL
            fixed4 normal0 = tex2D(_Normal0, IN.UVSplat0);
            fixed4 normal1 = tex2D(_Normal1, IN.UVSplat1);
            fixed4 normal2 = tex2D(_Normal2, IN.UVSplat2);
        #ifndef _TRI_BLEND
            fixed4 normal3 = tex2D(_Normal3, IN.UVSplat3);
        #endif
        #endif
        #if _EMISSIVE
            fixed4 emission0 = tex2D(_EmissionTex0, IN.UVSplat0) * _EmissionIntensity0;
            fixed4 emission1 = tex2D(_EmissionTex1, IN.UVSplat1) * _EmissionIntensity1;
            fixed4 emission2 = tex2D(_EmissionTex2, IN.UVSplat2) * _EmissionIntensity2;
        #endif
    #else
            fixed4 albedo0 = tex2D(_Splat0, IN.uv_Splat0);
            fixed4 albedo1 = tex2D(_Splat1, IN.uv_Splat1);
            fixed4 albedo2 = tex2D(_Splat2, IN.uv_Splat2);
        #ifndef _TRI_BLEND
            fixed4 albedo3 = tex2D(_Splat3, IN.uv_Splat3);
        #endif
        #ifndef _NO_NORMAL
            fixed4 normal0 = tex2D(_Normal0, IN.uv_Splat0);
            fixed4 normal1 = tex2D(_Normal1, IN.uv_Splat1);
            fixed4 normal2 = tex2D(_Normal2, IN.uv_Splat2);
        #ifndef _TRI_BLEND
            fixed4 normal3 = tex2D(_Normal3, IN.uv_Splat3);
        #endif
        #endif
        #if _EMISSIVE
            fixed4 emission0 = tex2D(_EmissionTex0, IN.uv_Splat0) * _EmissionIntensity0;
            fixed4 emission1 = tex2D(_EmissionTex1, IN.uv_Splat1) * _EmissionIntensity1;
            fixed4 emission2 = tex2D(_EmissionTex2, IN.uv_Splat2) * _EmissionIntensity2;
        #endif
    #endif

            half snowWhiteFactor = 0;
            half4 factor = 0;
            half4 blendAlbedo = 0;
            half4 blendNormal = 0;
            half smoothness = 0;
            half causticsIntensity = 0;
            half3 emission = 0;

            factor.rgb = tex2D(_Control, IN.UVControl).rgb;
            factor.a = max(1.0h - factor.r - factor.g - factor.b, 0.h);

#ifdef _TRI_BLEND
    #ifdef _NO_NORMAL
            factor = heightAdjust(_Height0, _Height1, _Height2, factor.rgb);
    #else
            factor.rgb = heightAdjust(normal0.b * _Height0, normal1.b * _Height1, normal2.b * _Height2, factor.rgb);
            blendNormal = triblend(normal0, normal1, normal2, factor.rgb);
    #endif
            blendAlbedo = triblend(albedo0, albedo1, albedo2, factor.rgb);
    #ifdef _EMISSIVE
            emission = triblend(emission0, emission1, emission2, factor.rgb);
    #endif
            smoothness = triblend(_Smoothness0, _Smoothness1, _Smoothness2, factor.rgb);
            snowWhiteFactor = triblend(_SnowWhiteFactor0, _SnowWhiteFactor1, _SnowWhiteFactor2, factor.rgb);
    #ifdef _CAUSTICS
            causticsIntensity = triblend(_CausticsIntensity0, _CausticsIntensity1, _CausticsIntensity2, factor.rgb);
    #endif
#else
    #ifdef _NO_NORMAL
            factor = heightAdjust(_Height0, _Height1, _Height2, _Height3, factor);
    #else
            factor = heightAdjust(normal0.b * _Height0, normal1.b * _Height1, normal2.b * _Height2, normal3.b * _Height3, factor);
            blendNormal = fourblend(normal0, normal1, normal2, normal3, factor);
    #endif
            blendAlbedo = fourblend(albedo0, albedo1, albedo2, albedo3, factor);
            smoothness = fourblend(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3, factor);
            snowWhiteFactor = fourblend(_SnowWhiteFactor0, _SnowWhiteFactor1, _SnowWhiteFactor2, _SnowWhiteFactor3, factor);
    #ifdef _CAUSTICS
            causticsIntensity = fourblend(_CausticsIntensity0, _CausticsIntensity1, _CausticsIntensity2, _CausticsIntensity3, factor);
    #endif
#endif
            
            o.Albedo = blendAlbedo.rgb;
            o.Emission = emission;
            o.Occlusion = 1.0h;

#ifndef _NO_NORMAL
            o.Normal = fixed3(blendNormal.x, blendNormal.y, 1) * 2.h - 1.h;
        #ifdef _BIG_NORMAL
            half3 bigNormal = tex2D(_BigNormal, IN.UVControl);
            bigNormal = bigNormal * 2.h - 1.h;
            bigNormal.z = 0.h; 
            o.Normal = bigNormal+o.Normal;
        #endif
            NormalTex = (o.Normal+1.h)/2.h;
            NormalTex.z = 1.h;
#endif

            o.Smoothness = blendAlbedo.a * smoothness;
            o.Metallic = 0;
            
            o.Normal.rg *= 1.5h;
            APPLY_UNREAL_LIGHTMAP(IN);

    #ifdef _RAINMAP
        #ifdef _LOW_RAINSNOW
            APPLY_LODRAIN_EFFECT(o)
            APPLY_LODSNOW_EFFECT(WorldNormalVector(IN, o.Normal), o)
        #else
            fixed3 normalWorld = WorldNormalVector(IN, o.Normal);
            half4 snowParamValues = 0;
            APPLY_RAIN_EFFECT(IN, normalWorld, o, snowParamValues)
            APPLY_SNOW_EFFECT(normalWorld, o, snowWhiteFactor, snowParamValues, 1)
        #endif
    #endif
        #ifdef _CAUSTICS
            _CausticsGlobalParams1.y *= causticsIntensity;
        #endif
            CAUSTICS_SURF(IN, o, IN.UVControl);
            // APPLY_CAUSTICS(IN.uv_Control, IN.worldPos, o);
            o.Alpha = 1.0h;
            PANGU_SURF(IN, o);
        }

        void final (Input IN, PanguSurfaceOutputStandard o, inout fixed4 color)
        {
            PANGU_FINAL(IN, o, color);
        }
        ENDCG
    }

    CustomEditor "TerrainBlendPBRShaderGUI"
//####SKIP Begin####
    FallBack "Legacy Shaders/VertexLit"
//####SKIP End####
}
