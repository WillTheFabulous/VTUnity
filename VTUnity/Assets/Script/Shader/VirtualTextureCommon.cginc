#ifndef VIRTUAL_TEXTURE_INCLUDED
#define VIRTUAL_TEXTURE_INCLUDED


float _PAGETABLESIZE;
float _TILESIZE;


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
#endif