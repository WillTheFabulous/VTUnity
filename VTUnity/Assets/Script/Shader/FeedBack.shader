Shader "VirtualTexture/FeedBack"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "VirtualTextureType"="Normal" }

        Pass
        {
            CGPROGRAM
            #include "VirtualTextureCommon.cginc"	
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv * 64 * 256; 
                float2 dx = ddx(uv);
                float2 dy = ddy(uv);
                float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy));
                float lambda = log2(rho);
                
            }
        
            ENDCG
        }
    }
  
}
