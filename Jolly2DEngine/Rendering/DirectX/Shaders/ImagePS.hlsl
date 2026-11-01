Texture2D ImageTexture : register(t0);
SamplerState ImageSampler : register(s0);

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

float4 main(PSInput input) : SV_TARGET
{
    return ImageTexture.Sample(ImageSampler, input.TexCoord);
}