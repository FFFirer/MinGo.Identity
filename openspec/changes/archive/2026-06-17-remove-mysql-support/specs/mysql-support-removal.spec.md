## REMOVED Requirements

### Requirement: MySQL database support
**Reason**: Project uses PostgreSQL exclusively. MySQL packages cause NU1608 version constraint warnings against EF Core 10. Code paths are untested and unused.

**Migration**: Configure the application with PostgreSQL (StorageTypes.POSTGRE) instead of MySQL (StorageTypes.MYSQL).

### Requirement: MySQL distributed cache support
**Reason**: The `Pomelo.Extensions.Caching.MySql` package is unused. PostgreSQL distributed cache is the active configuration.

**Migration**: Configure the application with DistributedCacheTypes.POSTGRE instead of DistributedCacheTypes.MYSQL.

### Requirement: MySQL Hangfire storage
**Reason**: The `Hangfire.MySqlStorage` package is unused. PostgreSQL Hangfire storage is the active configuration.

**Migration**: Configure Hangfire with PostgreSQL storage instead of MySqlStorage.

### Requirement: MySQL migrations assembly references
**Reason**: `SimpleIdServer.IdServer.MySQLMigrations` and `SidFormBuilder.MySQLMigrations` are referenced but unused. PostgreSQL migration assemblies are used at runtime.

**Migration**: Remove these package references. Migration assembly resolution will throw NotSupportedException if MYSQL storage type is configured.
