using Jolly2DEngine.Core.Graphics;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.Camera;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jolly2DEngine.Diagnostics;

namespace Jolly2DEngine.Rendering.DirectX
{
    public sealed class DirectXJollyRenderer : JollyRenderer
    {
        private readonly DirectXControl _control;
        public Camera2D Camera { get; } = new();
        public bool LineGlowEnabled
        {
            get => _control.LineGlowEnabled;
            set => _control.LineGlowEnabled = value;
        }

        public float LineGlowWidth
        {
            get => _control.LineGlowWidth;
            set => _control.LineGlowWidth = value;
        }

        public float LineGlowStrength
        {
            get => _control.LineGlowStrength;
            set => _control.LineGlowStrength = value;
        }

        public float LineGlowFalloff
        {
            get => _control.LineGlowFalloff;
            set => _control.LineGlowFalloff = value;
        }

        public JollyDiagnostics? Diagnostics
        {
            get => _control.Diagnostics;
            set => _control.Diagnostics = value;
        }

        public System.Windows.Media.Color ClearColor
        {
            get => _control.ClearColor;
            set => _control.ClearColor = value;
        }

        public DirectXJollyRenderer(DirectXControl control)
        {
            ArgumentNullException.ThrowIfNull(control);
            _control = control;
            _control.RenderSizeChanged += Control_RenderSizeChanged;
            Camera.SetViewport((float)control.ActualWidth, (float)control.ActualHeight);
        }

        public override JollyTexture LoadTexture(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Texture file not found: {filePath}", filePath);
            }

            BitmapImage bitmap = new();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(Path.GetFullPath(filePath), UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmap.EndInit();
            bitmap.Freeze();

            byte[] alphaMask = CreateAlphaMask(bitmap);

            DirectXImage image = DirectXImage.FromBitmapSource(bitmap);
            DirectXTexture texture = new(image);

            _control.RegisterTexture(texture);

            return new JollyTexture(texture, texture.Image.Width, texture.Image.Height, alphaMask);
        }

        public JollyTexture CreateSolidTexture(byte red, byte green, byte blue, byte alpha = 255)
        {
            byte[] pixels =
            [
                blue,
                green,
                red,
                alpha
            ];

            BitmapSource bitmap = BitmapSource.Create(1, 1, 96.0, 96.0, PixelFormats.Bgra32, null, pixels, 4);
            bitmap.Freeze();

            DirectXImage image = DirectXImage.FromBitmapSource(bitmap);
            DirectXTexture texture = new(image);

            _control.RegisterTexture(texture);

            byte[] alphaMask =
            [
                alpha
            ];

            return new JollyTexture(texture, 1, 1, alphaMask);
        }

        public JollyTexture CreateCircleTexture(byte red, byte green, byte blue, byte alpha = 255, int size = 32)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);

            int stride = checked(size * 4);
            byte[] pixels = new byte[checked(stride * size)];
            byte[] alphaMask = new byte[checked(size * size)];

            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float distance = MathF.Sqrt(dx * dx + dy * dy);

                    float coverage = System.Math.Clamp(radius - distance + 0.5f, 0.0f, 1.0f);
                    byte pixelAlpha = (byte)(alpha * coverage);

                    int pixelIndex = y * stride + x * 4;
                    int alphaIndex = y * size + x;

                    pixels[pixelIndex] = blue;
                    pixels[pixelIndex + 1] = green;
                    pixels[pixelIndex + 2] = red;
                    pixels[pixelIndex + 3] = pixelAlpha;

