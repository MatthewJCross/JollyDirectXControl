using Vortice.Direct3D11;

namespace Jolly2DEngine.Graphics
{
    public sealed class JollyRenderTarget : IDisposable
    {
        internal ID3D11Texture2D Texture { get; }
        internal ID3D11RenderTargetView RenderTargetView { get; }
        internal ID3D11ShaderResourceView ShaderResourceView { get; }

        public int Width { get; }
        public int Height { get; }

        internal JollyRenderTarget(ID3D11Texture2D texture, ID3D11RenderTargetView renderTargetView, ID3D11ShaderResourceView shaderResourceView, int width, int height)
        {
            Texture = texture;
            RenderTargetView = renderTargetView;
            ShaderResourceView = shaderResourceView;
            Width = width;
            Height = height;
        }

        public void Dispose()
        {
            ShaderResourceView.Dispose();
            RenderTargetView.Dispose();
            Texture.Dispose();
        }
    }
}
