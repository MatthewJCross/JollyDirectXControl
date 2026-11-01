using Jolly2DEngine.Graphics;
using Jolly2DEngine.Rendering.Camera;

namespace Jolly2DEngine.Rendering
{
    public sealed class JollyRenderSubmission
    {
        public JollyRenderFrame Frame { get; }

        public CameraState Camera { get; }

        public JollyRenderSubmission(JollyRenderFrame frame, Camera2D camera)
        {
            ArgumentNullException.ThrowIfNull(frame);
            ArgumentNullException.ThrowIfNull(camera);

            Frame = frame;
            Camera = camera.CaptureState();
        }
    }
}


