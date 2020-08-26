using UnityEngine;
using Pangu.Const;

namespace Pangu.Weather
{
    static public class WeatherConst
    {
        public const string RAIN_PARTICLE_PATH = "Scene/common/weather/rain_particles/rain_drop.prefab";
        public const string DISTANT_RAIN_PARTICLE_PATH = "Scene/common/weather/rain_particles/distant_rain_drop.prefab";
        public const string DEFAULT_RAIN_ENVTEX_PATH = "Scene/cube/textures/default_rain_cube2d.png";
        public const string RAIN_RIPPLE_TEX_FORMAT = ResSceneConst.WEATHER_RAIN_RIPPLE_TEX_FORMAT;
        public const string RAIN_SPLASH_PARTICLE_PATH = ResSceneConst.WEATHER_RAIN_SPLASH_PARTICLE_PATH;
        public const string RAIN_SPLASH_DATA_FORMAT = ResSceneConst.WEATHER_RAIN_SPLASH_DATA_FORMAT;
        public const string RAIN_SCREEN_PARTICLE_PATH = ResSceneConst.WEATHER_RAIN_SCREEN_PARTICLE_PATH;
        public const string SNOW_SCREEN_PARTICLE_PATH = ResSceneConst.WEATHER_SNOW_SCREEN_PARTICLE_PATH;
        public const string SNOW_PARAMS_TEX_PATH = ResSceneConst.WEATHER_SNOW_PARAMS_TEX_PATH;
        public const string STARRY_PREFAB_PATH = ResSceneConst.WEATHER_STARRY_PREFAB_PATH;
        public const string GALAXY_PREFAB_PATH = ResSceneConst.WEATHER_GALAXY_PREFAB_PATH;
        public const string MOON_PREFAB_PATH = ResSceneConst.WEATHER_MOON_PREFAB_PATH;
        public const string LIGHTNING_CLOUD_PATH = ResSceneConst.WEATHER_LIGHTNING_CLOUD_PATH;
        public const string WEATHER_SHADER_WARMUP_ASSET_PATH = ResSceneConst.WEATHER_SHADER_WARMUP_ASSET_PATH;
        public const string WEATHER_TRANSITION_SETTINGS_PATH = ResSceneConst.WEATHER_TRANSITION_SETTINGS_PATH;
        public const string DEFAULT_LIGHTNING_MODEL = ResSceneConst.WEATHER_DEFAULT_LIGHTNING_MODEL;
        public const string THUNDER_AUDIO_EVENT = ResAudioConst.EVENT_WEATHER_THUNDER;
        public const string WEATHER_PRESET_LIBRARY_PATH = ResSceneConst.WEATHER_PRESET_LIBRARY_PATH;
        public const string WEATHER_PROFILE_PATH_FORMAT = ResSceneConst.WEATHER_PROFILE_PATH_FORMAT;
        public const string DEFAULT_TEMPLATE_PATH = ResSceneConst.WEATHER_DEFAULT_TEMPLATE_PATH;

        public const string MOON_TEX_PATH = "Scene/common/weather/moon_tex.tga";
        public const string CLOUD_PREFAB_PATH = "Scene/common/weather/cloud/cloud.prefab";
        public const string CLOUD_LEGACY_PREFAB_PATH = "Scene/common/weather/cloud/cloud_legacy.prefab";
        public const string CLOUD_DEFAULT_TEX = "Scene/common/weather/cloud/cloud_map.png";
        public const string SKYBOX_PREFAB_PATH = "Scene/common/weather/skybox/skybox.prefab";
        public const string SKYBOX_LEGACY_PREFAB_PATH = "Scene/common/weather/skybox/skybox_legacy.prefab";

        public const string LIGHTNING_SHADER = "Tianyu Shaders/Util/Lightning";
        public const string STARRY_SHADER = "Tianyu Shaders/Util/Starry";
        public const float BACKGROUND_DISTANCE = 1990f;
        public const int SKYBOX_RENDERQUEUE = 2480;
        public const int STARRY_RENDERQUEUE = 2485;
        public const int CLOUD_RENDERQUEUE = 2490;
        public const int PAINTINGSKY_RENDERQUEUE = 2491;

        public const string KEYWORD_DEPTH_DECAL = "_DEPTH_DECAL";

        static public class MaterialPropertyID
        {
            static public int COLOR = Shader.PropertyToID("_Color");
            static public int TINT_COLOR = Shader.PropertyToID("_TintColor");
            static public int ALPHA = Shader.PropertyToID("_Alpha");
            static public int BRIGHTNESS = Shader.PropertyToID("_Brightness");
            static public int MOON_DETAIL = Shader.PropertyToID("_MoonDetails");
            static public int AMOUNT = Shader.PropertyToID("_Amount");
            static public int PARTICLE_ALPHA = Shader.PropertyToID("_VertexAlpha");
            static public int PAINTING_SKY_ALPHA = Shader.PropertyToID("_ChangeAlpha");
            static public int MIAN_TEX = Shader.PropertyToID("_MainTex");

