namespace Jolly2DEngine.Rendering.DirectX
{
    public sealed class DirectXSprite
    {
        public DirectXTexture Texture { get; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Rotation { get; set; }
        public float Opacity { get; set; } = 1.0f;
        public bool Visible { get; set; } = true;

        internal DirectXSprite(DirectXTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);
            Texture = texture;
            Width = texture.Image.Width;
            Height = texture.Image.Height;
        }
    }
}
