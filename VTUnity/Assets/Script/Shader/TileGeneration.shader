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
        Tags{ "VirtualTextureType"="Normal" }
        Pass
        {
            Tags { "LightMode" = "ForwardBase"}
            Cull Off ZWrite Off ZTest Always
            CGPROGRAM
            #pragma vertex vertDraw
            #pragma fragment frag
            
      
            #include "VirtualTextureCommon.cginc"
            #include "UnityCG.cginc"
        
            //UNITY_DECLARE_TEX2D(_Diffuse0);
            sampler2D _Diffuse0;
            //UNITY_DECLARE_TEX2D_NOSAMPLER(_Diffuse1);
            sampler2D _Diffuse1;
            UNITY_DECLARE_TEX2D(_AlphaMap);

            float3 _TerrainSize;
            float4 _Diffuse0_ST;

            float4 _TileInfo[4];

            struct appTileData{
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 lod : TEXCOORD1;
            };

            struct v2f_tile{
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 lod : TEXCOORD1;
            };

            v2f_tile vertDraw(appTileData v){
                v2f_tile o;
                UNITY_INITIALIZE_OUTPUT(v2f_tile, o);
                o.pos = v.vertex;
                o.uv = v.texcoord;
                o.lod = v.lod;
                return o;
            };
        
            fixed4 frag(v2f_tile i) : SV_Target
            {
                   
                float4 alpha = UNITY_SAMPLE_TEX2D(_AlphaMap, i.uv);

                float4 diffuse0UV = float4( frac(i.uv.x / (_TileInfo[0].x / _TerrainSize.x)),  frac(i.uv.y / (_TileInfo[0].y/ _TerrainSize.z) ), 0, i.lod.x - 1);
                float4 diffuse1UV = float4( frac(i.uv.x / (_TileInfo[1].x / _TerrainSize.x)),  frac(i.uv.y / (_TileInfo[1].y/ _TerrainSize.z) ), 0, i.lod.x - 1);


           
                float4 diffuse0 = tex2Dlod(_Diffuse0, diffuse0UV);
                float4 diffuse1 = tex2Dlod(_Diffuse1, diffuse1UV);

                fixed4 result = alpha.r * diffuse0 + alpha.g * diffuse1;
                return result;

            }


            ENDCG
        }
        
        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            Cull Off ZWrite Off ZTest Always
            CGPROGRAM
            #pragma vertex vertDraw
            #pragma fragment frag
      
            #include "VirtualTextureCommon.cginc"
            #include "UnityCG.cginc"
        
            sampler2D _Normal0;
            sampler2D _Normal1;

            UNITY_DECLARE_TEX2D(_AlphaMap);

            float3 _TerrainSize;
            float4 _Diffuse0_ST;

            float4 _TileInfo[4];

            struct appTileData{
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 lod : TEXCOORD1;
            };

            struct v2f_tile{
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 lod : TEXCOORD1;
            };

            v2f_tile vertDraw(appTileData v){
                v2f_tile o;
                UNITY_INITIALIZE_OUTPUT(v2f_tile, o);
                o.pos = v.vertex;
                o.uv = v.texcoord;
                o.lod = v.lod;
                return o;
            };
        
            fixed4 frag(v2f_tile i) : SV_Target
            {
                   
                float4 alpha = UNITY_SAMPLE_TEX2D(_AlphaMap, i.uv);

                float4 normal0UV = float4( frac(i.uv.x / (_TileInfo[0].x / _TerrainSize.x)),  frac(i.uv.y / (_TileInfo[0].y/ _TerrainSize.z) ), 0, i.lod.x - 1);
                float4 normal1UV = float4( frac(i.uv.x / (_TileInfo[1].x / _TerrainSize.x)),  frac(i.uv.y / (_TileInfo[1].y/ _TerrainSize.z) ), 0, i.lod.x - 1);


           
                float4 normal0 = tex2Dlod(_Normal0, normal0UV);
                float4 normal1 = tex2Dlod(_Normal1, normal1UV);

                fixed4 result = alpha.r * normal0 + alpha.g * normal1;
                return result;

            }


            ENDCG
        }

        Pass
        {
            Tags {"LightMode" = "Deferred"}

            Cull Off ZWrite Off ZTest Always
            CGPROGRAM
            #pragma vertex vertDraw
            #pragma fragment frag
      
            #include "VirtualTextureCommon.cginc"
            #include "UnityCG.cginc"
        
            sampler2D _Normal0;
            sampler2D _Normal1;

            UNITY_DECLARE_TEX2D(_AlphaMap);

            float3 _TerrainSize;
            float4 _Diffuse0_ST;

            float4 _TileInfo[4];

            struct appTileData{
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 lod : TEXCOORD1;
            };

            struct v2f_tile{
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 lod : TEXCOORD1;
            };

            v2f_tile vertDraw(appTileData v){
                v2f_tile o;
                UNITY_INITIALIZE_OUTPUT(v2f_tile, o);
                o.pos = v.vertex;
                o.uv = v.texcoord;
                o.lod = v.lod;
                return o;
            };
        
            fixed4 frag(v2f_tile i) : SV_Target
            {
                   

                return fixed4(0,1,0,1);

            }
        }

       

       
        
    }
    
}
