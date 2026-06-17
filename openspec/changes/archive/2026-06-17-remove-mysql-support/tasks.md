## 1. Remove MySQL packages from csproj files

- [x] 1.1 Remove `Pomelo.EntityFrameworkCore.MySql` from `src/MinGo.Identity.Configuration/MinGo.Identity.Configuration.csproj`
- [x] 1.2 Remove `SimpleIdServer.IdServer.MySQLMigrations` and `SidFormBuilder.MySQLMigrations` from `src/MinGo.IdServer/MinGo.IdServer.csproj`
- [x] 1.3 Remove `Pomelo.Extensions.Caching.MySql` and `Hangfire.MySqlStorage` from `src/MinGo.IdServer/MinGo.IdServer.csproj`
- [x] 1.4 Remove `SimpleIdServer.IdServer.MySQLMigrations` and `SidFormBuilder.MySQLMigrations` from `src/MinGo.Identity.Migrator/MinGo.Identity.Migrator.csproj`

## 2. Replace MySQL code paths with NotSupportedException

- [x] 2.1 `src/MinGo.Identity.Configuration/ConfigurationHelper.cs`: Replace MYSQL switch arms with `throw new NotSupportedException(...)`, comment original code
- [x] 2.2 `src/MinGo.IdServer/Conf/SidServerSetup.cs`: Replace `ConfigureDuendeMigration` MYSQL case
- [x] 2.3 `src/MinGo.IdServer/Conf/SidServerSetup.cs`: Replace `ConfigureOpeniddictMigration` MYSQL case
- [x] 2.4 `src/MinGo.IdServer/Conf/SidServerSetup.cs`: Replace `ConfigureHangfire` MYSQL case
- [x] 2.5 `src/MinGo.IdServer/Conf/SidServerSetup.cs`: Replace `ConfigureDistributedCache` MYSQL case
- [x] 2.6 `src/MinGo.IdServer/Conf/Migrations/BeforeDeployment/DropDistributedCacheDataSeeder.cs`: Replace `db.IsMySql()` branch
- [x] 2.7 `src/MinGo.IdServer/Conf/Migrations/BeforeDeployment/ClientTypeDataSeeder.cs`: Replace `db.IsMySql()` branch

## 3. Fix CS0618 obsolete API warning

- [x] 3.1 `src/MinGo.IdServer/Conf/SidServerSetup.cs`: Fix `AddSqliteCache` obsolete call (line 377)

## 4. Fix CS860x nullable warnings in Razor views

- [x] 4.1 Fix CS8600/CS8604 in `Views/Shared/_FormBuilderLayout.cshtml` (LayoutResource.GetString nullability)
- [x] 4.2 Fix CS8600/CS8604/CS8602 in `Views/Shared/_Layout.cshtml` (LayoutResource.GetString + User.Identity)
- [x] 4.3 Fix CS8604 in `Views/Consents/Index.cshtml` (string.Join nullable)
- [x] 4.4 Fix CS8604 in `Views/BackChannelConsents/Index.cshtml` (string.Join nullable)
- [x] 4.5 Fix CS8602 in `Views/Home/Profile.cshtml` (Model property chain)
- [x] 4.6 Fix CS8602/CS8601 in all area authenticate/register views (HttpContextAccessor pattern)

## 5. Build verification

- [x] 5.1 Run `dotnet build` and confirm 0 warnings

## 6. Fix transitive NU1608 (Hangfire version alignment)

- [x] 6.1 Add `Hangfire.AspNetCore` and `Hangfire.NetCore` to CPM at 1.8.23
- [x] 6.2 Add direct PackageReference for `Hangfire.AspNetCore` and `Hangfire.NetCore` in `MinGo.IdServer.csproj`
- [x] 6.3 Remove unused `Hangfire.MySqlStorage` and `Pomelo.EntityFrameworkCore.MySql` from CPM

## 7. Build verification

- [x] 7.1 Run `dotnet build` and confirm 0 errors, 6 remaining NU1608 (transitive Pomelo from SimpleIdServer - unfixable without NoWarn or Pomelo 10.x)
