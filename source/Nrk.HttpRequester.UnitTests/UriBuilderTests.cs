using Shouldly;
using System.Collections.Specialized;
using Xunit;

namespace Nrk.HttpRequester.UnitTests
{
    public class UriBuilderTests
    {
        [Fact]
        public void Build_GivenNoParameters_ShouldReturnTemplatePath()
        {
            var template = "test/path";

            var path = UriBuilder.Build(template, new NameValueCollection());

            path.ShouldBe("/test/path");
        }

        [Fact]
        public void Build_GivenParameterWithNullValue_ShouldIgnoreEmptyParameter()
        {
            var template = "test/path";
            var parameters = new NameValueCollection { {"validSet", "value"}, {"invalidSet", null}};

            var path = UriBuilder.Build(template,parameters);

            path.ShouldBe("/test/path?validSet=value");
        }

        [Fact]
        public void Build_GivenParameterWithValue_ShouldGenerateCorrectPath()
        {
            var template = "test/{path}";
            var parameters = new NameValueCollection { { "path", "1234" }, { "invalidSet", null }, { "query", "parameter" } };

            var path = UriBuilder.Build(template, parameters);

            path.ShouldBe("/test/1234?query=parameter");
        }
    }
}
