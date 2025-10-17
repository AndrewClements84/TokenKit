using System.Net;
using System.Text.Json;
using TokenKit.Services;

namespace TokenKit.Tests.Services
{
    public class ModelDataScraperTests
    {
        private class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
            public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) => _handler = handler;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_handler(request));
        }

        [Fact]
        public async Task FetchOpenAIModelsAsync_ShouldUseFallback_WhenNoApiKey()
        {
            var scraper = new ModelDataScraper();
            var result = await scraper.FetchOpenAIModelsAsync(null);

            Assert.NotNull(result);
            Assert.True(result.Count >= 3);
            Assert.All(result, m => Assert.Equal("OpenAI", m.Provider));
        }

        [Fact]
        public async Task FetchOpenAIModelsAsync_ShouldReturnParsedModels_OnSuccessfulResponse()
        {
            var json = JsonSerializer.Serialize(new
            {
                data = new[]
                {
                    new { id = "gpt-4o" },
                    new { id = "gpt-3.5-turbo" },
                    new { id = "other-model" }
                }
            });

            var handler = new FakeHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                });

            var client = new HttpClient(handler);
            var scraper = new ModelDataScraper(client);

            var result = await scraper.FetchOpenAIModelsAsync("sk-test-key");

            Assert.NotEmpty(result);
            Assert.Contains(result, m => m.Id == "gpt-4o");
            Assert.All(result, m => Assert.Equal("OpenAI", m.Provider));
        }

        [Fact]
        public async Task FetchOpenAIModelsAsync_ShouldFallback_WhenNoGptModelsFound()
        {
            var json = JsonSerializer.Serialize(new
            {
                data = new[] { new { id = "non-gpt-model" } }
            });

            var handler = new FakeHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                });

            var scraper = new ModelDataScraper(new HttpClient(handler));
            var result = await scraper.FetchOpenAIModelsAsync("sk-test-key");

            // fallback because there were no gpt-* models
            Assert.NotNull(result);
            Assert.True(result.Count >= 3);
            Assert.Contains(result, m => m.Id == "gpt-4o");
        }

        [Fact]
        public async Task FetchOpenAIModelsAsync_ShouldFallback_OnException()
        {
            var handler = new FakeHandler(_ => throw new HttpRequestException("network down"));
            var scraper = new ModelDataScraper(new HttpClient(handler));

            var result = await scraper.FetchOpenAIModelsAsync("sk-test-key");

            Assert.NotNull(result);
            Assert.True(result.Count >= 3);
        }
    }
}

