namespace Jolly2DTronCycles.Game
{
    public static class TronLevels
    {
        public static readonly TronLevel[] All =
        [
            new TronLevel( 1, "RPG",      1, 120.0f,  50.0f, 120.0f, 100.0f, 0.10f),
            new TronLevel( 2, "COBOL",    2, 135.0f,  70.0f, 180.0f, 100.0f, 0.20f),
            new TronLevel( 3, "BASIC",    3, 150.0f,  90.0f, 220.0f, 100.0f, 0.35f),
            new TronLevel( 4, "FORTRAN",  3, 165.0f, 100.0f, 240.0f, 100.0f, 0.50f),
            new TronLevel( 5, "SNOBOL",   3, 180.0f, 110.0f, 260.0f, 100.0f, 0.65f),
            new TronLevel( 6, "PL1",      3, 195.0f, 100.0f, 240.0f, 100.0f, 0.75f),
            new TronLevel( 7, "PASCAL",   3, 210.0f, 120.0f, 285.0f,  80.0f, 0.80f),
            new TronLevel( 8, "ALGOL",    3, 225.0f, 130.0f, 315.0f,  70.0f, 0.85f),
            new TronLevel( 9, "ASSEMBLY", 3, 240.0f, 150.0f, 360.0f,  40.0f, 0.90f),
            new TronLevel(10, "OS",       3, 255.0f, 160.0f, 390.0f,  20.0f, 0.95f),
            new TronLevel(11, "JCL",      3, 270.0f, 160.0f, 390.0f,  10.0f, 1.00f),
            new TronLevel(12, "USER",     3, 285.0f, 160.0f, 390.0f,   0.0f, 1.00f)
        ];

        public static TronLevel Get(int level)
        {
            int index = Math.Clamp(level, 1, All.Length) - 1;
            return All[index];
        }
    }
}