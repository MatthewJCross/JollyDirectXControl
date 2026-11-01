namespace Jolly2DEngine.Graphics
{
    public sealed class JollyTexture
    {
        public int Width { get; }
        public int Height { get; }

        internal object RendererTexture { get; }

        internal byte[] AlphaMask { get; }

        internal JollyTexture(object rendererTexture, int width, int height, byte[] alphaMask)
        {
            ArgumentNullException.ThrowIfNull(rendererTexture);
            ArgumentNullException.ThrowIfNull(alphaMask);

            if (alphaMask.Length != checked(width * height))
            {
                throw new ArgumentException("The alpha mask size does not match the texture dimensions.", nameof(alphaMask));
            }

            Width = width;
            Height = height;
            RendererTexture = rendererTexture;
            AlphaMask = alphaMask;
        }

        public byte GetAlpha(int x, int y)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            {
                return 0;
            }

            return AlphaMask[y * Width + x];
        }
    }
}
