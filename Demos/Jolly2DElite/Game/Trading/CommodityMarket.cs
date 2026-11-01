namespace Jolly2DElite.Game.Trading
{
    internal static class CommodityMarket
    {
        public static void GenerateMarket(int economy, int marketRandom, int[] marketAvailability)
        {
            ArgumentNullException.ThrowIfNull(marketAvailability);

            if (marketAvailability.Length < CommodityCatalogue.Commodities.Count)
            {
                throw new ArgumentException("Market availability array must contain all commodities.", nameof(marketAvailability));
            }

            for (int i = 0; i < CommodityCatalogue.Commodities.Count; i++)
            {
                CommodityDefinition commodity = CommodityCatalogue.Commodities[i];

                marketAvailability[i] = CalculateAvailability(commodity, economy, marketRandom);
            }
        }

        public static int CalculatePriceUnits(CommodityDefinition commodity, int economy, int marketRandom)
        {
            ArgumentNullException.ThrowIfNull(commodity);
            ArgumentOutOfRangeException.ThrowIfLessThan(economy, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(economy, 7);

            int randomPrice = marketRandom & commodity.Fluctuation;
            int economicAdjustment = Math.Abs(commodity.EconomicFactor) * economy;

            int price = commodity.BasePrice + randomPrice;

            if (commodity.EconomicFactor < 0)
            {
                price -= economicAdjustment;
            }
            else
            {
                price += economicAdjustment;
            }

            return price & 0xFF;
        }

        public static decimal CalculatePriceCredits(CommodityDefinition commodity, int economy, int marketRandom)
        {
            return CalculatePriceUnits(commodity, economy, marketRandom) * 0.4m;
        }

        public static int CalculateAvailability(CommodityDefinition commodity, int economy, int marketRandom)
        {
            ArgumentNullException.ThrowIfNull(commodity);
            ArgumentOutOfRangeException.ThrowIfLessThan(economy, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(economy, 7);

            if (commodity.Name == "Alien Items")
            {
                return 0;
            }

            int randomQuantity = marketRandom & commodity.Fluctuation;
            int economicAdjustment = Math.Abs(commodity.EconomicFactor) * economy;

            int quantity = commodity.BaseQuantity + randomQuantity;

            if (commodity.EconomicFactor < 0)
            {
                quantity += economicAdjustment;
            }
            else
            {
                quantity -= economicAdjustment;
            }

            quantity &= 0xFF;

            if ((quantity & 0x80) != 0)
            {
                return 0;
            }

            return quantity & 0x3F;
        }
    }
}