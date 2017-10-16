namespace Nrk.HttpRequester.IntegrationTests.TestData
{
    public static class Some
    {
        public const string Product = "MyApplication";
        public const string Version = "1.0.0";
        public static readonly UserAgent UserAgent = new UserAgent(Product, Version);
    }
}
