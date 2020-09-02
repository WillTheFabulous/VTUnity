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
			#pragma vertex vertUVWorld
			#pragma fragment frag
			
			
			fixed4 frag (v2f i) : SV_Target{

				//COMPUTE PAGE TABLE POSITION
				
				float pageSizeInUV = 1.0 / float(_PAGETABLESIZE);

				float2 pageTableUV = float2(floor(i.uv.x * float(_PAGETABLESIZE)), floor(i.uv.y * float(_PAGETABLESIZE))) / _PAGETABLESIZE;//int2(i.uv / pageSizeInUV) * pageSizeInUV;

				//SAMPLE PAGE TABLE

				float4 page = tex2D(_LOOKUPTEX, pageTableUV) * 255.0;

				if (_DEBUG == 2) 
				{
					return fixed4(page.b * 0.1, 0.0, 0.0, 1);
				}
				else
				{
					//COMPUTE PHYSICAL TEXTURE UV FROM TILE X Y
					float tileSizeWithPadding = _TILESIZE + 2 * _PADDINGSIZE;
					float2 physicalSize = tileSizeWithPadding * _PHYSICALTEXTURESIZE;

					float2 physicalBaseUV = float2( (page.r * tileSizeWithPadding + _PADDINGSIZE )/ physicalSize.x , (page.g * tileSizeWithPadding + _PADDINGSIZE) / physicalSize.y);

					//当前mip在uv space中的单块page大小
					float mipRectLength = 1.0 / exp2(_MAXMIP - page.b);
					//当前渲染的fragment在他所处的page中的比例
					float2 pageRatio = frac(i.uv / mipRectLength);//float2(mod(i.uv.x, mipRectLength)/ mipRectLength, mod(i.uv.y , mipRectLength) / mipRectLength);//mod(i.uv , mipRectLength) / mipRectLength;
					//结合physicalBaseUV(已经算上padding)，算出我们在physical texture上应该采的点
					float2 finalSampleUV = physicalBaseUV + float2(_TILESIZE * pageRatio.x / float(physicalSize.x), _TILESIZE * pageRatio.y / float(physicalSize.y));
					//SAMPLE PHYSICAL TEXTURE USING 
					fixed4 col;

					
					float2 finalSampleUVTexel = float2(finalSampleUV.x * physicalSize.x, finalSampleUV.y * physicalSize.y); 
					//float2 dx = ddx(finalSampleUVTexel);
					//float2 dy = ddy(finalSampleUVTexel);
					//float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy)));
					//float lambdaHard = log2(rho);
					//float hardwareMip = max(lambdaHard,0);//max(lambdaHard , 0.0);

					//float softwareMip = getMip(i.uv);
					//float targetMip = clamp(softwareMip, page.b, page.b + 3.0) - page.b;
					
					//float mipBias = targetMip - hardwareMip ;


					//float floorMip = -floor(hardwareMip);
					//float4 finalSampleUVBias = float4(finalSampleUV, 0, mipBias);
					
					//float rho = min(dotdx, dotdy);
					//float lambda = log2(sqrt(rho));
					//float requiredMip = max(lambda , 0);
					float vtSize = _PAGETABLESIZE * _TILESIZE;
					float2 dx = (vtSize / physicalSize.x) * ddx(i.uv);
					float2 dy = (vtSize / physicalSize.y) * ddy(i.uv);
					float dotdx = dot(dx, dx);
					float dotdy = dot(dy, dy);
					


					float vtMip = getMipStandard(i.uv);
					
					float physicalMip = clamp(vtMip, page.b, page.b + 5.0) - page.b;
					
					float multiplier = exp2(physicalMip - vtMip);

					float2 finaldx;
					float2 finaldy;

					finaldx = multiplier * dx;
					finaldy = multiplier * dy;



					if (_DEBUG == 0) {
						col = tex2D(_PHYSICALTEXTURE0, finalSampleUV, finaldx, finaldy);
						//col = tex2Dbias(_PHYSICALTEXTURE0, finalSampleUVBias);
						//col = tex2D(_PHYSICALTEXTURE0,finalSampleUV );
					}
					else {
						if (pageRatio.x < 0.05 || pageRatio.y < 0.05 || pageRatio.x > 0.95 || pageRatio.y > 0.95) {
							col = fixed4(page.b * 0.1, 0.0, 0.0, 1);
						}
						else {
							col = tex2D(_PHYSICALTEXTURE0, finalSampleUV, finaldx, finaldy);
							//col = tex2Dbias(_PHYSICALTEXTURE0, finalSampleUVBias);
						}
					}

					return col;
				}
				
			}

			
			ENDCG
		}
    }
  
}
