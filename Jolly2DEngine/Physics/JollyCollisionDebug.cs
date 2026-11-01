using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.Physics
{
    public static class JollyCollisionDebug
    {
        public static bool Enabled { get; set; }

        public static bool LogPixelChecks { get; set; }

        public static bool LogPixelHits { get; set; } = true;

        public static bool DrawColliders { get; set; }

        public static void CollisionEnter(JollyGameObject objectA, JollyGameObject objectB)
        {
            if (!Enabled)
            {
                return;
            }
        }

        public static void CollisionStay(JollyGameObject objectA, JollyGameObject objectB)
        {
            if (!Enabled)
            {
                return;
            }
        }

        public static void CollisionExit(JollyGameObject objectA, JollyGameObject objectB)
        {
            if (!Enabled)
            {
                return;
            }
        }

        public static void TriggerEnter(JollyGameObject objectA, JollyGameObject objectB)
        {
            if (!Enabled)
            {
                return;
            }
        }

        public static void TriggerStay(JollyGameObject objectA, JollyGameObject objectB)
        {
            if (!Enabled)
            {
                return;
            }
        }

        public static void TriggerExit(JollyGameObject objectA, JollyGameObject objectB)
        {
            if (!Enabled)
            {
                return;
            }
        }

        public static void PixelHit(JollySprite spriteA, JollySprite spriteB, int x, int y)
        {
            if (!Enabled || !LogPixelHits)
            {
                return;
            }
        }

        private static string GetName(JollyGameObject gameObject)
        {
            return string.IsNullOrWhiteSpace(gameObject.Name) ? gameObject.GetType().Name : gameObject.Name;
        }

        private static string GetName(JollySprite sprite)
        {
            return string.IsNullOrWhiteSpace(sprite.Name) ? "UnnamedSprite" : sprite.Name;
        }
    }
}