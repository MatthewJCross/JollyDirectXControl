Texture2D SpriteTexture : register(t0);
SamplerState SpriteSampler : register(s0);

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float Opacity : TEXCOORD1;
};

float4 main(PSInput input) : SV_TARGET
{
    float4 color = SpriteTexture.Sample(SpriteSampler, input.TexCoord);
    color.a *= input.Opacity;
    return color;
}