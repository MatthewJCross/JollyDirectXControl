using Jolly2DEngine.Rendering;

namespace Jolly2DEngine.Animation
{
    public sealed class JollyAnimation
    {
        public string Name { get; }

        public JollySpriteSheet Sheet { get; }

        public int StartFrame { get; }

        public int FrameCount { get; }

        public double FPS { get; }

        public bool Loop { get; }

        public JollyAnimation(string name, JollySpriteSheet sheet, int startFrame, int frameCount, double fps, bool loop = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(sheet);

            if (startFrame < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startFrame));
            }

            if (frameCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameCount));
            }

            if (startFrame + frameCount > sheet.FrameCount)
            {
                throw new ArgumentOutOfRangeException(nameof(frameCount), "The animation extends beyond the sprite sheet.");
            }

            if (fps <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(fps));
            }

            Name = name;
            Sheet = sheet;
            StartFrame = startFrame;
            FrameCount = frameCount;
            FPS = fps;
            Loop = loop;
        }

        public JollySpriteSheetFrame GetFrame(int animationFrame)
        {
            if (animationFrame < 0)
            {
                animationFrame = 0;
            }

            if (animationFrame >= FrameCount)
            {
                animationFrame = FrameCount - 1;
            }

            return Sheet.GetFrame(StartFrame + animationFrame);
        }
    }
}