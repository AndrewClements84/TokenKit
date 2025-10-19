using TokenKit.Core.Implementations;
using TokenKit.Core.Models;

namespace TokenKit.Tests.Core.Registry
{
    public class JsonModelRegistryTests
    {
        [Fact]
        public void Should_Handle_Missing_File_Gracefully()
        {
            // Arrange: create a temp folder with no models.data.json
            var tempDir = Path.Combine(Path.GetTempPath(), $"TokenKitTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var missingPath = Path.Combine(tempDir, "models.data.json");

            // Act
            var registry = new JsonModelRegistry(missingPath);
            var all = registry.GetAll();

            // Assert: should not throw and should return an empty list
            Assert.NotNull(all);
            Assert.Empty(all);
        }

        [Fact]
        public void Should_Load_Models_From_Valid_File()
        {
            // Arrange: create a valid JSON file
            var tempDir = Path.Combine(Path.GetTempPath(), $"TokenKitTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var filePath = Path.Combine(tempDir, "models.data.json");

            var models = new List<ModelInfo>
            {
                new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" },
                new() { Id = "claude-3-opus", Provider = "Anthropic", MaxTokens = 200000, InputPricePer1K = 0.015m, OutputPricePer1K = 0.075m, Encoding = "claude-3" }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(models, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);

            // Act
            var registry = new JsonModelRegistry(filePath);
            var all = registry.GetAll();
            var one = registry.Get("gpt-4o");

            // Assert
            Assert.NotNull(all);
            Assert.NotEmpty(all);
            Assert.Equal(2, all.Count);
            Assert.NotNull(one);
            Assert.Equal("gpt-4o", one!.Id);
            Assert.Equal("OpenAI", one.Provider);
        }

        [Fact]
        public void GetAll_Should_Filter_By_Provider()
        {
            // Arrange
            var models = new List<ModelInfo>
            {
                new() { Id = "a", Provider = "OpenAI" },
                new() { Id = "b", Provider = "Anthropic" },
                new() { Id = "c", Provider = "OpenAI" }
            };
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, System.Text.Json.JsonSerializer.Serialize(models));

            var registry = new JsonModelRegistry(tempFile);

            // Act
            var openaiModels = registry.GetAll("OpenAI");

            // Assert
            Assert.Equal(2, openaiModels.Count);
            Assert.All(openaiModels, m => Assert.Equal("OpenAI", m.Provider));
        }
    }
}
