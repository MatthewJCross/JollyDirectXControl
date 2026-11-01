namespace Jolly2DElite.Rendering
{
    public sealed class SpriteFont
    {
        public const int GlyphWidth = 14;
        public const int GlyphHeight = 16;
        public const int Columns = 16;

        public int CharacterSpacing { get; set; } = 0;
        public int LineSpacing { get; set; } = 0;

        public int MeasureWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int maximumWidth = 0;
            int currentWidth = 0;

            foreach (char character in text)
            {
                if (character == '\n')
                {
                    maximumWidth = Math.Max(maximumWidth, currentWidth);
                    currentWidth = 0;
                    continue;
                }

                currentWidth += GlyphWidth + CharacterSpacing;
            }

            maximumWidth = Math.Max(maximumWidth, currentWidth);

            return maximumWidth > 0 ? maximumWidth - CharacterSpacing : 0;
        }

        public int MeasureHeight(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int lines = 1;

            foreach (char character in text)
            {
                if (character == '\n')
                {
                    lines++;
                }
            }

            return (lines * GlyphHeight) + ((lines - 1) * LineSpacing);
        }

        public (int X, int Y, int Width, int Height) GetGlyphSource(char character)
        {
            int characterCode = character;

            // Unsupported characters are rendered as a space.
            if (characterCode < 32 || characterCode > 255)
            {
                characterCode = 32;
            }

            int column = characterCode % Columns;
            int row = characterCode / Columns;

            return (column * GlyphWidth, row * GlyphHeight, GlyphWidth, GlyphHeight);
        }

        public IEnumerable<(char Character, int X, int Y)> EnumerateGlyphs(string text, int x, int y)
        {
            if (string.IsNullOrEmpty(text))
            {
                yield break;
            }

            int startX = x;
            int currentX = x;
            int currentY = y;

            foreach (char character in text)
            {
                if (character == '\r')
                {
                    continue;
                }

                if (character == '\n')
                {
                    currentX = startX;
                    currentY += GlyphHeight + LineSpacing;
                    continue;
                }

                yield return (character, currentX, currentY);

                currentX += GlyphWidth + CharacterSpacing;
            }
        }
    }
}
