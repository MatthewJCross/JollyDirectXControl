using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Jolly2DEngine.Math
{
    public readonly struct JollyColor : IEquatable<JollyColor>
    {
        public float R { get; }
        public float G { get; }
        public float B { get; }
        public float A { get; }

        public JollyColor(float r, float g, float b, float a = 1.0f)
        {
            R = System.Math.Clamp(r, 0.0f, 1.0f);
            G = System.Math.Clamp(g, 0.0f, 1.0f);
            B = System.Math.Clamp(b, 0.0f, 1.0f);
            A = System.Math.Clamp(a, 0.0f, 1.0f);
        }

        public static JollyColor White => new(1.0f, 1.0f, 1.0f, 1.0f);
        public static JollyColor Black => new(0.0f, 0.0f, 0.0f, 1.0f);
        public static JollyColor Red => new(1.0f, 0.0f, 0.0f, 1.0f);
        public static JollyColor Green => new(0.0f, 1.0f, 0.0f, 1.0f);
        public static JollyColor Blue => new(0.0f, 0.0f, 1.0f, 1.0f);
        public static JollyColor Yellow => new(1.0f, 1.0f, 0.0f, 1.0f);
        public static JollyColor Cyan => new(0.0f, 1.0f, 1.0f, 1.0f);
        public static JollyColor Magenta => new(1.0f, 0.0f, 1.0f, 1.0f);

        public static JollyColor Orange => FromBytes(255, 165, 0);
        public static JollyColor Purple => FromBytes(128, 0, 128);
        public static JollyColor Pink => FromBytes(255, 192, 203);
        public static JollyColor Brown => FromBytes(165, 42, 42);

        public static JollyColor Lime => FromBytes(0, 255, 0);
        public static JollyColor Navy => FromBytes(0, 0, 128);
        public static JollyColor Teal => FromBytes(0, 128, 128);
        public static JollyColor Olive => FromBytes(128, 128, 0);
        public static JollyColor Maroon => FromBytes(128, 0, 0);

        public static JollyColor Silver => FromBytes(192, 192, 192);
        public static JollyColor Gray => FromBytes(128, 128, 128);
        public static JollyColor DarkGray => FromBytes(64, 64, 64);
        public static JollyColor LightGray => FromBytes(211, 211, 211);

        public static JollyColor Gold => FromBytes(255, 215, 0);
        public static JollyColor SkyBlue => FromBytes(135, 206, 235);
        public static JollyColor RoyalBlue => FromBytes(65, 105, 225);
        public static JollyColor DodgerBlue => FromBytes(30, 144, 255);
        public static JollyColor DeepBlue => FromBytes(0, 0, 139);

        public static JollyColor DarkGreen => FromBytes(0, 100, 0);
        public static JollyColor ForestGreen => FromBytes(34, 139, 34);
        public static JollyColor LimeGreen => FromBytes(50, 205, 50);

        public static JollyColor DarkRed => FromBytes(139, 0, 0);
        public static JollyColor Crimson => FromBytes(220, 20, 60);

        public static JollyColor Violet => FromBytes(238, 130, 238);
        public static JollyColor Indigo => FromBytes(75, 0, 130);

        public static JollyColor Beige => FromBytes(245, 245, 220);
        public static JollyColor Ivory => FromBytes(255, 255, 240);
        public static JollyColor Khaki => FromBytes(240, 230, 140);

        public static JollyColor Transparent => new(0.0f, 0.0f, 0.0f, 0.0f);

        public static JollyColor FromBytes(byte r, byte g, byte b, byte a = 255)
        {
            return new JollyColor(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        public static JollyColor FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                throw new FormatException("Color hex string cannot be empty.");
            }

            string value = hex.Trim();

            if (value.StartsWith("#", StringComparison.Ordinal))
            {
                value = value[1..];
            }

            if (value.Length != 3 && value.Length != 4 && value.Length != 6 && value.Length != 8)
            {
                throw new FormatException("Hex colors must use RGB, RGBA, RRGGBB, or RRGGBBAA format.");
            }

            if (!IsHexString(value))
            {
                throw new FormatException($"Invalid hex color '{hex}'.");
            }

            if (value.Length == 3)
            {
                byte r = ExpandNibble(value[0]);
                byte g = ExpandNibble(value[1]);
                byte b = ExpandNibble(value[2]);

                return FromBytes(r, g, b);
            }

            if (value.Length == 4)
            {
                byte r = ExpandNibble(value[0]);
                byte g = ExpandNibble(value[1]);
                byte b = ExpandNibble(value[2]);
                byte a = ExpandNibble(value[3]);

                return FromBytes(r, g, b, a);
            }

            if (!byte.TryParse(value[0..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte red) ||
                !byte.TryParse(value[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte green) ||
                !byte.TryParse(value[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte blue))
            {
                throw new FormatException($"Invalid hex color '{hex}'.");
            }

            if (value.Length == 6)
            {
                return FromBytes(red, green, blue);
            }

            if (!byte.TryParse(value[6..8], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte alpha))
            {
                throw new FormatException($"Invalid hex color '{hex}'.");
            }

            return FromBytes(red, green, blue, alpha);
        }

        public static bool TryFromHex(string? hex, out JollyColor color)
        {
            color = Transparent;

            if (string.IsNullOrWhiteSpace(hex))
            {
                return false;
            }

            try
            {
                color = FromHex(hex);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public string ToHex()
        {
            return $"#{ToByte(R):X2}{ToByte(G):X2}{ToByte(B):X2}";
        }

        public string ToHex(bool includeAlpha)
        {
            if (!includeAlpha)
            {
                return ToHex();
            }

            return $"#{ToByte(R):X2}{ToByte(G):X2}{ToByte(B):X2}{ToByte(A):X2}";
        }

        public string ToHexRgb()
        {
            return ToHex();
        }

        public string ToHexRgba()
        {
            return ToHex(true);
        }

        public JollyColor WithAlpha(float alpha)
        {
            return new JollyColor(R, G, B, alpha);
        }

        public JollyColor WithOpacity(float opacity)
        {
            return WithAlpha(opacity);
        }

        public JollyColor Lighten(float amount)
        {
            amount = System.Math.Clamp(amount, 0.0f, 1.0f);

            return new JollyColor(
                R + ((1.0f - R) * amount),
                G + ((1.0f - G) * amount),
                B + ((1.0f - B) * amount),
                A);
        }

        public JollyColor Darken(float amount)
        {
            amount = System.Math.Clamp(amount, 0.0f, 1.0f);

            return new JollyColor(
                R * (1.0f - amount),
                G * (1.0f - amount),
                B * (1.0f - amount),
                A);
        }

        public JollyColor Multiply(float amount)
        {
            return new JollyColor(
                R * amount,
                G * amount,
                B * amount,
                A);
        }

        public JollyColor WithRgb(float r, float g, float b)
        {
            return new JollyColor(r, g, b, A);
        }

        public static JollyColor Lerp(JollyColor start, JollyColor end, float amount)
        {
            amount = System.Math.Clamp(amount, 0.0f, 1.0f);

            return new JollyColor(
                start.R + ((end.R - start.R) * amount),
                start.G + ((end.G - start.G) * amount),
                start.B + ((end.B - start.B) * amount),
                start.A + ((end.A - start.A) * amount));
        }

        public static JollyColor Blend(JollyColor source, JollyColor destination, float amount)
        {
            return Lerp(source, destination, amount);
        }

        public static JollyColor operator *(JollyColor color, float amount)
        {
            return color.Multiply(amount);
        }

        public static JollyColor operator *(float amount, JollyColor color)
        {
            return color.Multiply(amount);
        }

        public static JollyColor operator +(JollyColor left, JollyColor right)
        {
            return new JollyColor(
                left.R + right.R,
                left.G + right.G,
                left.B + right.B,
                left.A + right.A);
        }

        public static JollyColor operator -(JollyColor left, JollyColor right)
        {
            return new JollyColor(
                left.R - right.R,
                left.G - right.G,
                left.B - right.B,
                left.A - right.A);
        }

        public bool Equals(JollyColor other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        public override bool Equals(object? obj)
        {
            return obj is JollyColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        public override string ToString()
        {
            return ToHex(true);
        }

        public static bool operator ==(JollyColor left, JollyColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JollyColor left, JollyColor right)
        {
            return !left.Equals(right);
        }

        private static byte ToByte(float value)
        {
            return (byte)System.Math.Round(System.Math.Clamp(value, 0.0f, 1.0f) * 255.0f);
        }

        private static bool IsHexString(string value)
        {
            foreach (char character in value)
            {
                if (!Uri.IsHexDigit(character))
                {
                    return false;
                }
            }

            return true;
        }

        private static byte ExpandNibble(char value)
        {
            byte nibble = value switch
            {
                >= '0' and <= '9' => (byte)(value - '0'),
                >= 'A' and <= 'F' => (byte)(value - 'A' + 10),
                >= 'a' and <= 'f' => (byte)(value - 'a' + 10),
                _ => throw new FormatException($"Invalid hex digit '{value}'.")
            };

            return (byte)((nibble << 4) | nibble);
        }
    }
}
