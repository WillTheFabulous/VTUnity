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
			#pragma enable_d3d11_debug_symbols
            #pragma vertex vertUVWorld
            #pragma fragment frag
           


            fixed4 frag(v2f i) : SV_Target
            {
                
                
                float2 page = floor(i.uv * _PAGETABLESIZE);
                float2 uv = i.uv * _PAGETABLESIZE * _TILESIZE; 
                float2 dx = ddx(uv);
                float2 dy = ddy(uv);
                float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy)));
                float lambda = log2(rho);
                int mip = max(int(lambda + 0.5) , 0);
                return fixed4(page / 255.0, mip / 255.0, 1);
                
                
                
            }
        
            ENDCG
        }
    }
  
}
