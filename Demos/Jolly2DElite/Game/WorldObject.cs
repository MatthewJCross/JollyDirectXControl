using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class WorldObject
    {
        public ShipModel Model { get; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Scale { get; set; } = 1.0f;
        public Vector3 Velocity { get; set; }
        public bool IsPersistent { get; set; }

        public WorldObject(ShipModel model, Vector3 position)
        {
            Model = model;
            Position = position;
        }
    }
}