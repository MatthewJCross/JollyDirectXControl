namespace Jolly2DEngine.Rendering.DirectX
{
    public sealed class DirectXTexture
    {
        public DirectXImage Image { get; internal set; }

        internal DirectXTexture(DirectXImage image)
        {
            ArgumentNullException.ThrowIfNull(image);
            Image = image;
        }
    }
}
