using Microsoft.Extensions.DependencyInjection;
using TokenKit.Core.Extensions;
using TokenKit.Core.Implementations;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;
using TokenKit.Services.Encoders;

namespace TokenKit.Tests.Core.Services
{
    public class ValidationServiceTests
    {
        private static ITokenKitCore CreateCore()
        {
            var services = new ServiceCollection();

            var models = new List<ModelInfo>
            {
                new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 1000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" }
            };
            services.AddSingleton<IModelRegistry>(new InMemoryModelRegistry(models));

            services.AddSingleton<ITokenizerEngine, SimpleTextEncoder>();
            services.AddSingleton<ICostEstimator, BasicCostEstimator>();
            services.AddSingleton<ITokenKitCore, TokenKitCore>();

            return services.BuildServiceProvider().GetRequiredService<ITokenKitCore>();
        }

        [Fact]
        public async Task Validate_ShouldReturnValid_WhenWithinTokenLimit()
        {
            var core = CreateCore();
            var text = "Hello from TokenKit within limits!";
            var result = await core.ValidateAsync(new ValidateRequest { Text = text, ModelId = "gpt-4o" });

            Assert.True(result.IsValid);
            Assert.Equal("OK", result.Message);
        }

        [Fact]
        public async Task Validate_ShouldReturnInvalid_WhenExceedsLimit()
        {
            var core = CreateCore();
            var longText = string.Join(' ', Enumerable.Repeat("word", 2000)); // <-- fix
            var result = await core.ValidateAsync(new ValidateRequest { Text = longText, ModelId = "gpt-4o" });

            Assert.False(result.IsValid);
            Assert.Contains("Exceeds", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class InMemoryModelRegistry : IModelRegistry
        {
            private readonly List<ModelInfo> _models;
            public InMemoryModelRegistry(IEnumerable<ModelInfo> models) => _models = models.ToList();
            public ModelInfo? Get(string id) => _models.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));
            public IReadOnlyList<ModelInfo> GetAll(string? provider = null) =>
                string.IsNullOrWhiteSpace(provider)
                    ? _models
                    : _models.Where(m => string.Equals(m.Provider, provider, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
