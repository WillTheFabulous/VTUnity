Shader "VirtualTexture/TileGeneration"
{
    Properties
    {
        _Diffuse0("Diffuse_0", 2D) = "black" {}
        _Diffuse1("Diffuse_1", 2D) = "black" {}
        _Diffuse2("Diffuse_2", 2D) = "black" {}

        _Normal0("Normal_0", 2D) = "bump" {}
        _Normal1("Normal_1", 2D) = "bump" {}
        _Normal2("Normal_2", 2D) = "bump" {}

        _AlphaMap("AlphaMap",2D) = "black" {}


    }
    SubShader
    {
        Tags { "VirtualTextureType"="Normal" }
        Cull Off ZWrite Off ZTest Always
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vertDraw
            #pragma fragment frag
            #pragma target 5.0
      
            #include "VirtualTextureCommon.cginc"
            #include "UnityCG.cginc"
        
            UNITY_DECLARE_TEX2D(_Diffuse0);
            UNITY_DECLARE_TEX2D_NOSAMPLER(_Diffuse1);

            UNITY_DECLARE_TEX2D(_AlphaMap);


            v2f_img vertDraw(appData v){
                v2f_img o;
                UNITY_INITIALIZE_OUTPUT(v2f_img, o);
                o.pos = v.vertex;
                //o.pos.w = 1.0;
                o.uv = v.texcoord;
                return o;
            };
        
            fixed4 frag(v2f_img i) : SV_Target
            {
                float4 alpha = UNITY_SAMPLE_TEX2D(_AlphaMap, i.uv);

                float4 diffuse0 = UNITY_SAMPLE_TEX2D(_Diffuse0, i.uv);
                float4 diffuse1 = UNITY_SAMPLE_TEX2D_SAMPLER(_Diffuse1, _Diffuse0, i.uv);

                fixed4 result = alpha.r * diffuse0 + alpha.g * diffuse1;
                return result;
                //return fixed4(1.0,1.0,1.0,1.0);
            }


            ENDCG
        }
    }
    
}
