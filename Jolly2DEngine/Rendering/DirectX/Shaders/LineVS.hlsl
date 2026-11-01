cbuffer LineConstants : register(b0)
{
    float ViewportWidth;
    float ViewportHeight;
    float GlowStrength;
    float GlowFalloff;
    float GlowEnabled;
    float Padding0;
    float Padding1;
    float Padding2;
};

struct VSInput
{
    float2 Position : POSITION;
    float4 Color : COLOR0;
    float2 LineData : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float Edge : TEXCOORD0;
    float CoreRatio : TEXCOORD1;
};

VSOutput main(VSInput input)
{
    VSOutput output;

    float2 ndc;

    ndc.x = (input.Position.x / ViewportWidth) * 2.0 - 1.0;
    ndc.y = 1.0 - (input.Position.y / ViewportHeight) * 2.0;

    output.Position = float4(ndc, 0.0, 1.0);
    output.Color = input.Color;
    output.Edge = input.LineData.x;
    output.CoreRatio = input.LineData.y;

    return output;
}