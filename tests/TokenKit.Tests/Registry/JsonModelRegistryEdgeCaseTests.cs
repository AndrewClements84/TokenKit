using System.Text.Json;
using TokenKit.Core.Implementations;
using TokenKit.Core.Models;

namespace TokenKit.Tests.Core.Registry
{
    public class JsonModelRegistryEdgeCaseTests
    {
        [Fact]
        public void Should_Handle_Missing_File_Gracefully()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"TokenKitTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var missingFile = Path.Combine(tempDir, "models.data.json");

            // Act
            var registry = new JsonModelRegistry(missingFile);
            var result = registry.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Should_Return_Empty_When_JsonFile_Is_Empty_Array()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"TokenKitTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var jsonPath = Path.Combine(tempDir, "models.data.json");
            File.WriteAllText(jsonPath, "[]");

            // Act
            var registry = new JsonModelRegistry(jsonPath);
            var result = registry.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Should_Handle_Invalid_Json_Gracefully()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"TokenKitTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var jsonPath = Path.Combine(tempDir, "models.data.json");
            File.WriteAllText(jsonPath, "{ invalid json }");

            // Act
            var ex = Record.Exception(() =>
            {
                var reg = new JsonModelRegistry(jsonPath);
                _ = reg.GetAll();
            });

            // Assert: even if an exception occurs internally, it should not bubble to caller
            Assert.Null(ex);
        }

        [Fact]
        public void Should_Load_Valid_File_Correctly()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"TokenKitTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var jsonPath = Path.Combine(tempDir, "models.data.json");

            var models = new List<ModelInfo>
            {
                new() { Id = "gpt-4o", Provider = "OpenAI", MaxTokens = 128000, InputPricePer1K = 0.005m, OutputPricePer1K = 0.015m, Encoding = "cl100k_base" }
            };
            var json = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonPath, json);

            // Act
            var registry = new JsonModelRegistry(jsonPath);
            var all = registry.GetAll();
            var one = registry.Get("gpt-4o");

            // Assert
            Assert.NotNull(all);
            Assert.Single(all);
            Assert.NotNull(one);
            Assert.Equal("gpt-4o", one!.Id);
        }

        [Fact]
        public void Should_Filter_By_Provider_CaseInsensitive()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"TokenKitTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var jsonPath = Path.Combine(tempDir, "models.data.json");

            var models = new List<ModelInfo>
            {
                new() { Id = "a", Provider = "OpenAI" },
                new() { Id = "b", Provider = "Anthropic" },
                new() { Id = "c", Provider = "openai" }
            };
            var json = JsonSerializer.Serialize(models);
            File.WriteAllText(jsonPath, json);

            var registry = new JsonModelRegistry(jsonPath);

            // Act
            var openaiModels = registry.GetAll("OPENAI");

            // Assert
            Assert.Equal(2, openaiModels.Count);
            Assert.All(openaiModels, m => Assert.Equal("OpenAI", m.Provider, ignoreCase: true));
        }
    }
}
