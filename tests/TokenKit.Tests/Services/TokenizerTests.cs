using Microsoft.Extensions.DependencyInjection;
using TokenKit.Core.Extensions;
using TokenKit.Core.Interfaces;
using TokenKit.Core.Models;
using TokenKit.Core.Implementations;
using TokenKit.Services.Encoders;

namespace TokenKit.Tests.Core.Services
{
    public class TokenizerTests
    {
        private static ITokenKitCore CreateCore()
        {
            var services = new ServiceCollection();

            // 1️⃣  Provide an in-memory registry with one model (no JSON needed)
            var models = new List<ModelInfo>
            {
                new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" }
            };
            services.AddSingleton<IModelRegistry>(new InMemoryModelRegistry(models));

            // 2️⃣  Register engines + cost estimator + core
            services.AddSingleton<ITokenizerEngine, SimpleTextEncoder>();
            services.AddSingleton<ITokenizerEngine, SharpTokenEncoder>();
            services.AddSingleton<ITokenizerEngine, MLTokenizersEncoder>();
            services.AddSingleton<ICostEstimator, BasicCostEstimator>();
            services.AddSingleton<ITokenKitCore, TokenKitCore>();

            return services.BuildServiceProvider().GetRequiredService<ITokenKitCore>();
        }

        [Fact]
        public async Task Analyze_ShouldReturnExpectedTokenCount()
        {
            var core = CreateCore();

            var result = await core.AnalyzeAsync(new AnalyzeRequest
            {
                Text = "Hello world from TokenKit",
                ModelId = "gpt-4o",
                Engine = "simple"
            });

            Assert.True(result.TokenCount > 0);
            Assert.Equal("simple", result.Engine);
        }

        [Fact]
        public void CostEstimator_ShouldReturnCorrectCost()
        {
            var model = new ModelInfo
            {
                Id = "mock-model",
                InputPricePer1K = 0.01m,
                OutputPricePer1K = 0.02m
            };
            var estimator = new BasicCostEstimator();
            var cost = estimator.EstimateTotal(model, 1500);
            Assert.Equal(Math.Round(1500 * 0.03m / 1000m, 6), cost);
        }

        [Fact]
        public async Task DifferentEncoders_ShouldReturnDifferentCounts()
        {
            var core = CreateCore();
            var text = "Hello world from TokenKit — multi-engine test!";

            var simple = await core.AnalyzeAsync(new AnalyzeRequest { Text = text, ModelId = "gpt-4o", Engine = "simple" });
            var sharp = await core.AnalyzeAsync(new AnalyzeRequest { Text = text, ModelId = "gpt-4o", Engine = "sharptoken" });

            Assert.NotEqual(simple.TokenCount, sharp.TokenCount);
            Assert.Equal("simple", simple.Engine);
            Assert.Equal("sharptoken", sharp.Engine);
        }

        [Fact]
        public async Task MLEngine_ShouldReturnTokenCount()
        {
            var core = CreateCore();
            var text = "TokenKit makes multi-encoder testing straightforward!";
            var ml = await core.AnalyzeAsync(new AnalyzeRequest { Text = text, ModelId = "gpt-4o", Engine = "mltokenizers" });

            Assert.True(ml.TokenCount > 0);
            Assert.Equal("mltokenizers", ml.Engine);
        }

        [Fact]
        public async Task InvalidEngine_ShouldFallbackToDefault()
        {
            var core = CreateCore();
            var result = await core.AnalyzeAsync(new AnalyzeRequest { Text = "Invalid engine test", ModelId = "gpt-4o", Engine = "unknown" });

            Assert.True(result.TokenCount > 0);
            Assert.Equal("simple", result.Engine);
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
