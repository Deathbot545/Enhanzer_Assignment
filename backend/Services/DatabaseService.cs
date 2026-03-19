using Applican.Api.Models;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Applican.Api.Services;

public class DatabaseService
{
    private readonly string _serverConnectionString;
    private readonly string _databaseName;

    public DatabaseService(IConfiguration configuration)
    {
        _serverConnectionString = configuration["SqlServer:ServerConnection"]
            ?? "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=True;";

        _databaseName = configuration["SqlServer:DatabaseName"] ?? "ApplicanAssignmentDb";
    }

    public async Task EnsureDatabaseAndTablesAsync(CancellationToken cancellationToken = default)
    {
        var safeDatabaseName = GetSafeDatabaseName(_databaseName);

        await using (var connection = new SqlConnection(_serverConnectionString))
        {
            await connection.OpenAsync(cancellationToken);

            var createDatabaseSql = $@"
IF DB_ID(N'{safeDatabaseName}') IS NULL
BEGIN
    CREATE DATABASE [{safeDatabaseName}]
END";

            await using var createDatabaseCommand = new SqlCommand(createDatabaseSql, connection);
            await createDatabaseCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var dbConnection = new SqlConnection(GetDatabaseConnectionString());
        await dbConnection.OpenAsync(cancellationToken);

        var createLocationTableSql = @"
IF OBJECT_ID('dbo.Location_Details', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Location_Details (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Location_Code NVARCHAR(50) NOT NULL,
        Location_Name NVARCHAR(200) NOT NULL,
        Created_On DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    )
END";

        var createPurchaseBillTableSql = @"
IF OBJECT_ID('dbo.Purchase_Bills', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Purchase_Bills (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Bill_Number NVARCHAR(50) NOT NULL,
        Supplier_Name NVARCHAR(150) NOT NULL,
        Bill_Date DATE NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Remarks NVARCHAR(300) NULL,
        Created_On DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    )
END";

        await using (var locationCommand = new SqlCommand(createLocationTableSql, dbConnection))
        {
            await locationCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var purchaseBillCommand = new SqlCommand(createPurchaseBillTableSql, dbConnection);
        await purchaseBillCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> SaveLocationsAsync(IEnumerable<UserLocation> locations, CancellationToken cancellationToken = default)
    {
        var locationList = locations.ToList();
        if (!locationList.Any())
        {
            return 0;
        }

        await using var connection = new SqlConnection(GetDatabaseConnectionString());
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var clearCommand = new SqlCommand("DELETE FROM dbo.Location_Details", connection, (SqlTransaction)transaction))
            {
                await clearCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            var insertSql = @"INSERT INTO dbo.Location_Details (Location_Code, Location_Name)
VALUES (@LocationCode, @LocationName)";

            foreach (var location in locationList)
            {
                await using var insertCommand = new SqlCommand(insertSql, connection, (SqlTransaction)transaction);
                insertCommand.Parameters.AddWithValue("@LocationCode", location.Location_Code);
                insertCommand.Parameters.AddWithValue("@LocationName", location.Location_Name);
                await insertCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return locationList.Count;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> AddPurchaseBillAsync(PurchaseBillCreateRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(GetDatabaseConnectionString());
        await connection.OpenAsync(cancellationToken);

        var sql = @"
INSERT INTO dbo.Purchase_Bills (Bill_Number, Supplier_Name, Bill_Date, Amount, Remarks)
VALUES (@BillNumber, @SupplierName, @BillDate, @Amount, @Remarks);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@BillNumber", request.BillNumber);
        command.Parameters.AddWithValue("@SupplierName", request.SupplierName);
        command.Parameters.AddWithValue("@BillDate", request.BillDate.Date);
        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is int id ? id : 0;
    }

    public async Task<IReadOnlyList<UserLocation>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        var locations = new List<UserLocation>();

        await using var connection = new SqlConnection(GetDatabaseConnectionString());
        await connection.OpenAsync(cancellationToken);

        var sql = @"SELECT Location_Code, Location_Name FROM dbo.Location_Details ORDER BY Location_Name";

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            locations.Add(new UserLocation
            {
                Location_Code = reader.GetString(0),
                Location_Name = reader.GetString(1)
            });
        }

        return locations;
    }

    private string GetDatabaseConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(_serverConnectionString)
        {
            InitialCatalog = GetSafeDatabaseName(_databaseName)
        };

        return builder.ConnectionString;
    }

    private static string GetSafeDatabaseName(string databaseName)
    {
        var safeName = Regex.Replace(databaseName, "[^a-zA-Z0-9_]", string.Empty);
        if (string.IsNullOrWhiteSpace(safeName))
        {
            throw new InvalidOperationException("Invalid database name configured.");
        }

        return safeName;
    }
}
