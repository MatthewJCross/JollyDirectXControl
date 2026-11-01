namespace Jolly2DElite.Game.Trading
{
    internal sealed record CommodityDefinition(string Name, string Unit, int BasePrice, int EconomicFactor, int BaseQuantity, int Fluctuation);
}