                    alphaMask[alphaIndex] = pixelAlpha;
                }
            }

            BitmapSource bitmap = BitmapSource.Create(size, size, 96.0, 96.0, PixelFormats.Bgra32, null, pixels, stride);
            bitmap.Freeze();

            DirectXImage image = DirectXImage.FromBitmapSource(bitmap);
            DirectXTexture texture = new(image);

            _control.RegisterTexture(texture);

            return new JollyTexture(texture, size, size, alphaMask);
        }

        internal void UnloadTexture(JollyTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (texture.RendererTexture is not DirectXTexture directXTexture)
            {
                return;
            }

            _control.UnregisterTexture(directXTexture);
        }

        public override JollySprite CreateSprite(JollyTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (texture.RendererTexture is not DirectXTexture)
            {
                throw new InvalidOperationException("JollyTexture was not created by the DirectX renderer.");
            }

            return new JollySprite(texture);
        }

        public void SubmitFrame(JollyRenderFrame frame) 
        { 
            ArgumentNullException.ThrowIfNull(frame); 
            _control.SubmitRenderFrame(frame, Camera); 
        }

        private static byte[] CreateAlphaMask(BitmapSource source)
        {
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = checked(width * 4);

            byte[] pixels = new byte[checked(stride * height)];

            FormatConvertedBitmap converted = new(source, PixelFormats.Bgra32, null, 0.0);
            converted.CopyPixels(pixels, stride, 0);

            byte[] alphaMask = new byte[checked(width * height)];

            for (int y = 0; y < height; y++)
            {
                int sourceRow = y * stride;
                int destinationRow = y * width;

                for (int x = 0; x < width; x++)
                {
                    alphaMask[destinationRow + x] = pixels[sourceRow + x * 4 + 3];
                }
            }

            return alphaMask;
        }

        public JollyTexture CreateTextTexture(string text, string fontFamily, float fontSize, JollyColor color, bool bold = false, bool italic = false, bool ultraBold = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be empty.", nameof(text));
            }

            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fontSize, 0.0f);

            System.Windows.FontWeight fontWeight = ultraBold ? System.Windows.FontWeights.Black : bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;
            System.Windows.Media.Typeface typeface = new(new System.Windows.Media.FontFamily(fontFamily), italic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal, fontWeight, System.Windows.FontStretches.Normal);

            System.Windows.Media.Brush brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(color.A * 255.0f), (byte)(color.R * 255.0f), (byte)(color.G * 255.0f), (byte)(color.B * 255.0f)));

            brush.Freeze();

            System.Windows.Media.FormattedText formattedText = new(text, System.Globalization.CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight, typeface, fontSize, brush, 1.0);

            int width = System.Math.Max(1, (int)System.Math.Ceiling(formattedText.WidthIncludingTrailingWhitespace));
            int height = System.Math.Max(1, (int)System.Math.Ceiling(formattedText.Height));

            System.Windows.Media.DrawingVisual visual = new();

            using (System.Windows.Media.DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawText(formattedText, new System.Windows.Point(0.0, 0.0));
            }

            System.Windows.Media.Imaging.RenderTargetBitmap bitmap = new(width, height, 96.0, 96.0, System.Windows.Media.PixelFormats.Pbgra32);

            bitmap.Render(visual);
            bitmap.Freeze();

            DirectXImage image = DirectXImage.FromBitmapSource(bitmap);

            return CreateTextureResource(image, width, height);
        }

        public static float MeasureTextWidth(string text, string fontFamily, float fontSize, bool bold = false, bool ultraBold = false, bool italic = false)
        {
            System.Windows.Media.Typeface typeface = new(new System.Windows.Media.FontFamily(fontFamily), italic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal, ultraBold ? System.Windows.FontWeights.Black : bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal, System.Windows.FontStretches.Normal);
            System.Windows.Media.FormattedText formattedText = new(text, System.Globalization.CultureInfo.InvariantCulture, System.Windows.FlowDirection.LeftToRight, typeface, fontSize, System.Windows.Media.Brushes.White, 1.0);

            return (float)formattedText.WidthIncludingTrailingWhitespace;
        }

        public JollyTexture CreateTextureFromBgraPixels(int width, int height, byte[] pixels)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);
            ArgumentNullException.ThrowIfNull(pixels);

            int stride = checked(width * 4);

            if (pixels.Length != checked(stride * height))
            {
                throw new ArgumentException("Pixel buffer size does not match the specified dimensions.", nameof(pixels));
            }

            byte[] pixelCopy = (byte[])pixels.Clone();
            BitmapSource bitmap = BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Bgra32, null, pixelCopy, stride);
            bitmap.Freeze();

            DirectXImage image = DirectXImage.FromBitmapSource(bitmap);

            return CreateTextureResource(image, width, height);
        }

        public void UpdateTextureFromBgraPixels(JollyTexture texture, byte[] pixels)
        {
            ArgumentNullException.ThrowIfNull(texture);
            ArgumentNullException.ThrowIfNull(pixels);

            if (texture.RendererTexture is not DirectXTexture directXTexture)
            {
                throw new InvalidOperationException("JollyTexture was not created by the DirectX renderer.");
            }

            int expectedLength = checked(texture.Width * texture.Height * 4);

            if (pixels.Length != expectedLength)
            {
                throw new ArgumentException("Pixel buffer size does not match the texture dimensions.", nameof(pixels));
            }

            byte[] pixelCopy = (byte[])pixels.Clone();
            DirectXImage image = new(texture.Width, texture.Height, pixelCopy);
            _control.UpdateTexture(directXTexture, image);

            for (int i = 0; i < texture.AlphaMask.Length; i++)
            {
                texture.AlphaMask[i] = pixelCopy[i * 4 + 3];
            }
        }

        private JollyTexture CreateTextureResource(DirectXImage image, int width, int height)
        {
            DirectXTexture texture = new(image);

            _control.RegisterTexture(texture);

            byte[] alphaMask = new byte[checked(width * height)];

            ReadOnlySpan<byte> pixels = image.Pixels.Span;

            for (int i = 0; i < alphaMask.Length; i++)
            {
                alphaMask[i] = pixels[i * 4 + 3];
            }

            return new JollyTexture(texture, width, height, alphaMask);
        }
    
        public void Resize(float width, float height)
        {
            if (width <= 0.0f || height <= 0.0f)
            {
                return;
            }

            Camera.SetViewport(width, height);
        }

        private void Control_RenderSizeChanged(float width, float height)
        {
            Camera.SetViewport(width, height);
        }

        public float SetLogicalResolution(float width, float height)
        {
            Camera.Zoom = MathF.Min(Camera.ViewportSize.X / width, Camera.ViewportSize.Y / height);
            return Camera.Zoom;
        }
    }
}