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

        

        _TerrainSize("TerrainSize", Vector) = (1.0, 1.0, 1.0, 1.0)
        

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

            float3 _TerrainSize;
            float4 _Diffuse0_ST;

            float4 _TileInfo[4];

            v2f_img vertDraw(appData v){
                v2f_img o;
                UNITY_INITIALIZE_OUTPUT(v2f_img, o);
                o.pos = v.vertex;
                o.uv = v.texcoord;
                return o;
            };
        
            fixed4 frag(v2f_img i) : SV_Target
            {
                   
                float4 alpha = UNITY_SAMPLE_TEX2D(_AlphaMap, i.uv);

                float2 diffuse0UV = float2( frac(i.uv.x / (_TileInfo[0].x / _TerrainSize.x)),  frac(i.uv.y / (_TileInfo[0].y/ _TerrainSize.z) ));
                float2 diffuse1UV = float2( frac(i.uv.x / (_TileInfo[1].x / _TerrainSize.x)),  frac(i.uv.y / (_TileInfo[1].y/ _TerrainSize.z) ));

                float4 diffuse0 = UNITY_SAMPLE_TEX2D(_Diffuse0, diffuse0UV);
                float4 diffuse1 = UNITY_SAMPLE_TEX2D_SAMPLER(_Diffuse1, _Diffuse0, diffuse1UV);

                fixed4 result = alpha.r * diffuse0 + alpha.g * diffuse1;
                return result;
                //return fixed4(1.0,1.0,1.0,1.0);
            }


            ENDCG
        }
    }
    
}
