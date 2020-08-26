using UnityEngine;

namespace Pangu.Weather
{
    using MaterialPropertyID = WeatherConst.MaterialPropertyID;

    [ExecuteInEditMode]
    public class WeatherDefault : MonoBehaviour
    {
        void Start()
        {
            WeatherStartup.SetDefault();
        }
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public class WeatherStartup
    {
        static WeatherStartup()
        {
            SetDefault();
        }

        static public void SetDefault()
        {
            Shader.SetGlobalVector(MaterialPropertyID.SCENE_LIGHT_GLOBAL_PARAMS, Vector3.one);
            Shader.SetGlobalColor(MaterialPropertyID.FOLIAGE_COLOR_SCALE, Color.white);
            Shader.SetGlobalFloat(MaterialPropertyID.SCENE_ENVIRONMENT_SCALE, 1f);
            Shader.SetGlobalColor(MaterialPropertyID.SCENE_ENVIRONMENT_COLOR, Color.white * 0.7f);
            Shader.SetGlobalColor(MaterialPropertyID.CHARACTER_SKIN_SCALE, Color.white);
            Shader.SetGlobalFloat(MaterialPropertyID.CHARACTER_EYE_LIGHT_SCALE, 1f);
            Shader.SetGlobalColor(MaterialPropertyID.CHARACTER_ENVIRONMENT_SCALE, Color.white);
            Shader.SetGlobalVector(MaterialPropertyID.WATER_GLOBAL_PARAMS, Vector4.one);
            Shader.SetGlobalFloat(MaterialPropertyID.PARTICLE_ALPHA_SCALE, 1f);
            Shader.SetGlobalFloat(MaterialPropertyID.SCENE_ENVIRONMENT_AUTO_EXPOSURE, 1f);
            Shader.SetGlobalVector(MaterialPropertyID.FOG_INFO1, Vector4.zero);
            Shader.SetGlobalVector(MaterialPropertyID.FOG_INFO3, Vector4.zero);
            Shader.SetGlobalTexture(MaterialPropertyID.SWIMMING_RIPPLE_MAP, Texture2D.blackTexture); //swimming ripple map init
            Shader.SetGlobalFloat(MaterialPropertyID.ORIGINAL_BLOOM, 1f);
            Shader.SetGlobalFloat(MaterialPropertyID.OUTPUT_ALPHA, 0f);
            Shader.EnableKeyword(WeatherConst.KEYWORD_DEPTH_DECAL);
            DynamicBoneSetting.globalWindEnable = false;

            
        }
    }
}
