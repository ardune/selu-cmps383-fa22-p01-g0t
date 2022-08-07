using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP22.P01.Tests.Web.Helpers;

namespace SP22.P01.Tests.Web;

//Hi 383 student - don't mess with this for the time being
[TestClass]
public class WeatherForecastTests
{
    private WebTestContext context;

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task GetWeather_ReturnsData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var result = await webClient.GetAsync("weatherForecast");

        //assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = result.Content.ReadAsJsonAsync<WeatherForecast[]>();
        content.Result.Should().NotBeNull().And.HaveCount(5);
    }
}