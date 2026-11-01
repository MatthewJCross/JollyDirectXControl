using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jolly2DEngine.Rendering.DirectX
{
    public sealed class DirectXImage
    {
        public int Width { get; }
        public int Height { get; }
        public ReadOnlyMemory<byte> Pixels { get; }

        public DirectXImage(int width, int height, ReadOnlyMemory<byte> pixels)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            int expectedLength = checked(width * height * 4);

            if (pixels.Length != expectedLength)
            {
                throw new ArgumentException($"Expected {expectedLength} RGBA bytes but received {pixels.Length}.", nameof(pixels));
            }

            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public static DirectXImage FromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Image file path cannot be empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Image file not found: {filePath}", filePath);
            }

            using FileStream stream = File.OpenRead(filePath);
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            return FromBitmapSource(bitmap);
        }


        public static DirectXImage FromBitmapSource(BitmapSource source)
        {
            ArgumentNullException.ThrowIfNull(source);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = checked(width * 4);
            byte[] pixels = new byte[checked(stride * height)];

            FormatConvertedBitmap converted = new();
            converted.BeginInit();
            converted.Source = source;
            converted.DestinationFormat = PixelFormats.Bgra32;
            converted.EndInit();
            converted.Freeze();

            converted.CopyPixels(pixels, stride, 0);

            int transparentPixels = 0;
            int partialAlphaPixels = 0;
            int opaquePixels = 0;
            int nonBlackPixels = 0;

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte blue = pixels[i];
                byte green = pixels[i + 1];
                byte red = pixels[i + 2];
                byte alpha = pixels[i + 3];

                if (alpha == 0)
                {
                    transparentPixels++;
                }
                else if (alpha == 255)
                {
                    opaquePixels++;
                }
                else
                {
                    partialAlphaPixels++;
                }

                if (red != 0 || green != 0 || blue != 0)
                {
                    nonBlackPixels++;
                }
            }

            return new DirectXImage(width, height, pixels);
        }
    }
}