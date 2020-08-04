Shader "VirtualTexture/FeedBack"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "VirtualTextureType" = "Normal" }

        Pass
        {
            CGPROGRAM
            #include "VirtualTextureCommon.cginc"	
            #pragma vertex vert
            #pragma fragment frag
            v2f vertUVWorld(appData v){
                
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
    
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = float2((worldPos.x - _TERRAINPOS.x) / _TERRAINSIZE.x + 0.5, (worldPos.z - _TERRAINPOS.z) / _TERRAINSIZE.z + 0.5 );
                return o;
            
            }


            fixed4 frag(v2f i) : SV_Target
            {
                
                
                float2 page = floor(i.uv * _PAGETABLESIZE);
                float2 uv = i.uv * _PAGETABLESIZE * _TILESIZE; 
                float2 dx = ddx(uv);
                float2 dy = ddy(uv);
                float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy)));
                float lambda = log2(rho);
                int mip = max(int(lambda + 0.5), 0);
                return fixed4(page / 255.0, mip / 255.0, 1);
                
                
                
            }
        
            ENDCG
        }
    }
  
}
