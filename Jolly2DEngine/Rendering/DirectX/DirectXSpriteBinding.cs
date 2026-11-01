
using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.Rendering.DirectX
{
    internal sealed class DirectXSpriteBinding
    {
        public JollySprite Sprite { get; }

        public DirectXSpriteState State { get; }

        public DirectXSpriteBinding(JollySprite sprite, DirectXTexture texture)
        {
            ArgumentNullException.ThrowIfNull(sprite);
            ArgumentNullException.ThrowIfNull(texture);

            Sprite = sprite;
            State = new DirectXSpriteState(texture);
        }

        public void Synchronize()
        {
            State.X = Sprite.X;
            State.Y = Sprite.Y;
            State.Width = Sprite.Width;
            State.Height = Sprite.Height;
            State.Rotation = Sprite.Rotation;
            State.Opacity = Sprite.Opacity;
            State.Visible = Sprite.Visible;
            State.Layer = Sprite.Layer;
        }
    }
}
