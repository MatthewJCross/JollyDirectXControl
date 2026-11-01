using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.Core.Graphics
{
    public abstract class JollyRenderer
    {
        public abstract JollyTexture LoadTexture(string filePath);

        public abstract JollySprite CreateSprite(JollyTexture texture);

        public JollySprite CreateSprite(string filePath)
        {
            JollyTexture texture = LoadTexture(filePath);
            return CreateSprite(texture);
        }

        internal virtual void Update(JollyGameTime gameTime)
        {
        }
    }
}
