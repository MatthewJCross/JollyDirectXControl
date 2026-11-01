using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using System.Numerics;

namespace Jolly2DEngine.World
{
    public sealed class JollyTileMap
    {
        private readonly Dictionary<int, JollyTileAnimation> _tileAnimations = new();
        private readonly Dictionary<int, float> _tileConveyors = new();
        private readonly HashSet<int> _hazardTiles = new();

        public event Action<JollyGameObject, int, int, int>? HazardContact;

        public JollyTexture? Texture { get; private set; }
        public int TextureColumns { get; private set; }
        public int TextureRows { get; private set; }
        public int TextureTileCount => TextureColumns * TextureRows;
        
        private readonly bool[,] _solid;
        private readonly int[,] _tiles;

        public int Width { get; }
        public int Height { get; }
        public float TileWidth { get; }
        public float TileHeight { get; }

        public JollyTileMap(int width, int height, float tileWidth, float tileHeight)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (tileWidth <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(tileWidth));
            }

            if (tileHeight <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(tileHeight));
            }

            Width = width;
            Height = height;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            _solid = new bool[width, height];
            _tiles = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = -1;
                }
            }
        }

        public void NotifyHazardContact(JollyGameObject gameObject, int tileX, int tileY, int tileIndex)
        {
            HazardContact?.Invoke(gameObject, tileX, tileY, tileIndex);
        }

        public bool IsSolid(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return false;
            }

            return _solid[x, y];
        }

        public void SetSolid(int x, int y, bool solid)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            _solid[x, y] = solid;
        }

        public int GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return -1;
            }

            return _tiles[x, y];
        }

        public void SetTile(int x, int y, int tileIndex)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (tileIndex < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(tileIndex));
            }

            _tiles[x, y] = tileIndex;
        }

        public Vector2 GetTilePosition(int x, int y)
        {
            return new Vector2(x * TileWidth, y * TileHeight);
        }

        public Vector2 WorldToTile(float x, float y)
        {
            return new Vector2(
                MathF.Floor(x / TileWidth),
                MathF.Floor(y / TileHeight));
        }

        public bool IsSolidAtWorldPosition(float x, float y)
        {
            int tileX = (int)MathF.Floor(x / TileWidth);
            int tileY = (int)MathF.Floor(y / TileHeight);
            return IsSolid(tileX, tileY);
        }

        public bool TryGetTileAtWorldPosition(float x, float y, out int tileIndex)
        {
            int tileX = (int)MathF.Floor(x / TileWidth);
            int tileY = (int)MathF.Floor(y / TileHeight);

            if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
            {
                tileIndex = -1;
                return false;
            }

            tileIndex = _tiles[tileX, tileY];
            return tileIndex >= 0;
        }

        public void SetTexture(JollyTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (texture.Width % TileWidth != 0.0f)
            {
                throw new ArgumentException("Texture width must divide evenly by the tile width.", nameof(texture));
            }

            if (texture.Height % TileHeight != 0.0f)
            {
                throw new ArgumentException("Texture height must divide evenly by the tile height.", nameof(texture));
            }

            Texture = texture;
            TextureColumns = (int)(texture.Width / TileWidth);
            TextureRows = (int)(texture.Height / TileHeight);
        }

        public bool TryGetTileSourceRectangle(int tileIndex, out int x, out int y, out int width, out int height)
        {
            if (Texture == null || tileIndex < 0 || tileIndex >= TextureTileCount)
            {
                x = 0;
                y = 0;
                width = 0;
                height = 0;
                return false;
            }

            int column = tileIndex % TextureColumns;
            int row = tileIndex / TextureColumns;

            x = column * (int)TileWidth;
            y = row * (int)TileHeight;
            width = (int)TileWidth;
            height = (int)TileHeight;

            return true;
        }

        public void SetAnimatedTile(int tileIndex, int startFrame, int frameCount, float framesPerSecond, bool loop = true)
        {
            if (tileIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tileIndex));
            }

            if (startFrame < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startFrame));
            }

            if (frameCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameCount));
            }

            if (framesPerSecond <= 0.0f || !float.IsFinite(framesPerSecond))
            {
                throw new ArgumentOutOfRangeException(nameof(framesPerSecond));
            }

            _tileAnimations[tileIndex] = new JollyTileAnimation(startFrame, frameCount, framesPerSecond, loop);
        }

        public void RemoveAnimatedTile(int tileIndex)
        {
            _tileAnimations.Remove(tileIndex);
        }

        public bool IsAnimatedTile(int tileIndex)
        {
            return _tileAnimations.ContainsKey(tileIndex);
        }

        public void SetConveyorTile(int tileIndex, float speed)
        {
            if (tileIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tileIndex));
            }

            if (!float.IsFinite(speed))
            {
                throw new ArgumentOutOfRangeException(nameof(speed));
            }

            if (speed == 0.0f)
            {
                _tileConveyors.Remove(tileIndex);
                return;
            }

            _tileConveyors[tileIndex] = speed;
        }

        public void RemoveConveyorTile(int tileIndex)
        {
            _tileConveyors.Remove(tileIndex);
        }

        public bool IsConveyorTile(int tileIndex)
        {
            return _tileConveyors.ContainsKey(tileIndex);
        }

        public float GetConveyorSpeed(int tileIndex)
        {
            return _tileConveyors.TryGetValue(tileIndex, out float speed) ? speed : 0.0f;
        }

        public int GetAnimatedTileIndex(int tileIndex, float elapsedSeconds)
        {
            if (!_tileAnimations.TryGetValue(tileIndex, out JollyTileAnimation animation))
            {
                return tileIndex;
            }

            if (animation.FrameCount <= 1)
            {
                return animation.StartFrame;
            }

            float frame = elapsedSeconds * animation.FramesPerSecond;
            int frameIndex = (int)MathF.Floor(frame);

            if (animation.Loop)
            {
                frameIndex %= animation.FrameCount;
            }
            else
            {
                frameIndex = System.Math.Min(frameIndex, animation.FrameCount - 1);
            }

            return animation.StartFrame + frameIndex;
        }

        public void SetHazardTile(int tileIndex, bool hazard)
        {
            if (tileIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tileIndex));
            }

            if (hazard)
            {
                _hazardTiles.Add(tileIndex);
            }
            else
            {
                _hazardTiles.Remove(tileIndex);
            }
        }

        public bool IsHazardTile(int tileIndex)
        {
            return _hazardTiles.Contains(tileIndex);
        }

        private readonly struct JollyTileAnimation
        {
            public int StartFrame { get; }
            public int FrameCount { get; }
            public float FramesPerSecond { get; }
            public bool Loop { get; }

            public JollyTileAnimation(int startFrame, int frameCount, float framesPerSecond, bool loop)
            {
                StartFrame = startFrame;
                FrameCount = frameCount;
                FramesPerSecond = framesPerSecond;
                Loop = loop;
            }
        }
    }
}