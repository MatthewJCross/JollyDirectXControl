namespace Jolly2DElite.Game.Trading
{
    internal sealed class TradingService
    {
        private const int CommodityCount = 17;
        private const int TonnesCommodityCount = 13;
        private const int MaximumSpecialCargoQuantity = 200;
        private const int MaximumMarketAvailability = 63;

        public bool TryBuy(Commander commander, int commodityIndex, int quantity, int economy, out string message)
        {
            ArgumentNullException.ThrowIfNull(commander);

            if (!IsValidTransaction(commodityIndex, quantity, out message))
            {
                return false;
            }

            CommodityDefinition commodity = CommodityCatalogue.Commodities[commodityIndex];

            int available = commander.MarketAvailability[commodityIndex];

            if (quantity > available)
            {
                message = "INSUFFICIENT MARKET STOCK";
                return false;
            }

            if (!HasCargoSpace(commander, commodityIndex, quantity))
            {
                message = "CARGO HOLD FULL";
                return false;
            }

            int priceUnits = CommodityMarket.CalculatePriceUnits(commodity, economy, commander.MarketRandom);
            long totalCost = (long)priceUnits * 4 * quantity;

            if (totalCost > commander.Cash)
            {
                message = "INSUFFICIENT CASH";
                return false;
            }

            commander.Cash -= (uint)totalCost;
            commander.Cargo[commodityIndex] += quantity;
            commander.MarketAvailability[commodityIndex] -= quantity;
            message = $"BOUGHT {quantity} {commodity.Name.ToUpperInvariant()}";

            return true;
        }

        public bool TrySell(Commander commander, int commodityIndex, int quantity, int economy, out string message)
        {
            ArgumentNullException.ThrowIfNull(commander);

            if (!IsValidTransaction(commodityIndex, quantity, out message))
            {
                return false;
            }

            if (quantity > commander.Cargo[commodityIndex])
            {
                message = "NOT ENOUGH CARGO";
                return false;
            }

            CommodityDefinition commodity = CommodityCatalogue.Commodities[commodityIndex];

            int priceUnits = CommodityMarket.CalculatePriceUnits(commodity, economy, commander.MarketRandom);
            long totalSaleValue = (long)priceUnits * 4 * quantity;
            long newCash = commander.Cash + totalSaleValue;

            if (newCash > uint.MaxValue)
            {
                message = "CASH LIMIT EXCEEDED";
                return false;
            }

            commander.Cargo[commodityIndex] -= quantity;
            commander.Cash = (uint)newCash;
            commander.MarketAvailability[commodityIndex] = Math.Min(MaximumMarketAvailability, commander.MarketAvailability[commodityIndex] + quantity);
            message = $"SOLD {quantity} {commodity.Name.ToUpperInvariant()}";

            return true;
        }

        public int GetPriceUnits(Commander commander, int commodityIndex, int economy)
        {
            ArgumentNullException.ThrowIfNull(commander);

            if ((uint)commodityIndex >= CommodityCount)
            {
                throw new ArgumentOutOfRangeException(nameof(commodityIndex));
            }

            return CommodityMarket.CalculatePriceUnits(CommodityCatalogue.Commodities[commodityIndex], economy, commander.MarketRandom);
        }

        public decimal GetPriceCredits(Commander commander, int commodityIndex, int economy)
        {
            return GetPriceUnits(commander, commodityIndex, economy) * 0.4m;
        }

        private static bool IsValidTransaction(int commodityIndex, int quantity, out string message)
        {
            if ((uint)commodityIndex >= CommodityCount)
            {
                message = "INVALID COMMODITY";
                return false;
            }

            if (quantity <= 0)
            {
                message = "INVALID QUANTITY";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static bool HasCargoSpace(Commander commander, int commodityIndex, int quantity)
        {
            if (commodityIndex >= TonnesCommodityCount)
            {
                return commander.Cargo[commodityIndex] + quantity <= MaximumSpecialCargoQuantity;
            }

            int tonnesInHold = 0;

            for (int i = 0; i < TonnesCommodityCount; i++)
            {
                tonnesInHold += commander.Cargo[i];
            }

            return tonnesInHold + quantity <= commander.CargoCapacity;
        }
    }
}
