using Npgsql;
using System.Runtime.CompilerServices;

namespace NotificationRealTime.Net10;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UserDao
{
    private readonly NpgsqlConnection _connection;
    public UserDao()
    {
        const string connectionString = "sua-connection-aqui";
        _connection = new NpgsqlConnection(connectionString);
    }

    public async IAsyncEnumerable<User> GetUsersAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await _connection.OpenAsync(ct);
        const string query = "select pa.upshot_document_id, pa.dt_performed_analysis from geoid.performed_analysis pa" +
                             " where pa.upshot_document_id is not null limit 10;";
        await using var command = new NpgsqlCommand(query, _connection);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            yield return new User
            {
                Id = reader.GetString(0),
                CreatedAt = reader.GetDateTime(1)
            };
        }
    }
}