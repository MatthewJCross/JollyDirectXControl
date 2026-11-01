using Jolly2DEngine.Core.Graphics;
using Jolly2DEngine.Graphics;
using System.IO;

namespace JollyDirectXTestCtrl.Rendering
{
    public sealed class JollyTextureManager
    {
        private readonly JollyRenderer _renderer;
        private readonly Dictionary<string, JollyTexture> _textures = new(StringComparer.OrdinalIgnoreCase);

        public JollyTextureManager(JollyRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            _renderer = renderer;
        }

        public int Count => _textures.Count;

        public JollyTexture Load(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            string fullPath = Path.GetFullPath(filePath);

            if (_textures.TryGetValue(fullPath, out JollyTexture? texture))
            {
                return texture;
            }

            texture = _renderer.LoadTexture(fullPath);
            _textures.Add(fullPath, texture);

            return texture;
        }

        public bool TryGet(string filePath, out JollyTexture? texture)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            string fullPath = Path.GetFullPath(filePath);

            return _textures.TryGetValue(fullPath, out texture);
        }

        public bool Contains(string filePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            string fullPath = Path.GetFullPath(filePath);

            return _textures.ContainsKey(fullPath);
        }

        public void Clear()
        {
            _textures.Clear();
        }
    }
}

