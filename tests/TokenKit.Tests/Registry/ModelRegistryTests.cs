using System.Text.Json;
using TokenKit.Models;

namespace TokenKit.Tests.Registry
{
    public class ModelRegistryTests
    {
        private static List<ModelSpec> TryLoadModelsForTest(string basePath)
        {
            var baseDir = basePath;
            var localPath = Path.Combine(baseDir, "Registry", "models.data.json");
            var fallbackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", "models.data.json");

            var filePath = File.Exists(localPath)
                ? localPath
                : File.Exists(fallbackPath)
                    ? fallbackPath
                    : null;

            if (filePath == null)
                throw new FileNotFoundException("Could not locate models.data.json in output or source Registry folder.");

            var json = File.ReadAllText(filePath);
            var models = JsonSerializer.Deserialize<List<ModelSpec>>(json);

            if (models == null || models.Count == 0)
                throw new InvalidDataException("Model registry file is empty or invalid JSON.");

            return models!;
        }

        private static List<ModelSpec> LoadModelsForTest(string basePath)
        {
            try
            {
                return TryLoadModelsForTest(basePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to load model registry: {ex.Message}");
                return new List<ModelSpec>();
            }
        }

        [Fact]
        public void Should_Throw_FileNotFound_When_NoRegistryExists()
        {
            var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(temp);

            // Temporarily isolate the fallback path
            var originalBaseDir = AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY");
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", temp);

            var ex = Record.Exception(() => TryLoadModelsForTest(temp));

            // restore
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY", originalBaseDir);

            Assert.IsType<FileNotFoundException>(ex);
        }

        [Fact]
        public void Should_Throw_InvalidData_When_EmptyJson()
        {
            var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var regDir = Path.Combine(temp, "Registry");
            Directory.CreateDirectory(regDir);

            var jsonPath = Path.Combine(regDir, "models.data.json");
            File.WriteAllText(jsonPath, "[]");

            var ex = Record.Exception(() => TryLoadModelsForTest(temp));
            Assert.IsType<InvalidDataException>(ex);
        }

        [Fact]
        public void Should_Catch_Exception_And_Return_EmptyList()
        {
            var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var regDir = Path.Combine(temp, "Registry");
            Directory.CreateDirectory(regDir);

            var jsonPath = Path.Combine(regDir, "models.data.json");
            File.WriteAllText(jsonPath, "{ invalid json }");

            var result = LoadModelsForTest(temp);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
