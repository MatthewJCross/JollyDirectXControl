namespace Jolly2DElite.Game
{
    internal static class UniverseFactory
    {
        public static List<WorldObject> CreateInitialObjects(int[] systemSeeds)
        {
            ArgumentNullException.ThrowIfNull(systemSeeds);

            if (systemSeeds.Length != 6)
            {
                throw new ArgumentException("Exactly six system seeds are required.", nameof(systemSeeds));
            }

            return [];
        }
    }
}


