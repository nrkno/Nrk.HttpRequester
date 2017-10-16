using System;

namespace Nrk.HttpRequester.UnitTests.TestData
{
    public static class Some
    {
        public const string Product = "Application";
        public const string Version = "1.0.0";
        public const string Comment = "This is a comment";
        public const string DataCenter = "Azure-WE";
        public const string MachineName = "MachineName";

        public static readonly UserAgent UserAgent = new UserAgent(Product, Version);
        public static readonly Uri Uri = new Uri("https://pølse.med.lompe");
    }
}
