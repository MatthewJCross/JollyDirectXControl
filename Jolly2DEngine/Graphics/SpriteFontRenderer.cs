using Jolly2DEngine.Core.Graphics;
using Jolly2DEngine.Math;

namespace Jolly2DEngine.Graphics
{
    public sealed class SpriteFontRenderer
    {
        private readonly JollyRenderer _renderer;
        private readonly JollyTexture _texture;
        private readonly int _columns;
        private readonly int _characterWidth;
        private readonly int _characterHeight;
        private readonly int _firstCharacter;
        private readonly List<List<JollySprite>> _textSprites = [];

        public float CharacterSpacing { get; set; }
        public float LineSpacing { get; set; } = 16.0f;
        public float Scale { get; set; } = 1.0f;

        public IEnumerable<JollySprite> Sprites => _textSprites.SelectMany(static sprites => sprites);

        public SpriteFontRenderer(JollyRenderer renderer, JollyTexture texture, int columns, int characterWidth = 14, int characterHeight = 16, int firstCharacter = 32)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(texture);

            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }

            if (characterWidth <= 0 || characterHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(characterWidth));
            }

            _renderer = renderer;
            _texture = texture;
            _columns = columns;
            _characterWidth = characterWidth;
            _characterHeight = characterHeight;
            _firstCharacter = firstCharacter;
        }

        public void Clear()
        {
            _textSprites.Clear();
        }

        public void DrawText(string text, float x, float y)
        {
            ArgumentNullException.ThrowIfNull(text);

            List<JollySprite> sprites = [];
            float startX = x;
            int textureColumns = _columns;
            int textureRows = (int)(_texture.Height / _characterHeight);

            foreach (char character in text)
            {
                if (character == '\r')
                {
                    continue;
                }

                if (character == '\n')
                {
                    x = startX;
                    y += LineSpacing;
                    continue;
                }

                int characterIndex = character - _firstCharacter;

                if (characterIndex < 0 || characterIndex >= textureColumns * textureRows)
                {
                    characterIndex = '?' - _firstCharacter;
                }

                if (characterIndex < 0 || characterIndex >= textureColumns * textureRows)
                {
                    x += (_characterWidth + CharacterSpacing) * Scale;
                    continue;
                }

                int column = characterIndex % textureColumns;
                int row = characterIndex / textureColumns;

                JollySprite sprite = _renderer.CreateSprite(_texture);
                sprite.SourceX = (float)(column * _characterWidth) / _texture.Width;
                sprite.SourceY = (float)(row * _characterHeight) / _texture.Height;
                sprite.SourceWidth = (float)_characterWidth / _texture.Width;
                sprite.SourceHeight = (float)_characterHeight / _texture.Height;
                sprite.Position = new JollyVector2(x, y);
                sprite.Size = new JollyVector2(_characterWidth, _characterHeight);
                sprite.Scale = new JollyVector2(Scale, Scale);

                sprites.Add(sprite);

                x += (_characterWidth + CharacterSpacing) * Scale;
            }

            _textSprites.Add(sprites);
        }
    }
}