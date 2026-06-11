using MinGo.Identity.Configuration;
using Xunit;

namespace MinGo.Identity.UnitTests.Configuration;

public class GetFormBuilderMigrationsAssemblyTests
{
    [Theory]
    [InlineData(StorageTypes.SQLSERVER, "SidFormBuilder.SqlServerMigrations")]
    [InlineData(StorageTypes.POSTGRE, "SidFormBuilder.PostgreMigrations")]
    [InlineData(StorageTypes.MYSQL, "SidFormBuilder.MySQLMigrations")]
    [InlineData(StorageTypes.SQLITE, "SidFormBuilder.SqliteMigrations")]
    public void ShouldReturnCorrectAssemblyName(StorageTypes type, string expected)
    {
        var result = ConfigurationHelper.GetFormBuilderMigrationsAssembly(type);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void InMemory_ShouldReturnNull()
    {
        var result = ConfigurationHelper.GetFormBuilderMigrationsAssembly(StorageTypes.INMEMORY);
        Assert.Null(result);
    }

    [Fact]
    public void InvalidType_ShouldThrowArgumentOutOfRangeException()
    {
        var invalidType = (StorageTypes)999;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ConfigurationHelper.GetFormBuilderMigrationsAssembly(invalidType));
    }
}
