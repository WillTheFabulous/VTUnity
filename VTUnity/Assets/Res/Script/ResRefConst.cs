// Resource const used by code!
// It must be the TOPEST level module, Please do not refer to other scripts!!

namespace Pangu.Const
{
    public class ResUIConst
    {
        public const string HUD_UI = "Adapter/FreeWalk/FreeWalkHUD.prefab";
        public const string LOGIN_UI = "Adapter/FreeWalk/FreeWalkLogin.prefab";
        public const string UI_GOD_RAY_MAT = "Shader/Materials/UI/UIGodRay.mat";
        public const string UI_GOD_RAY_BEAMS_MAT = "Shader/Materials/UI/UIGodRayBeams.mat";
        public const string UI_GOD_RAY_SHADOW_MAT = "Shader/Materials/UI/UIGodRayShadow.mat";
        public const string UI_LENS_FLARE_MAT = "Shader/Materials/UI/UILensFlareAvatar.mat";
        public const string UI_LENS_FLARE_PREFAB = "Effect/prefabs/ui/ef_ui_avatar_lensflare.prefab";
        public const string UI_SCENE_UI_OBJECT_CANVAS_PREFAB = "Adapter/Prefabs/SceneUIObjectCanvas.prefab";
    }

    public class ResCharConst
    {
        public const string LL_MOUTH_ANIMATION = "character/10004/ani/10004_p_f_idle_ls_talk01.fbx";
        public const string CHAR_LOOK_AT_PATH = "Adapter/Prefabs/Lookat/Lookat_";
    }

    public class ResAnimConst
    {
        public const string ANIM_INSTANCING_ASSET = "Adapter/Prefabs/AnimIntancingData.asset";
        public const string ANIM_CONTROLLER = "item/717002/ani/717002_ani.bytes";
    }

    public class ResSceneConst
    {
        // SCENE
        public const string SCENE_ENV_PATH = "Scene/cube/textures/default_cube2d.png";
        public const string SCENE_ENVSEA_PATH = "Scene/water/models/textures/slj_noonSea_cube2d.png";
        public const string SCENE_PAINTINGSKY = "Scene/sky/prefabs/paintingSkyDefault.prefab";
        public const string SCENE_UNDERWATER_TEXTURE = "Shader/textures/underWaterNoise.tga";
        public const string SCENE_UNDERWATER_GODRAY_PATH = "Shader/Assets/UnderWaterGodRay.prefab";
        public const string SCENE_UNDERWATER_PARTICLE_GODRAT_PATH = "Shader/Assets/UnderWaterGodRayNew.prefab";
        public const string SCENE_UNDERWATER_PARTICLE_PATH = "Effect/prefabs/common/ef_UnderSeaFoam_02.prefab";
        public const string SCENE_UNDERWATER_GODRAY_TEXTURE = "Shader/Textures/underWaterGodRay01.png";
        public const string SCENE_UNDERWATER_GODRAY_MASKTEXTURE = "Shader/Textures/underWaterGodRayMask.png";
        public const string SCENE_UNDERWATER_PARTICLE_GODRAY_TEXTURE = "Shader/Textures/underWaterGodRay02.tga";
        public const string SCENE_FFTOCEAN_PATH = "Scene/water/prefabs/NewFFTOcean.prefab";
        public const string SCENE_MIRROR = "Scene/mirror/prefabs/slj_playerBirth_mirrorHouse.prefab";
        public const string SCENE_LOW_SHADOW = "Shader/Assets/shadowsPlane.prefab";
        public const string SCENE_VLIGHTMAP_TEX = "/VLightmap.png";
        public const string SCENE_VLIGHTMAP_CONFIG = "/VLightmapConfig.asset";
        public const string SCENE_POWER_SAVE = "scene/save_power/savepower.png";
        public const string SCENE_CAUSTICS_BASE = "Scene/terrain/models/textures/caustic/caustics_{0:D3}.bmp";
        public const string SCENE_FLOWER_SEA_NIGHT_TRAIL_DATA = "Shader/Assets/NightTrailData.asset";

