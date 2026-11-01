namespace Jolly2DElite.Game
{
    internal static class ShipCatalogue
    {
        public static ShipModel CreateCoriolisStation() => new CoriolisStationModel();
        public static ShipModel CreateCougar() => new CougarModel();
        public static ShipModel CreateAdder() => new AdderModel();
        public static ShipModel CreateAnaconda() => new AnacondaModel();
        public static ShipModel CreateCobraMkIII() => new CobraMkIIIModel();
        public static ShipModel CreateAsteroid() => new AsteroidModel();
        public static ShipModel CreatePlanet() => new PlanetModel();
        public static ShipModel CreateSun() => new SunModel();
    }
}
