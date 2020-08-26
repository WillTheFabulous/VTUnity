#ifndef TERRAIN_COMMON
#define TERRAIN_COMMON

inline fixed triblend(fixed valueR, fixed valueG, fixed valueB, fixed3 factor)
{
    return valueR * factor.r + valueG * factor.g + valueB * factor.b;
}

inline fixed3 triblend(fixed3 valueR, fixed3 valueG, fixed3 valueB, fixed3 factor)
{
    return valueR * factor.r + valueG * factor.g + valueB * factor.b;
}

inline fixed4 triblend(fixed4 valueR, fixed4 valueG, fixed4 valueB, fixed3 factor)
{
    return valueR * factor.r + valueG * factor.g + valueB * factor.b;
}

inline fixed fourblend(fixed valueR, fixed valueG, fixed valueB, fixed valueA, fixed4 factor)
{
    return valueR * factor.r + valueG * factor.g + valueB * factor.b + valueA * factor.a;
}

inline fixed2 fourblend(fixed2 valueR, fixed2 valueG, fixed2 valueB, fixed2 valueA, fixed4 factor)
{
    return valueR * factor.r + valueG * factor.g + valueB * factor.b + valueA * factor.a;
}

inline fixed3 fourblend(fixed3 valueR, fixed3 valueG, fixed3 valueB, fixed3 valueA, fixed4 factor)
{
    return valueR * factor.r + valueG * factor.g + valueB * factor.b + valueA * factor.a;
}

inline fixed4 fourblend(fixed4 valueR, fixed4 valueG, fixed4 valueB, fixed4 valueA, fixed4 factor)
{
    return valueR * factor.r + valueG * factor.g + valueB * factor.b + valueA * factor.a;
}

//fixed _HeightWeight;

inline half heightAdjust(half heightR, half heightG, half factor)
{
    half2 factor2 = half2(1.h - factor, factor);
    half2 blend = half2(heightR, heightG) * factor2;
    half ma = max(blend.r, blend.g);
    blend = max(blend - ma + _HeightWeight, 0.h) * factor2;
    return blend.g / (blend.r + blend.g);
}

inline half3 heightAdjust(half heightR, half heightG, half heightB, half3 factor)
{
    half3 blend = half3(heightR, heightG, heightB) * factor;
    half ma = max(blend.r, max(blend.g, blend.b));
    blend = max(blend - ma + _HeightWeight , 0.h) * factor;
    return blend / (blend.r + blend.g + blend.b);
}

inline half4 heightAdjust(half heightR, half heightG, half heightB, half heightA, half4 factor)
{
    half4 blend = half4(heightR, heightG, heightB, heightA) * factor;
    half ma = max(blend.a, max(blend.r, max(blend.g, blend.b)));
    blend = max(blend - ma + _HeightWeight , 0.h) * factor;
    return blend / (blend.r + blend.g + blend.b + blend.a);
}

#endif //TERRAIN_COMMON