        // WEATHER
        public const string WEATHER_RAIN_PARTICLE_PATH = "Scene/common/weather/rain_particles/rain_drop.prefab";
        public const string WEATHER_RAIN_RIPPLE_TEX_FORMAT = "Scene/common/weather/rain_ripples/rain_ripple_{0}.png";
        public const string WEATHER_RAIN_SPLASH_PARTICLE_PATH = "Scene/common/weather/rain_splash.prefab";
        public const string WEATHER_RAIN_SPLASH_DATA_FORMAT = "Scene/common/weather/rain_splash/{0}.asset";
        public const string WEATHER_RAIN_SCREEN_PARTICLE_PATH = "Scene/common/weather/rain_screen.prefab";
        public const string WEATHER_SNOW_SCREEN_PARTICLE_PATH = "Scene/common/weather/snow_screen.prefab";
        public const string WEATHER_SNOW_PARAMS_TEX_PATH = "Scene/hdtex/textures/snow_params_tex.tga";
        public const string WEATHER_STARRY_PREFAB_PATH = "Scene/common/weather/starry.prefab";
        public const string WEATHER_GALAXY_PREFAB_PATH = "Scene/common/weather/galaxy.prefab";
        public const string WEATHER_MOON_PREFAB_PATH = "Scene/common/weather/moon.prefab";
        public const string WEATHER_LIGHTNING_CLOUD_PATH = "Scene/common/weather/lightning_cloud.prefab";
        public const string WEATHER_SHADER_WARMUP_ASSET_PATH = "Scene/common/weather/shader_warm.asset";
        public const string WEATHER_TRANSITION_SETTINGS_PATH = "Scene/common/weather/transition_settings.asset";
        public const string WEATHER_DEFAULT_LIGHTNING_MODEL = "Scene/common/weather/default_lightning.asset";
        public const string WEATHER_PRESET_LIBRARY_PATH = "Scene/weather/preset_library.asset";
        public const string WEATHER_PROFILE_PATH_FORMAT = "Scene/weather/{0}.asset";
        public const string WEATHER_DEFAULT_TEMPLATE_PATH = "Scene/weather/weather_template.asset";
        public const string WEATHER_DEFAULT_FLARE_ASSET = "Scene/common/weather/cheap_plastic_lens.asset";
    }

    public class ResEffectConst
    {
        public const string EFFECT_SCREEN_CRUSH = "Effect/prefabs/camera/ef_screen_crack001.prefab";
        public const string EFFECT_NIGHT_TRAIL = "Scene/common/weather/ef_flower_trail.prefab";
        public const string EFFECT_CLOUD_PARTICLE_PREFAB = "Effect/prefabs/scene/gas/ef_cloud_01_zl.prefab";
        public const string EFFECT_EDITOR_CLOUD_PARTICLE_PREFAB = "Assets/Res/Effect/prefabs/scene/gas/ef_cloud_01_zl.prefab";
    }

    public class ResAudioConst
    {
        public const string EVENT_WEATHER_THUNDER = "event:/Ambient/Weather/3D/wheather_thunder_lightning_01";
        public const string AUDIO_MAIN_BUS = "bus:/";
        public const string AUDIO_MUSIC_BUS = "bus:/Music";
        public const string AUDIO_SOUND_BUS = "bus:/Sound";
        public const string AUDIO_CUTSCENE_MUSIC_BUS = "bus:/StoryCmtMusic";
        public const string AUDIO_CUTSCENE_SOUND_BUS = "bus:/StoryCmtSound";
        public const string AUDIO_MUSICIAN_BUS = "bus:/PlayMusic";
    }

    public class ResShaderConst
    {
        public const string DISSOLVE_TEX_PATH = "Shader/Textures/Dissolve/dissolvePatternUniform.png";
        public const string BURN_LOOKUP_TEX_PATH = "Shader/Textures/BurnLookup.png";
        public const string BLINDNESS_MASK_TEX = "Shader/Textures/blindness.png";
        public const string MINIMAP_IMAGE_MATERIAL = "Shader/Materials/UI/UIMiniMapImage.mat";
        public const string MINIMAP_RAW_IMAGE_MATERIAL = "Shader/Materials/UI/UIMiniMapSoftEdge.mat";
        public const string TRANSPARENT_DEPTH_MATERIAL = "Shader/Materials/TransparentDepth.mat";
        public const string SSS_TEX_PATH = "Shader/Textures/sss.tga";
    }

    public class FishRodConst
    {
        public const string PHYSIC_ROD_DATA_PATH = "Adapter/Prefabs/PhysicRodData.asset";
        public const string PHYSIC_ROPE_MAT_PATH = "Shader/Materials/FishRopeMat.mat";
    }

    public class PlayerSceneConst
    {
        public const string MAIN_LIGHT_DATA_PATH = "Scene/player/prefabs/MainLightRoot.prefab";
        public const string SCENE_EFFECT_DATA_PATH = "Scene/player/prefabs/PlayerEnvir.prefab";
        public const string CAMERA_SETTING_DATA_PATH = "Scene/player/prefabs/CameraSetting.prefab";
    }
}
