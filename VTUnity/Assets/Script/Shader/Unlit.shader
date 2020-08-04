// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VirtualTexture/Unlit"
{

    SubShader
    {
        Tags { "VirtualTextureType"="Normal" }
		LOD 100
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "VirtualTextureCommon.cginc"
			#pragma enable_d3d11_debug_symbols
			#pragma vertex vert
			#pragma fragment frag
			
			
			fixed4 frag (v2f i) : SV_Target{

				//COMPUTE PAGE TABLE POSITION
				
				float pageSizeInUV = 1.0 / _PAGETABLESIZE;

				float2 pageTableUV = int2(i.uv / pageSizeInUV) * pageSizeInUV;

				//SAMPLE PAGE TABLE

				float4 page = tex2D(_LOOKUPTEX, pageTableUV) * 255.0;
				
				//COMPUTE PHYSICAL TEXTURE UV FROM TILE X Y
				int2 physicalSize = (_TILESIZE + _PADDINGSIZE) * _PHYSICALTEXTURESIZE;
				////////////////////////
				float2 physicalBaseUV = float2(page.r / float(_PHYSICALTEXTURESIZE.x), page.g / float(_PHYSICALTEXTURESIZE.y));

				float pageB = page.b;

				//当前mip在uv space中的单块page大小
				float mipRectLength = 1.0 / exp2(_MAXMIP - pageB);
				//当前渲染的fragment在他所处的page中的比例
				float2 pageRatio = frac(i.uv / mipRectLength);
				//结合physicalBaseUV(已经算上padding)，算出我们在physical texture上应该采的点
				float2 finalSampleUV = physicalBaseUV + float2(_TILESIZE * pageRatio.x / float(physicalSize.x), _TILESIZE * pageRatio.y / float(physicalSize.y));
				//SAMPLE PHYSICAL TEXTURE USING 
				fixed4 col = tex2D(_PHYSICALTEXTURE0, finalSampleUV);
				/**
				fixed4 col = fixed4(1, 1, 0, 0);
				**/
				return col;
			}
			ENDCG
		}
    }
  
}
