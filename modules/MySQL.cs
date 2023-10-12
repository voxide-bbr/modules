using BBRAPIModules;
using MySqlConnector;

namespace Voxide;
[RequireModule(typeof(Library))]
[Module("MySQL", "1.0.0")]
public class MySQL : BattleBitModule
{
    public static async Task<List<string>> FetchRowsAsync(string connectionString)
    {
        var rows = new List<string>();

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand("SELECT * FROM example;", connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = reader.GetString(0); // adjust according to your table structure
            rows.Add(row);
        }

        return rows;
    }
}