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
            //#include "VirtualTextureCommon.cginc"	
            #pragma vertex vert
            #pragma fragment frag


            struct appData
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            v2f vert(appData v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
    
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;

            }

            fixed4 frag(v2f i) : SV_Target
            {
                /**
                float2 page = floor(i.uv * _PAGETABLESIZE);
                float2 uv = i.uv * _PAGETABLESIZE * _TILESIZE; 
                float2 dx = ddx(uv);
                float2 dy = ddy(uv);
                float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy)));
                float lambda = log2(rho);
                int mip = max(int(lambda + 0.5), 0);
                **/
                return fixed4(0.0 / 255.0, 255.0 / 255.0, 0, 0.5);
                
                
                
            }
        
            ENDCG
        }
    }
  
}
