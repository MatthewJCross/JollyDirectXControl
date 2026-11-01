namespace Jolly2DEngine.Graphics
{
    public sealed class JollyRenderFrame
    {
        private readonly List<JollySpriteSnapshot> _sprites;
        private readonly List<JollyLineSnapshot> _lines;
        private readonly List<JollyTriangleSnapshot> _triangles;

        public IReadOnlyList<JollySpriteSnapshot> Sprites => _sprites;
        public IReadOnlyList<JollyLineSnapshot> Lines => _lines;
        public IReadOnlyList<JollyTriangleSnapshot> Triangles => _triangles;

        public JollyRenderFrame(IEnumerable<JollySprite> sprites, IEnumerable<JollyLineSnapshot>? lines = null, IEnumerable<JollyTriangleSnapshot>? triangles = null)
        {
            ArgumentNullException.ThrowIfNull(sprites);

            _sprites = sprites.Where(static sprite => sprite.Visible).OrderBy(static sprite => sprite.Layer).Select(static sprite => sprite.CreateSnapshot()).ToList();
            _lines = lines?.Where(static line => line.Thickness > 0.0f).ToList() ?? [];
            _triangles = triangles?.ToList() ?? [];
        }
    }
}
