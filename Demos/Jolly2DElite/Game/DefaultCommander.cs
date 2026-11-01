namespace Jolly2DElite.Game
{
    internal static class DefaultCommander
    {
        public const string Name = "JAMESON";

        public const int SystemX = 20;
        public const int SystemY = 173;

        public static ReadOnlySpan<byte> GalaxySeeds => [0x4A, 0x5A, 0x48, 0x02, 0x53, 0xB7];

        public const int Cash = 1000;
        public const int Fuel = 70;
        public const int FrontLaser = 15;
        public const int CargoCapacity = 22;
        public const int Missiles = 3;

        public static ReadOnlySpan<byte> MarketAvailability => [16, 15, 17, 0, 3, 28, 14, 0, 0, 10, 0, 17, 58, 7, 9, 8, 0];

        public const int SaveCount = 0x80;
    }
}