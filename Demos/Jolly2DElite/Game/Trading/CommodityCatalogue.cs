namespace Jolly2DElite.Game.Trading
{
    internal static class CommodityCatalogue
    {
        public static IReadOnlyList<CommodityDefinition> Commodities { get; } =
        [
            new("Food", "t", 19, -2, 6, 0x01),
            new("Textiles", "t", 20, -1, 10, 0x03),
            new("Radioactives", "t", 65, -3, 2, 0x07),
            new("Slaves", "t", 40, -5, -30, 0x1F),
            new("Liquor/Wines", "t", 83, -5, -5, 0x0F),
            new("Luxuries", "t", 196, 8, 54, 0x03),
            new("Narcotics", "t", 235, 29, 8, 0x78),
            new("Computers", "t", 154, 14, 56, 0x03),
            new("Machinery", "t", 117, 6, 40, 0x07),
            new("Alloys", "t", 78, 1, 17, 0x1F),
            new("Firearms", "t", 124, 13, 29, 0x07),
            new("Furs", "t", 176, -9, -36, 0x3F),
            new("Minerals", "t", 32, -1, 53, 0x03),
            new("Gold", "kg", 97, -1, 66, 0x07),
            new("Platinum", "kg", 171, -2, 55, 0x1F),
            new("Gem-Stones", "g", 45, -1, -6, 0x0F),
            new("Alien Items", "t", 53, 15, -64, 0x07)
        ];

        public static IReadOnlyDictionary<string, decimal> EquipmentPrices { get; } =
            new Dictionary<string, decimal>
            {
                ["Missile"] = 30.0m,
                ["Large Cargo Bay"] = 400.0m,
                ["E.C.M. System"] = 600.0m,
                ["Extra Pulse Lasers"] = 400.0m,
                ["Extra Beam Lasers"] = 1000.0m,
                ["Fuel Scoops"] = 525.0m,
                ["Escape Pod"] = 1000.0m,
                ["Energy Bomb"] = 900.0m,
                ["Energy Unit"] = 1500.0m,
                ["Docking Computers"] = 1000.0m,
                ["Galactic Hyperspace"] = 5000.0m,
                ["Military Laser"] = 6000.0m,
                ["Mining Laser"] = 800.0m
            };
    }
}