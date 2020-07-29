#ifndef VIRTUAL_TEXTURE_INCLUDED
#define VIRTUAL_TEXTURE_INCLUDED


float _PAGETABLESIZE;
float _MAXMIP;

int _TILESIZE;
int _PADDINGSIZE;
int2 _PHYSICALTEXTURESIZE;


sampler2D _LOOKUPTEX;


sampler2D _PHYSICALTEXTURE0; //blended diffuse
sampler2D _PHYSICALTEXTURE1; //blended normal
sampler2D _PHYSICALTEXTURE2;
sampler2D _PHYSICALTEXTURE3;



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