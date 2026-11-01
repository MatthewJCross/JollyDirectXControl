using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;

namespace Jolly2DEngine.Graphics
{
    /// <summary>Controls the colour palette used by the TV static generator.</summary>
    public enum TvStaticColourMode
    {
        Monochrome,
        FullColour,
        BlueTint,
        GreenTint,
        RedTint
    }

    /// <summary>
    /// Generates animated television static and exposes it as a reusable sprite.
    /// Optional texture and text overlays are composited into each generated frame.
    /// </summary>
    public sealed class TvStaticGenerator
    {
        private sealed record Overlay(JollyTexture Texture, int X, int Y, float Opacity);

        private readonly DirectXJollyRenderer _renderer;
        private readonly Random _random;
        private readonly List<Overlay> _overlays = [];
        private readonly byte[] _pixels;
        private readonly float _frameInterval;
        private float _elapsed;

        public int Width { get; }
        public int Height { get; }
        public int FramesPerSecond { get; }
        public TvStaticColourMode ColourMode { get; set; }
        public JollyTexture Texture { get; }
        public JollySprite Sprite { get; }

        /// <param name="renderer">Renderer that owns the generated texture.</param>
        /// <param name="width">Generated bitmap width in pixels.</param>
        /// <param name="height">Generated bitmap height in pixels.</param>
        /// <param name="framesPerSecond">Static animation rate, from 1 to 120 FPS.</param>
        /// <param name="randomSeed">Optional seed for repeatable static.</param>
        public TvStaticGenerator(DirectXJollyRenderer renderer, int width, int height, int framesPerSecond = 25, int? randomSeed = null, TvStaticColourMode colourMode = TvStaticColourMode.Monochrome)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(framesPerSecond, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(framesPerSecond, 120);

            _renderer = renderer;
            _random = randomSeed.HasValue ? new Random(randomSeed.Value) : Random.Shared;
            _frameInterval = 1.0f / framesPerSecond;
            Width = width;
            Height = height;
            FramesPerSecond = framesPerSecond;
            ColourMode = colourMode;
            _pixels = new byte[checked(width * height * 4)];

            GenerateFrame();
            CompositeOverlays();
            Texture = renderer.CreateTextureFromBgraPixels(width, height, _pixels);
            Sprite = renderer.CreateSprite(Texture);
        }

        /// <summary>
        /// Advances the animation. Returns true only when a new frame was generated.
        /// Call this from the game update loop using elapsed seconds.
        /// </summary>
        public bool Update(float deltaSeconds)
        {
            if (deltaSeconds < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            _elapsed += deltaSeconds;

            if (_elapsed < _frameInterval)
            {
                return false;
            }

            _elapsed %= _frameInterval;
            GenerateFrame();
            CompositeOverlays();
            _renderer.UpdateTextureFromBgraPixels(Texture, _pixels);
            return true;
        }

        /// <summary>Adds an existing texture over the static. Coordinates are top-left pixel coordinates.</summary>
        public void AddOverlay(JollyTexture texture, int x, int y, float opacity = 1.0f)
        {
            ArgumentNullException.ThrowIfNull(texture);
            ValidateOpacity(opacity);
            _overlays.Add(new Overlay(texture, x, y, opacity));
        }

        /// <summary>Creates a text texture and adds it as an overlay over the static.</summary>
        public JollyTexture AddText(string text, string fontFamily, float fontSize, JollyColor color, int x, int y, bool bold = false, bool italic = false, bool ultraBold = false, float opacity = 1.0f)
        {
            ValidateOpacity(opacity);
            JollyTexture textTexture = _renderer.CreateTextTexture(text, fontFamily, fontSize, color, bold, italic, ultraBold);
            AddOverlay(textTexture, x, y, opacity);
            return textTexture;
        }

        /// <summary>Removes all image and text overlays.</summary>
        public void ClearOverlays()
        {
            _overlays.Clear();
        }

        private void GenerateFrame()
        {
            for (int y = 0; y < Height; y++)
            {
                bool scanline = y % 4 == 0;
                int row = y * Width * 4;

                for (int x = 0; x < Width; x++)
                {
                    int red;
                    int green;
                    int blue;

                    switch (ColourMode)
                    {
                        case TvStaticColourMode.FullColour:
                            red = _random.Next(256);
                            green = _random.Next(256);
                            blue = _random.Next(256);
                            break;
                        case TvStaticColourMode.BlueTint:
                            red = _random.Next(80);
                            green = _random.Next(160);
                            blue = _random.Next(256);
                            break;
                        case TvStaticColourMode.GreenTint:
                            red = _random.Next(80);
                            green = _random.Next(256);
                            blue = _random.Next(160);
                            break;
                        case TvStaticColourMode.RedTint:
                            red = _random.Next(256);
                            green = _random.Next(120);
                            blue = _random.Next(120);
                            break;
                        default:
                            red = _random.Next(256);
                            green = red;
                            blue = red;
                            break;
                    }

                    if (scanline)
                    {
                        red = red * 3 / 5;
                        green = green * 3 / 5;
                        blue = blue * 3 / 5;
                    }

                    int index = row + x * 4;
                    _pixels[index] = (byte)blue;
                    _pixels[index + 1] = (byte)green;
                    _pixels[index + 2] = (byte)red;
                    _pixels[index + 3] = 255;
                }
            }
        }

        private void CompositeOverlays()
        {
            foreach (Overlay overlay in _overlays)
            {
                if (overlay.Texture.RendererTexture is not DirectXTexture directXTexture)
                {
                    continue;
                }

                ReadOnlySpan<byte> source = directXTexture.Image.Pixels.Span;

                for (int sourceY = 0; sourceY < overlay.Texture.Height; sourceY++)
                {
                    int destinationY = overlay.Y + sourceY;
                    if ((uint)destinationY >= (uint)Height)
                    {
                        continue;
                    }

                    for (int sourceX = 0; sourceX < overlay.Texture.Width; sourceX++)
                    {
                        int destinationX = overlay.X + sourceX;
                        if ((uint)destinationX >= (uint)Width)
                        {
                            continue;
                        }

                        int sourceIndex = (sourceY * overlay.Texture.Width + sourceX) * 4;
                        int destinationIndex = (destinationY * Width + destinationX) * 4;
                        float alpha = source[sourceIndex + 3] / 255.0f * overlay.Opacity;

                        if (alpha <= 0.0f)
                        {
                            continue;
                        }

                        float inverseAlpha = 1.0f - alpha;
                        _pixels[destinationIndex] = Blend(source[sourceIndex], _pixels[destinationIndex], alpha, inverseAlpha);
                        _pixels[destinationIndex + 1] = Blend(source[sourceIndex + 1], _pixels[destinationIndex + 1], alpha, inverseAlpha);
                        _pixels[destinationIndex + 2] = Blend(source[sourceIndex + 2], _pixels[destinationIndex + 2], alpha, inverseAlpha);
                        _pixels[destinationIndex + 3] = 255;
                    }
                }
            }
        }

        private static byte Blend(byte foreground, byte background, float alpha, float inverseAlpha)
        {
            return (byte)System.Math.Clamp((int)(foreground * alpha + background * inverseAlpha), 0, 255);
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
