Shader "VirtualTexture/FeedBack"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "VirtualTextureType" = "Normal" }
        ZTest LEqual
        ZWrite On
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
                float mip = clamp((int)getMip(i.uv) + _FEEDBACKBIAS, 0, _MAXMIP);
                //新feedback方式
                int mipLength = exp2(mip);
                float2 pageResult =float2( floor(page.x / mipLength), floor(page.y/mipLength));
                pageResult =  pageResult * mipLength;
                return fixed4(pageResult / 255.0, mip / 255.0, 1);//Alpha can be vt id

            }
        
            ENDCG
        }
    }
  
}
