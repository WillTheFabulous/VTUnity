#ifndef VIRTUAL_TEXTURE_INCLUDED
#define VIRTUAL_TEXTURE_INCLUDED


float _PAGETABLESIZE;
float _MAXMIP;


int _TILESIZE;
int _PADDINGSIZE;
int2 _PHYSICALTEXTURESIZE;

float3 _TERRAINPOS;
int3 _TERRAINSIZE;

int _PHYSICALMAXMIP;
int _DEBUG;


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

v2f vertUVWorld(appData v) {

	v2f o;
	UNITY_INITIALIZE_OUTPUT(v2f, o);

	o.pos = UnityObjectToClipPos(v.vertex);

	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.uv = float2((worldPos.x - _TERRAINPOS.x) / _TERRAINSIZE.x, (worldPos.z - _TERRAINPOS.z) / _TERRAINSIZE.z);
	return o;

}

v2f vert(appData v)
{
    v2f o;
    UNITY_INITIALIZE_OUTPUT(v2f, o);
    
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    return o;

}

float getMip(float2 uv){

    float2 texelPos = uv * _PAGETABLESIZE * _TILESIZE; 
    float2 dx = ddx(texelPos);
    float2 dy = ddy(texelPos);
   
    float rho = max(sqrt(dot(dx, dx)), sqrt(dot(dy, dy)));
    float lambda = log2(rho);
    float mip = max(lambda + 0.5 , 0);


    //float px = dot(dx,dx);
    //float py = dot(dy,dy);
    //float maxlod = 0.5 * log2( max(px, py) );
    //float minlod = 0.5 * log2( min(px, py) );
    //float mip = max(minlod, 0.0);
    return mip;

}

float mod(float  a, float  b)   
{  
	return a - b*floor(a / b);   
}
#endif