using Jolly2DEngine.Graphics;
using Jolly2DEngine.Rendering.Camera;
using System.Numerics;

namespace Jolly2DEngine.World
{
    public sealed class JollyTileMapRenderer
    {
        private readonly JollyTileMap _tileMap;
        private readonly Func<JollyTexture, JollySprite> _spriteFactory;
        private readonly Dictionary<(int X, int Y), JollySprite> _spriteCache = new();
        private readonly List<JollySprite> _sprites = new();

        private JollyTexture? _texture;
        private float _animationTime;

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public int Layer { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public JollyTileMapRenderer(JollyTileMap tileMap, Func<JollyTexture, JollySprite> spriteFactory)
        {
            ArgumentNullException.ThrowIfNull(tileMap);
            ArgumentNullException.ThrowIfNull(spriteFactory);

            _tileMap = tileMap;
            _spriteFactory = spriteFactory;
        }

        public void Rebuild()
        {
            _spriteCache.Clear();
            _sprites.Clear();

            if (_tileMap.Texture == null)
            {
                _texture = null;
                return;
            }

            _texture = _tileMap.Texture;

            for (int y = 0; y < _tileMap.Height; y++)
            {
                for (int x = 0; x < _tileMap.Width; x++)
                {
                    int tileIndex = _tileMap.GetTile(x, y);

                    if (tileIndex < 0)
                    {
                        continue;
                    }

                    JollySprite sprite = CreateTileSprite(x, y, tileIndex);

                    _spriteCache[(x, y)] = sprite;
                }
            }
        }

        public void Update(Camera2D camera, float deltaSeconds)
        {
            ArgumentNullException.ThrowIfNull(camera);

            _animationTime += MathF.Max(0.0f, deltaSeconds);

            _sprites.Clear();

            if (_tileMap.Texture == null)
            {
                return;
            }

            if (_texture != _tileMap.Texture)
            {
                Rebuild();
            }

            GetVisibleTileRange(camera, out int minimumX, out int maximumX, out int minimumY, out int maximumY);

            for (int y = minimumY; y <= maximumY; y++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    int tileIndex = _tileMap.GetTile(x, y);

                    if (tileIndex < 0)
                    {
                        continue;
                    }

                    int displayTileIndex = _tileMap.GetAnimatedTileIndex(tileIndex, _animationTime);

                    if (!_spriteCache.TryGetValue((x, y), out JollySprite? sprite))
                    {
                        sprite = CreateTileSprite(x, y, displayTileIndex);
                        _spriteCache[(x, y)] = sprite;
                    }
                    else
                    {
                        UpdateTileSprite(sprite, x, y, displayTileIndex);
                    }

                    sprite.Visible = true;
                    _sprites.Add(sprite);
                }
            }
        }

        private JollySprite CreateTileSprite(int tileX, int tileY, int tileIndex)
        {
            if (_texture == null)
            {
                throw new InvalidOperationException("Tile map texture has not been assigned.");
            }

            JollySprite sprite = _spriteFactory(_texture);

            UpdateTileSprite(sprite, tileX, tileY, tileIndex);

            return sprite;
        }

        private void UpdateTileSprite(JollySprite sprite, int tileX, int tileY, int tileIndex)
        {
            if (_texture == null)
            {
                return;
            }

            if (!_tileMap.TryGetTileSourceRectangle(
                tileIndex,
                out int sourceX,
                out int sourceY,
                out int sourceWidth,
                out int sourceHeight))
            {
                sprite.Visible = false;
                return;
            }

            sprite.X = X + tileX * _tileMap.TileWidth;
            sprite.Y = Y + tileY * _tileMap.TileHeight;
            sprite.Width = _tileMap.TileWidth;
            sprite.Height = _tileMap.TileHeight;
            sprite.SourceX = (float)sourceX / _texture.Width;
            sprite.SourceY = (float)sourceY / _texture.Height;
            sprite.SourceWidth = (float)sourceWidth / _texture.Width;
            sprite.SourceHeight = (float)sourceHeight / _texture.Height;
            sprite.Layer = Layer;
            sprite.Visible = true;
        }

        private void GetVisibleTileRange(Camera2D camera, out int minimumX, out int maximumX, out int minimumY, out int maximumY)
        {
            Vector2 topLeft = camera.ScreenToWorld(Vector2.Zero);
            Vector2 topRight = camera.ScreenToWorld(new Vector2(camera.ViewportSize.X, 0.0f));
            Vector2 bottomLeft = camera.ScreenToWorld(new Vector2(0.0f, camera.ViewportSize.Y));
            Vector2 bottomRight = camera.ScreenToWorld(camera.ViewportSize);

            float minimumWorldX = MathF.Min(topLeft.X, MathF.Min(topRight.X, MathF.Min(bottomLeft.X, bottomRight.X)));
            float maximumWorldX = MathF.Max(topLeft.X, MathF.Max(topRight.X, MathF.Max(bottomLeft.X, bottomRight.X)));
            float minimumWorldY = MathF.Min(topLeft.Y, MathF.Min(topRight.Y, MathF.Min(bottomLeft.Y, bottomRight.Y)));
            float maximumWorldY = MathF.Max(topLeft.Y, MathF.Max(topRight.Y, MathF.Max(bottomLeft.Y, bottomRight.Y)));

            minimumX = (int)MathF.Floor((minimumWorldX - X) / _tileMap.TileWidth) - 1;
            maximumX = (int)MathF.Floor((maximumWorldX - X) / _tileMap.TileWidth) + 1;
            minimumY = (int)MathF.Floor((minimumWorldY - Y) / _tileMap.TileHeight) - 1;
            maximumY = (int)MathF.Floor((maximumWorldY - Y) / _tileMap.TileHeight) + 1;

            minimumX = System.Math.Max(0, minimumX);
            maximumX = System.Math.Min(_tileMap.Width - 1, maximumX);
            minimumY = System.Math.Max(0, minimumY);
            maximumY = System.Math.Min(_tileMap.Height - 1, maximumY);
        }
    }
}