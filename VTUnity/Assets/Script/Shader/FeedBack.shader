﻿Shader "VirtualTexture/FeedBack"
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
                int mip = max((int)getMip(i.uv) - 1, 0);
                return fixed4(page / 255.0, mip / 255.0, 1);//Alpha can be vt id

            }
        
            ENDCG
        }
    }
  
}
