using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests
    {
        [Fact]
        public async Task Get_Claims()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(_ =>
                {});

            var client = application.CreateClient();

            //Added CancellationToken for the warning
            var response = await client.GetAsync("/Claims", TestContext.Current.CancellationToken);

            response.EnsureSuccessStatusCode();

            //TODO: Apart from ensuring 200 OK being returned, what else can be asserted?

            // 1. Assert that the response is JSON
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            // 2. Assert that we can successfully deserialize the result into a collection of Claims
            var claims = await response.Content.ReadFromJsonAsync<IEnumerable<Claim>>(
                cancellationToken: TestContext.Current.CancellationToken);
            
            Assert.NotNull(claims);
        }

    }
}
