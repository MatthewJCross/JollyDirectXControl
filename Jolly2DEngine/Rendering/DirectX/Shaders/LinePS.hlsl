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

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float Edge : TEXCOORD0;
    float CoreRatio : TEXCOORD1;
};

float4 main(PSInput input) : SV_TARGET
{
    float distanceFromCenter = abs(input.Edge);
    float coreRatio = saturate(input.CoreRatio);

    float core = 1.0 - smoothstep(coreRatio, min(coreRatio + 0.03, 1.0), distanceFromCenter);

    float glow = 0.0;

    if (GlowEnabled > 0.5)
    {
        float glowPosition = saturate((distanceFromCenter - coreRatio) / max(1.0 - coreRatio, 0.001));
        glow = pow(1.0 - glowPosition, max(GlowFalloff, 0.01)) * GlowStrength;
    }

    float alpha = max(core, glow);

    return float4(input.Color.rgb, input.Color.a * saturate(alpha));
}