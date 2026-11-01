namespace Jolly2DEngine.Rendering.DirectX
{
    internal sealed class DirectXSpriteState
    {
        public DirectXTexture Texture { get; }
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public float Rotation;
        public float Opacity;
        public bool Visible;
        public int Layer;

        public DirectXSpriteState(DirectXTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            Texture = texture;
            Width = texture.Image.Width;
            Height = texture.Image.Height;
            Opacity = 1.0f;
            Visible = true;
        }
    }
}
