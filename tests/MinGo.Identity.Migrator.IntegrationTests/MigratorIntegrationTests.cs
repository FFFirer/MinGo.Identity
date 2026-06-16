using Npgsql;
using Xunit;
using Xunit.Abstractions;

namespace MinGo.Identity.Migrator.IntegrationTests;

/// <summary>
/// Migrator CLI 集成测试。
/// 所有测试共享同一个 DbFixture，依次串行执行。
/// </summary>
[Collection("Migrator Integration Tests")]
public class MigratorIntegrationTests : IClassFixture<DbFixture>
{
    private readonly DbFixture _fixture;
    private readonly ITestOutputHelper _output;

    public MigratorIntegrationTests(DbFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    // ============================================================
    //  migrate 命令 — 通过 MigratorService 直接调用
    // ============================================================

    [Fact]
    public async Task Migrate_ShouldSucceed()
    {
        var (exitCode, stdOut, stdErr) = await RunMigrateAsync();

        _output.WriteLine("=== stdout ===");
        _output.WriteLine(stdOut);
        if (!string.IsNullOrEmpty(stdErr))
        {
            _output.WriteLine("=== stderr ===");
            _output.WriteLine(stdErr);
        }

        Assert.Equal(0, exitCode);
        Assert.Contains("All operations completed successfully", stdOut);
    }

    [Fact]
    public async Task Migrate_ShouldBeIdempotent()
    {
        var (code1, out1, _) = await RunMigrateAsync();
        _output.WriteLine($"First run exit code: {code1}");
        Assert.Equal(0, code1);

        var (code2, out2, _) = await RunMigrateAsync();
        _output.WriteLine($"Second run exit code: {code2}");
        Assert.Equal(0, code2);
    }

    [Fact]
    public async Task Migrate_ShouldCreateEfHistory()
    {
        await RunMigrateAsync();

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """SELECT COUNT(*) FROM "__EFMigrationsHistory" """, conn);
        var count = (long)(await cmd.ExecuteScalarAsync())!;

        _output.WriteLine($"EF Migration History entries: {count}");
        Assert.True(count > 0, "Migration history is empty — no migrations were applied");
    }

    [Fact]
    public async Task Migrate_ShouldCreateSisTables()
    {
        await RunMigrateAsync();

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """
            SELECT COUNT(*) FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_type = 'BASE TABLE'
            """, conn);
        var tableCount = (long)(await cmd.ExecuteScalarAsync())!;

