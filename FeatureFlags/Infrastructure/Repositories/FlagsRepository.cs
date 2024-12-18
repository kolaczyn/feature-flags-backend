using Dapper;
using FeatureFlags.Domain.Errors;
using FeatureFlags.Domain.Models;
using FeatureFlags.Domain.Repositories;
using Npgsql;

namespace FeatureFlags.Infrastructure.Repositories;

public sealed class FlagsRepository(IConfiguration configuration) : IFlagsRepository
{
    private readonly IConfiguration _configuration = configuration;
    // private NpgsqlConnection _connection;

    private FlagDomain[] _flags =
    [
        new(Id: "1", Label: "greetUser", Value: true),
        new(Id: "2", Label: "aboutSection", Value: false)
    ];

    public async Task<FlagDomain[]> GetAll(CancellationToken ct)
    {
        var rows = await TestConnection(ct);
        Console.WriteLine($"Got: {rows}");
        return _flags;
    }

    public (FlagDomain?, IAppError?) PatchFlag(string id, bool value, CancellationToken ct)
    {
        var found = _flags.FirstOrDefault(x => x.Id == id);
        if (found == null)
        {
            return (null, new FlagDoesNotExist());
        }

        _flags = _flags.Select(x => x.Id == id ? new FlagDomain(Id: id, Label: x.Label, Value: value) : x).ToArray();

        var foundAfterChange = _flags.FirstOrDefault(x => x.Id == id)!;


        return (foundAfterChange, null);
    }

    private string ConnectionString() =>
        // TODO I should throw error if it's null
        _configuration.GetConnectionString("PostgresConnection")!;

    // TODO refactor this later
    private NpgsqlConnection GetConnection()
    {
        // NpgsqlDataSource.create lepiej reużywa połączenia
        // Npgsql.DepndencyInjection

        return new NpgsqlConnection(ConnectionString());
    }

    private async Task<string> TestConnection(CancellationToken ct)
    {
        await using var connection = GetConnection();
        var result = await connection.QueryFirstAsync<string>("SELECT 'Hello World'", ct);
        return result ?? "Nothing";
    }
}