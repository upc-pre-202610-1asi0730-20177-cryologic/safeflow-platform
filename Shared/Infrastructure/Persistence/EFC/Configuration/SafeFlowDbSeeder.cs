using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace SafeFlow.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Crea la base de datos (si no existe), el esquema de tablas y no inserta datos de ejemplo.
/// </summary>
public static class SafeFlowDbSeeder
{
    public static async Task InitializeAsync(
        AppDbContext context,
        IConfiguration configuration,
        CancellationToken ct = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        await EnsureDatabaseExistsAsync(connectionString, ct);
        await context.Database.EnsureCreatedAsync(ct);
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName)) return;

        builder.Database = string.Empty;
        await using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"CREATE DATABASE IF NOT EXISTS `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        await command.ExecuteNonQueryAsync(ct);
    }
}
