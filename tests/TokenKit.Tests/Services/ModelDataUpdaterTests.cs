using System.Text.Json;
using TokenKit.Models;
using TokenKit.Services;

namespace TokenKit.Tests.Services
{
    public class ModelDataUpdaterTests
    {
        [Fact]
        public void Constructor_ShouldCreateDirectory_WhenMissing()
        {
            // Arrange
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var relativeFolder = "RegistryTests";
            var relativeFile = Path.Combine(relativeFolder, "models.data.json");

            // Act
            var updater = new ModelDataUpdater(relativeFile);

            // Assert
            var expectedFullPath = Path.Combine(AppContext.BaseDirectory, relativeFolder);
            Assert.True(Directory.Exists(expectedFullPath));
        }

        [Fact]
        public async Task UpdateAsync_ShouldSerializeAndSaveFile()
        {
            // Arrange
            var relativePath = Path.Combine("RegistryTests", $"{Guid.NewGuid()}.json");
            var updater = new ModelDataUpdater(relativePath);

            // Act
            await updater.UpdateAsync(null); // uses fallback data

            // Assert
            var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
            Assert.True(File.Exists(fullPath));

            var json = await File.ReadAllTextAsync(fullPath);
            var models = JsonSerializer.Deserialize<List<ModelSpec>>(json);
            Assert.NotNull(models);
            Assert.True(models!.Count >= 3);
        }
    }
}
