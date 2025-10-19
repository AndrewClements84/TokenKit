using System.Text.Json;
using TokenKit.Core.Models;
using TokenKit.Services;

namespace TokenKit.Tests.Services
{
    public class ModelDataUpdaterTests
    {
        [Fact]
        public void Constructor_ShouldCreateDirectory_WhenMissing()
        {
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var relativeFolder = "RegistryTests";
            var relativeFile = Path.Combine(relativeFolder, "models.data.json");

            var updater = new ModelDataUpdater(relativeFile);

            var expectedFullPath = Path.Combine(AppContext.BaseDirectory, relativeFolder);
            Assert.True(Directory.Exists(expectedFullPath));
        }

        [Fact]
        public async Task UpdateAsync_ShouldSerializeAndSaveFile()
        {
            var relativePath = Path.Combine("RegistryTests", $"{Guid.NewGuid()}.json");
            var updater = new ModelDataUpdater(relativePath);

            await updater.UpdateAsync(null); // uses fallback data

            var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
            Assert.True(File.Exists(fullPath));

            var json = await File.ReadAllTextAsync(fullPath);
            var models = JsonSerializer.Deserialize<List<ModelInfo>>(json);

            Assert.NotNull(models);
            Assert.True(models!.Count >= 3);
        }
    }
}
