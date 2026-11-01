cbuffer TriangleConstants : register(b0)
{
    float ViewportWidth;
    float ViewportHeight;
    float Padding0;
    float Padding1;
};

struct VSInput
{
    float2 Position : POSITION;
    float4 Color : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VSOutput main(VSInput input)
{
    VSOutput output;

    float2 ndc;
    ndc.x = (input.Position.x / ViewportWidth) * 2.0 - 1.0;
    ndc.y = 1.0 - (input.Position.y / ViewportHeight) * 2.0;

    output.Position = float4(ndc, 0.0, 1.0);
    output.Color = input.Color;

    return output;
}