using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;

namespace Jolly2DEngine.Graphics
{
    /// <summary>
    /// Builds a single texture by compositing other textures and text into a BGRA canvas.
    /// </summary>
    public sealed class JollyTextureComposer
    {
        private readonly DirectXJollyRenderer _renderer;
        private readonly byte[] _pixels;

        public int Width { get; }
        public int Height { get; }

        public JollyTextureComposer(DirectXJollyRenderer renderer, int width, int height, JollyColor? background = null)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);

            _renderer = renderer;
            Width = width;
            Height = height;
            _pixels = new byte[checked(width * height * 4)];

            if (background.HasValue)
            {
                Fill(background.Value);
            }
        }

        /// <summary>Fills the entire canvas with a colour.</summary>
        public void Fill(JollyColor color)
        {
            byte red = ToByte(color.R);
            byte green = ToByte(color.G);
            byte blue = ToByte(color.B);
            byte alpha = ToByte(color.A);

            for (int i = 0; i < _pixels.Length; i += 4)
            {
                _pixels[i] = blue;
                _pixels[i + 1] = green;
                _pixels[i + 2] = red;
                _pixels[i + 3] = alpha;
            }
        }

        /// <summary>Draws an existing texture onto the canvas at top-left pixel coordinates.</summary>
        public void DrawTexture(JollyTexture texture, int x, int y, float opacity = 1.0f)
        {
            ArgumentNullException.ThrowIfNull(texture);
            DrawTexture(texture, x, y, texture.Width, texture.Height, opacity);
        }

        /// <summary>Draws a texture scaled to the requested size, alpha-blending it over the canvas.</summary>
        public void DrawTexture(JollyTexture texture, int x, int y, int width, int height, float opacity = 1.0f)
        {
            ArgumentNullException.ThrowIfNull(texture);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);
            ValidateOpacity(opacity);

            if (texture.RendererTexture is not DirectXTexture directXTexture)
            {
                throw new InvalidOperationException("JollyTexture was not created by the DirectX renderer.");
            }

            ReadOnlySpan<byte> source = directXTexture.Image.Pixels.Span;

            for (int destinationY = System.Math.Max(0, y); destinationY < System.Math.Min(Height, y + height); destinationY++)
            {
                int sourceY = (int)((long)(destinationY - y) * texture.Height / height);

                for (int destinationX = System.Math.Max(0, x); destinationX < System.Math.Min(Width, x + width); destinationX++)
                {
                    int sourceX = (int)((long)(destinationX - x) * texture.Width / width);
                    int sourceIndex = (sourceY * texture.Width + sourceX) * 4;
                    int destinationIndex = (destinationY * Width + destinationX) * 4;

                    float sourceAlpha = source[sourceIndex + 3] / 255.0f * opacity;
                    if (sourceAlpha <= 0.0f)
                    {
                        continue;
                    }

                    float destinationAlpha = _pixels[destinationIndex + 3] / 255.0f;
                    float outputAlpha = sourceAlpha + destinationAlpha * (1.0f - sourceAlpha);
                    if (outputAlpha <= 0.0f)
                    {
                        _pixels[destinationIndex] = 0;
                        _pixels[destinationIndex + 1] = 0;
                        _pixels[destinationIndex + 2] = 0;
                        _pixels[destinationIndex + 3] = 0;
                        continue;
                    }

                    float destinationWeight = destinationAlpha * (1.0f - sourceAlpha);
                    _pixels[destinationIndex] = Blend(source[sourceIndex], _pixels[destinationIndex], sourceAlpha, destinationWeight, outputAlpha);
                    _pixels[destinationIndex + 1] = Blend(source[sourceIndex + 1], _pixels[destinationIndex + 1], sourceAlpha, destinationWeight, outputAlpha);
                    _pixels[destinationIndex + 2] = Blend(source[sourceIndex + 2], _pixels[destinationIndex + 2], sourceAlpha, destinationWeight, outputAlpha);
                    _pixels[destinationIndex + 3] = ToByte(outputAlpha);
                }
            }
        }

        /// <summary>Creates a text texture and draws it onto the canvas.</summary>
        public void DrawText(string text, string fontFamily, float fontSize, JollyColor color, int x, int y, bool bold = false, bool italic = false, bool ultraBold = false, float opacity = 1.0f)
        {
            JollyTexture textTexture = _renderer.CreateTextTexture(text, fontFamily, fontSize, color, bold, italic, ultraBold);
            DrawTexture(textTexture, x, y, opacity);
        }

        /// <summary>Creates a new engine texture containing the current canvas.</summary>
        public JollyTexture CreateTexture()
        {
            return _renderer.CreateTextureFromBgraPixels(Width, Height, _pixels);
        }

        /// <summary>Replaces the pixels in an existing same-sized texture.</summary>
        public void UpdateTexture(JollyTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (texture.Width != Width || texture.Height != Height)
            {
                throw new ArgumentException("The destination texture must match the canvas dimensions.", nameof(texture));
            }

            _renderer.UpdateTextureFromBgraPixels(texture, _pixels);
        }

        private static byte Blend(byte foreground, byte background, float sourceAlpha, float destinationWeight, float outputAlpha)
        {
            float output = (foreground * sourceAlpha + background * destinationWeight) / outputAlpha;
            return (byte)System.Math.Clamp((int)System.Math.Round(output), 0, 255);
        }

        private static byte ToByte(float value)
        {
            return (byte)System.Math.Clamp((int)(value * 255.0f), 0, 255);
        }

        private static void ValidateOpacity(float opacity)
        {
            if (opacity < 0.0f || opacity > 1.0f || float.IsNaN(opacity))
            {
                throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0 and 1.");
            }
        }
    }
}
