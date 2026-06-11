using MinGo.Identity.Configuration;
using Xunit;

namespace MinGo.Identity.UnitTests.Configuration;

public class GetSisMigrationsAssemblyTests
{
    [Theory]
    [InlineData(StorageTypes.SQLSERVER, "SimpleIdServer.IdServer.SqlServerMigrations")]
    [InlineData(StorageTypes.POSTGRE, "SimpleIdServer.IdServer.PostgreMigrations")]
    [InlineData(StorageTypes.MYSQL, "SimpleIdServer.IdServer.MySQLMigrations")]
    [InlineData(StorageTypes.SQLITE, "SimpleIdServer.IdServer.SqliteMigrations")]
    public void ShouldReturnCorrectAssemblyName(StorageTypes type, string expected)
    {
        var result = ConfigurationHelper.GetSisMigrationsAssembly(type);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void InMemory_ShouldReturnNull()
    {
        var result = ConfigurationHelper.GetSisMigrationsAssembly(StorageTypes.INMEMORY);
        Assert.Null(result);
    }

    [Fact]
    public void InvalidType_ShouldThrowArgumentOutOfRangeException()
    {
        var invalidType = (StorageTypes)999;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ConfigurationHelper.GetSisMigrationsAssembly(invalidType));
    }
}