            static public int SCENE_AMBIENT_COLOR = Shader.PropertyToID("_AmbientColor");
            static public int SCENE_ENVIRONMENT_TEXTURE = Shader.PropertyToID("_EnvTexPBR");
            static public int SCENE_ENVIRONMENT_SEATEXTURE = Shader.PropertyToID("_SeaEnvTex");
            static public int SCENE_ENVIRONMENT_SEABACKCOLOR = Shader.PropertyToID("_SeaBackColor");
            static public int SCENE_ENVIRONMENT_CAMERASLOPE = Shader.PropertyToID("_CameraSlope");
            static public int SCENE_ENVIRONMENT_WEATHERCHANGEAMOUNT = Shader.PropertyToID("_WeatherChangeAmount");
            static public int SCENE_ENVIRONMENT_SCALE = Shader.PropertyToID("_EnvStrength");
            static public int SCENE_ENVIRONMENT_COLOR = Shader.PropertyToID("_EnvColor");
            static public int SCENE_LIGHT_GLOBAL_PARAMS = Shader.PropertyToID("_SceneLightGlobalParams");
            static public int WATER_LIGHT_DIR = Shader.PropertyToID("_WaterLightDir");
            static public int SCENE_ENVIRONMENT_AUTO_EXPOSURE = Shader.PropertyToID("_AutoExposure");
            static public int SCENE_NIGHT_SCALE = Shader.PropertyToID("_NightScale");
            static public int LOD_GI_COLOR = Shader.PropertyToID("_LOD_GI_Color");

            static public int FOLIAGE_COLOR_SCALE = Shader.PropertyToID("_FoliageColorScale");
            static public int GRASS_SEA_COLOR_SCALE = Shader.PropertyToID("_GrassSeaColor");
            static public int WIND_GRASS_FORCE = Shader.PropertyToID("_WaveForceWeather");
            static public int EMISSION_BREATH = Shader.PropertyToID("_EmissionBreath");
            static public int OCEAN_SHALLOW_COLOR = Shader.PropertyToID("_OceanShallowColor");
            static public int OCEAN_DEEP_COLOR = Shader.PropertyToID("_OceanDeepColor");
            static public int OCEAN_GLOBAL_PARAMS = Shader.PropertyToID("_OceanGlobalParams");
            static public int WATER_GLOBAL_PARAMS = Shader.PropertyToID("_WaterGlobalParams");
            static public int RIVER_COLOR_SCALE = Shader.PropertyToID("_WeatherWaterColorScale");
            static public int PARTICLE_ALPHA_SCALE = Shader.PropertyToID("_RainAlphaScale");

            static public int CHARACTER_AMBIENT_COLOR = Shader.PropertyToID("_CharacterAmbient");
            static public int CHARACTER_ENVIRONMENT_SCALE = Shader.PropertyToID("_CharacterEnvironmentScale");
            static public int CHARACTER_SKIN_SCALE = Shader.PropertyToID("_CharacterSkinColorScale");
            static public int CHARACTER_EYE_LIGHT_SCALE = Shader.PropertyToID("_CharacterEyeLightScale");
            static public int CHARACTER_SSS_LIGHT_DIR = Shader.PropertyToID("_SSSLightDir");
            static public int VOLUMETRIC_LIGHTMAP_SCALE = Shader.PropertyToID("_VlmScale");

            static public int FOG_INFO1 = Shader.PropertyToID("_FogInfo");
            static public int FOG_INFO2 = Shader.PropertyToID("_FogInfo2");
            static public int FOG_INFO3 = Shader.PropertyToID("_FogInfo3");
            static public int FOG_COLOR1 = Shader.PropertyToID("_FogColor1");
            static public int FOG_COLOR2 = Shader.PropertyToID("_FogColor2");
            static public int FOG_COLOR3 = Shader.PropertyToID("_FogColor3");
            static public int UNDER_WATER_COLOR = Shader.PropertyToID("_UnderWaterColor");
            static public int UNDER_WATER_PARAM = Shader.PropertyToID("_UnderWaterParam");

            static public int SKYBOX_COLOR_BASE = Shader.PropertyToID("_NightSkyColBase");
            static public int SKYBOX_COLOR_DELTA = Shader.PropertyToID("_NightSkyColDelta");
            static public int SKYBOX_RAYLEIGH_SCATTERING = Shader.PropertyToID("_RayleighScattering");
            static public int SKYBOX_MIE_SCATTERING = Shader.PropertyToID("_MieScattering");
            static public int SKYBOX_MIE_PHASE_FUNCTION = Shader.PropertyToID("_MiePhaseParams");
            static public int SKYBOX_SKYLINE_POS = Shader.PropertyToID("_SkyLinePos");
            static public int SKYBOX_SUN_DIR = Shader.PropertyToID("_SkyboxSunDir");
            static public int SKYBOX_MOON_DIR = Shader.PropertyToID("_SkyboxMoonDir");
            static public int SKYBOX_MOON_RAYLEIGH_SCATTERING = Shader.PropertyToID("_MoonRayleighScattering");
            static public int SKYBOX_MOON_MIE_SCATTERING = Shader.PropertyToID("_MoonMieScattering");