        _output.WriteLine($"Total tables after migration: {tableCount}");
        Assert.True(tableCount >= 10,
            $"Expected at least 10 tables after SIS migration, found {tableCount}");
    }

    // ============================================================
    //  script 命令
    // ============================================================

    [Fact]
    public async Task Script_ShouldGenerateSql()
    {
        var (exitCode, stdOut, stdErr) = await RunScriptAsync();

        _output.WriteLine(stdOut);
        if (!string.IsNullOrEmpty(stdErr))
        {
            _output.WriteLine("=== stderr ===");
            _output.WriteLine(stdErr);
        }

        Assert.Equal(0, exitCode);
        Assert.NotEmpty(stdOut);
        Assert.Contains("CREATE TABLE", stdOut, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Script_WithOutputFile_ShouldWriteSqlFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"migrate_script_{Guid.NewGuid():N}.sql");
        try
        {
            var (exitCode, stdOut, _) = await RunScriptAsync(tempFile);

            _output.WriteLine($"Script written to: {tempFile}");
            Assert.Equal(0, exitCode);

            var fileContent = await File.ReadAllTextAsync(tempFile);
            Assert.NotEmpty(fileContent);
            Assert.Contains("CREATE TABLE", fileContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    // ============================================================
    //  扩展表（需要先生成迁移文件）
    // ============================================================

    [Fact]
    public async Task ExtensionTable_UserProfiles_ShouldExist()
    {
        await RunMigrateAsync();

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            """SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'user_profiles')""",
            conn);
        var exists = (bool)(await cmd.ExecuteScalarAsync())!;

        _output.WriteLine($"Extension table 'user_profiles' exists: {exists}");
        Assert.True(exists,
            "'user_profiles' not found. Generate the initial migration:\n" +
            "  dotnet ef migrations add InitialCreate\n" +
            "    --project src/MinGo.Identity.Migrations\n" +
            "    --startup-project src/MinGo.Identity.Migrator\n" +
            "    --context ExtStoreDbContext");
    }

    // ============================================================
    //  Seed Data 验证 — SIDS-manager 客户端
    // ============================================================

    [Fact]
    public async Task Seed_ShouldCreateSidManagerClient()
    {
        await RunMigrateAsync();

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        // 验证 SIDS-manager 客户端存在（表名来自 EF Core 约定：Client → "Clients"）
        await using var clientCmd = new NpgsqlCommand(
            """SELECT "Id", "ClientSecret", "ClientId" FROM "Clients" WHERE "ClientId" = 'SIDS-manager'""",
            conn);
        await using var reader = await clientCmd.ExecuteReaderAsync();

        Assert.True(await reader.ReadAsync(),
            "SIDS-manager client was NOT created by migration pipeline. " +
            "Check that RegisterAdministrationSeeders() is called in MigratorService.");

        var clientId = reader.GetString(reader.GetOrdinal("ClientId"));
        var clientSecret = reader.GetString(reader.GetOrdinal("ClientSecret"));

        _output.WriteLine($"Found client: {clientId}");
        Assert.Equal("SIDS-manager", clientId);
        Assert.False(string.IsNullOrEmpty(clientSecret),
            "SIDS-manager client secret should not be empty");
    }

    [Fact]
    public async Task Seed_ShouldCreateSidManagerClientWithClientCredentialsGrant()
    {
        await RunMigrateAsync();

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        // SIDS-manager 必须有 client_credentials grant type（admin 容器依赖它换取 access token）
        // GrantTypes 存储为 JSON 数组字符串，如 '["client_credentials","authorization_code"]'
        await using var grantCmd = new NpgsqlCommand(
            """
            SELECT EXISTS (
                SELECT 1 FROM "Clients"
                WHERE "ClientId" = 'SIDS-manager'
                  AND "GrantTypes"::jsonb ? 'client_credentials'
            )
            """,
            conn);
        var hasClientCredentials = (bool)(await grantCmd.ExecuteScalarAsync())!;

        _output.WriteLine($"SIDS-manager has client_credentials grant: {hasClientCredentials}");
        Assert.True(hasClientCredentials,
            "SIDS-manager must support client_credentials grant type for admin API access");
    }

    [Fact]
    public async Task Seed_ShouldCreateAdministrativeScopes()
    {
        await RunMigrateAsync();

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        // 验证关键管理域存在（EF Core 表名：Scope → "Scopes"）
        await using var scopeCmd = new NpgsqlCommand(
            """SELECT COUNT(*) FROM "Scopes" WHERE "Name" = 'role' OR "Name" = 'provisioning'""",
            conn);
        var count = (long)(await scopeCmd.ExecuteScalarAsync())!;

        _output.WriteLine($"Administrative scopes (role, provisioning) found: {count}");
        Assert.True(count >= 2,
            "Expected at least 2 administrative scopes to be seeded");
    }

    [Fact]
    public async Task Seed_ShouldCreateDefaultUser()
    {
        // InitUserDataSeeder 创建 SIS 默认用户
        await RunMigrateAsync();

        await using var conn = new NpgsqlConnection(_fixture.ConnectionString);
        await conn.OpenAsync();

        await using var userCmd = new NpgsqlCommand(
            """SELECT COUNT(*) FROM "Users" WHERE "Realm" = 'master'""",
            conn);
        var userCount = (long)(await userCmd.ExecuteScalarAsync())!;

        _output.WriteLine($"Users in master realm: {userCount}");
        Assert.True(userCount > 0,
            "Expected at least one default user to be seeded in master realm");
    }

    // ============================================================
    //  端到端 CLI 验证 — 通过 dotnet run 进程调用
    // ============================================================

    [Fact]
    public async Task Cli_Migrate_ShouldSucceed()
    {
        var migratorProject = FindMigratorProjectPath();
        var envVars = new Dictionary<string, string>
        {
            ["StorageConfiguration__ConnectionString"] = _fixture.ConnectionString,
            ["StorageConfiguration__Type"] = "POSTGRE"
        };
        var (exitCode, stdOut, stdErr) = await RunProcessAsync(
            "dotnet",
            $"run --project \"{migratorProject}\" -- migrate",
            Path.GetDirectoryName(migratorProject)!,
            envVars
        );

        _output.WriteLine("=== CLI stdout ===");
        _output.WriteLine(stdOut);
        if (!string.IsNullOrEmpty(stdErr))
        {
            _output.WriteLine("=== CLI stderr ===");
            _output.WriteLine(stdErr);
        }

        Assert.Equal(0, exitCode);
        Assert.Contains("All operations completed successfully", stdOut);
    }

    // ============================================================
    //  辅助方法
    // ============================================================

    private async Task<(int, string, string)> RunMigrateAsync()
    {
        var stdOut = new StringWriter();
        var stdErr = new StringWriter();
        var originalOut = Console.Out;
        var originalError = Console.Error;

        try
        {
            Console.SetOut(stdOut);
            Console.SetError(stdErr);

            var service = CreateMigratorService();
            await service.RunMigrationsAsync();

            return (0, stdOut.ToString(), stdErr.ToString());
        }
        catch (Exception ex)
        {
            await stdErr.WriteLineAsync(ex.ToString());
            return (1, stdOut.ToString(), stdErr.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private async Task<(int, string, string)> RunScriptAsync(string? outputFile = null)
    {
        var stdOut = new StringWriter();
        var stdErr = new StringWriter();
        var originalOut = Console.Out;
        var originalError = Console.Error;

        try
        {
            Console.SetOut(stdOut);
            Console.SetError(stdErr);

            var service = CreateMigratorService();
            var outputInfo = outputFile != null ? new FileInfo(outputFile) : null;
            await service.GenerateScriptAsync(outputInfo);

            return (0, stdOut.ToString(), stdErr.ToString());
        }
        catch (Exception ex)
        {
            await stdErr.WriteLineAsync(ex.ToString());
            return (1, stdOut.ToString(), stdErr.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private MigratorService CreateMigratorService()
    {
        return new MigratorService(_fixture.StorageConfiguration);
    }

    private static async Task<(int, string, string)> RunProcessAsync(
        string fileName, string arguments, string workingDir,
        Dictionary<string, string>? environmentVariables = null)
    {
        var stdOut = new StringWriter();
        var stdErr = new StringWriter();

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        if (environmentVariables != null)
        {
            foreach (var kv in environmentVariables)
                startInfo.EnvironmentVariables[kv.Key] = kv.Value;
        }

        using var process = new System.Diagnostics.Process { StartInfo = startInfo };
        process.Start();
        var outTask = process.StandardOutput.ReadToEndAsync();
        var errTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        stdOut.Write(await outTask);
        stdErr.Write(await errTask);

        return (process.ExitCode, stdOut.ToString(), stdErr.ToString());
    }

    private static string FindMigratorProjectPath()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (Directory.GetFiles(dir, "*.slnx").Length > 0)
            {
                return Path.Combine(dir, "src", "MinGo.Identity.Migrator", "MinGo.Identity.Migrator.csproj");
            }
            dir = Path.GetDirectoryName(dir);
        }
        throw new InvalidOperationException(
            "Solution root not found from " + AppContext.BaseDirectory);
    }
}
