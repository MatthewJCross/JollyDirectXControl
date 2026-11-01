cbuffer SpriteConstants : register(b0)
{
    float PositionX;
    float PositionY;
    float Width;
    float Height;
    float Rotation;
    float Opacity;
    float ViewportWidth;
    float ViewportHeight;
    float FlipHorizontal;
    float FlipVertical;
    float SourceX;
    float SourceY;
    float SourceWidth;
    float SourceHeight;
    float Padding0;
    float Padding1;
};

struct VSInput
{
    float3 Position : POSITION;
    float2 TexCoord : TEXCOORD0;

    float4 InstancePositionSize : TEXCOORD1;
    float4 InstanceRotationOpacity : TEXCOORD2;
    float4 InstanceSource : TEXCOORD3;
    float4 InstancePadding : TEXCOORD4;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float Opacity : TEXCOORD1;
};

VSOutput main(VSInput input)
{
    VSOutput output;

    float2 position = input.InstancePositionSize.xy;
    float2 size = input.InstancePositionSize.zw;

    float rotation = input.InstanceRotationOpacity.x;
    float opacity = saturate(input.InstanceRotationOpacity.y);

    float flipHorizontal = input.InstanceRotationOpacity.z;
    float flipVertical = input.InstanceRotationOpacity.w;

    float sourceX = input.InstanceSource.x;
    float sourceY = input.InstanceSource.y;
    float sourceWidth = input.InstanceSource.z;
    float sourceHeight = input.InstanceSource.w;

    float2 localPosition = input.Position.xy * size;

    float cosRotation = cos(rotation);
    float sinRotation = sin(rotation);

    float2 rotatedPosition;

    rotatedPosition.x = localPosition.x * cosRotation - localPosition.y * sinRotation;
    rotatedPosition.y = localPosition.x * sinRotation + localPosition.y * cosRotation;

    float2 screenPosition = position + rotatedPosition;

    float2 ndc;

    ndc.x = (screenPosition.x / ViewportWidth) * 2.0f - 1.0f;
    ndc.y = 1.0f - (screenPosition.y / ViewportHeight) * 2.0f;

    output.Position = float4(ndc, 0.0f, 1.0f);

    float2 texCoord = input.TexCoord;

    if (flipHorizontal > 0.5f)
    {
        texCoord.x = 1.0f - texCoord.x;
    }

    if (flipVertical > 0.5f)
    {
        texCoord.y = 1.0f - texCoord.y;
    }

    output.TexCoord.x = sourceX + texCoord.x * sourceWidth;
    output.TexCoord.y = sourceY + texCoord.y * sourceHeight;

    output.Opacity = opacity;

    return output;
}