            static public int CLOUD_COLOR_BASE = Shader.PropertyToID("_CloudColor");
            static public int CLOUD_COLOR_SHADOW = Shader.PropertyToID("_ShadowColor");
            static public int CLOUD_PARAMS0 = Shader.PropertyToID("_CloudParams0");
            static public int CLOUD_PARAMS1 = Shader.PropertyToID("_CloudParams1");
            static public int CLOUD_MAP_TEXTURE = Shader.PropertyToID("_CloudMap");
            static public int CLOUD_SUN_COLOR = Shader.PropertyToID("_SunColor");
            static public int CLOUD_SUN_BRIGHTNESS = Shader.PropertyToID("_SunBrightness");
            static public int CLOUD_SUN_RANGE = Shader.PropertyToID("_SunRange");
            static public int CLOUD_COORD = Shader.PropertyToID("_Coord");

            static public int SKYBOX_ZENITH_COLOR = Shader.PropertyToID("_ZenithColor");
            static public int SKYBOX_HORIZON_COLOR = Shader.PropertyToID("_HorizonColor");
            static public int SKYBOX_HORIZON_FALLOFF = Shader.PropertyToID("_HorizonFalloff");
            static public int SKYBOX_MIE_SCATTER_COLOR = Shader.PropertyToID("_MieScatterColor");
            static public int SKYBOX_MIE_SCATTER_FACTOR = Shader.PropertyToID("_MieScatterFactor");
            static public int SKYBOX_MOON_MIE_SCATTER_COLOR = Shader.PropertyToID("_MoonMieScatterColor");
            static public int SKYBOX_MOON_MIE_SCATTER_FACTOR = Shader.PropertyToID("_MoonMieScatterFactor");

            static public int CLOUD_AMBIENT_COLOR = Shader.PropertyToID("_CloudAmbient");
            static public int CLOUD_LIGHT_COLOR = Shader.PropertyToID("_CloudLight");
            static public int CLOUD_ATTENUATION = Shader.PropertyToID("_Attenuation");
            static public int CLOUD_STEP_SIZE = Shader.PropertyToID("_StepSize");
            static public int CLOUD_ALPHA_SATURATION = Shader.PropertyToID("_AlphaSaturation");
            static public int CLOUD_MASK = Shader.PropertyToID("_Mask");
            static public int CLOUD_SCATTER = Shader.PropertyToID("_ScatterMultiplier");
            static public int CLOUD_ROTATE = Shader.PropertyToID("_CloudRotate");

            static public int RAIN_SNOW_PARAMS = Shader.PropertyToID("_RainSnowParams");
            static public int RAIN_RIPPLE_STRENGTH = Shader.PropertyToID("_RainRippleStrength");
            static public int RAIN_RIPPLE_SPEED = Shader.PropertyToID("_RainRippleSpeed");
            static public int RAIN_RIPPLE_TILING = Shader.PropertyToID("_RainRippleTilling");
            static public int RAIN_RIPPLE_NORMAL = Shader.PropertyToID("_RainRippleNormalTex");

            static public int CLOUD_SEA_FACING_LIGHT_COLOR = Shader.PropertyToID("_CloudSeaFacingLightColor");
            static public int CLOUD_SEA_BACKING_LIGHT_COLOR = Shader.PropertyToID("_CloudSeaBackingLightColor");

            static public int FISH_AMBIENT = Shader.PropertyToID("_FishAmbient");

            static public int SPLASH_HEIGHT_MAP = Shader.PropertyToID("_SplashHeightMap");
            static public int SPLASH_HEIGHT_OFFSET = Shader.PropertyToID("_SplashHeightOffset");
            static public int SPLASH_HEIGHT_SCALE = Shader.PropertyToID("_SplashHeightScale");

            //swimming ripple map
            static public int SWIMMING_RIPPLE_MAP = Shader.PropertyToID("_RippleMap");

            static public int FLARE_TEX = Shader.PropertyToID("_FlareTexture");

            //paintingSky
            static public int PAINTING_SKY_ALPHA_ADD = Shader.PropertyToID("_PaintingSkyAlphaAdd");

            static public int ORIGINAL_BLOOM = Shader.PropertyToID("_OriginalBloom");

            // FakeGodRay
            static public int FAKE_GOD_RAY_SUN_COLOR_AMOUNT = Shader.PropertyToID("_SunColorAmount");
            static public int FAKE_GOD_RAY_MAINTEX = Shader.PropertyToID("_Maintex");
            static public int FAKE_GOD_RAY_MASK = Shader.PropertyToID("_Mask");
            static public int FAKE_GOD_RAY_MUSPEED = Shader.PropertyToID("_MUSpeed");
            static public int FAKE_GOD_RAY_MVSPEED = Shader.PropertyToID("_MVSpeed");
            static public int FAKE_GOD_RAY_DISTORTION = Shader.PropertyToID("_Distortion");

            static public int OUTPUT_ALPHA = Shader.PropertyToID("_OutputAlpha");
        }

    }
}