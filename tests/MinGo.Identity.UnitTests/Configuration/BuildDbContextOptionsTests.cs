using Microsoft.EntityFrameworkCore;
using MinGo.Identity.Configuration;
using Xunit;

namespace MinGo.Identity.UnitTests.Configuration;

public class BuildDbContextOptionsTests
{
    [Fact]
    public void InMemory_WithNullMigrationsAssembly_ShouldNotThrow()
    {
        var config = new StorageConfiguration
        {
            Type = StorageTypes.INMEMORY,
            ConnectionString = "test_db"
        };
        var builder = new DbContextOptionsBuilder();

        var exception = Record.Exception(() =>
            ConfigurationHelper.BuildDbContextOptions(builder, config, null));

        Assert.Null(exception);
    }

    [Fact]
    public void InMemory_WithNullMigrationsAssembly_ShouldReturnBuilder()
    {
        var config = new StorageConfiguration
        {
            Type = StorageTypes.INMEMORY,
            ConnectionString = "test_db"
        };
        var builder = new DbContextOptionsBuilder();

        var result = ConfigurationHelper.BuildDbContextOptions(builder, config, null);

        Assert.Same(builder, result);
    }
}
