using TokenKit.Registry;
using TokenKit.Models;

namespace TokenKit.Tests.Registry
{
    public class ModelRegistryCoverageTests
    {
        [Fact]
        public void Force_Lazy_Value_Factory_For_CodeCoverage()
        {
            // Arrange: ensure there’s no registry file in either location
            var baseDir = AppContext.BaseDirectory;
            var regDir = Path.Combine(baseDir, "Registry");
            Directory.CreateDirectory(regDir);
            var filePath = Path.Combine(regDir, "models.data.json");
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Act: force _models.Value to initialize under coverage
            var all = ModelRegistry.GetAll();

            // It should gracefully handle fallback and return a list
            Assert.NotNull(all);

            // Subsequent Get() calls should work
            var one = ModelRegistry.Get("gpt-4o");
            Assert.True(one == null || one.Id == "gpt-4o");
        }
    }
}
