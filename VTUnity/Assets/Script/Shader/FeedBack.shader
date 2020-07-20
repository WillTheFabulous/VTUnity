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

            fixed4 frag(v2f i) :SV_Target
            {
                
            }
        
            ENDCG
        }
    }
  
}
