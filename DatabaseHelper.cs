using MySql.Data.MySqlClient;
using System.Data;

public class DatabaseHelper
{
    private MySqlConnection connection;

    public DatabaseHelper()
    {
        // Set up your connection string
        string connectionString = "server=localhost;database=gcy_ictprg431;user=root;password=;";
        connection = new MySqlConnection(connectionString);
    }

    public void OpenConnection()
    {
        if (connection.State == ConnectionState.Closed)
            connection.Open();
    }

    public void CloseConnection()
    {
        if (connection.State == ConnectionState.Open)
            connection.Close();
    }

    // Overload for queries without parameters
    public MySqlDataAdapter ExecuteQuery(string query)
    {
        MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
        return adapter;
    }

    // Overload for parameterized queries
    public MySqlDataAdapter ExecuteQuery(string query, params MySqlParameter[] parameters)
    {
        OpenConnection();
        MySqlCommand command = new MySqlCommand(query, connection);
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }
        MySqlDataAdapter adapter = new MySqlDataAdapter(command);
        CloseConnection();
        return adapter;
    }

    // For non-query commands like INSERT, UPDATE, DELETE
    public void ExecuteNonQuery(string query, params MySqlParameter[] parameters)
    {
        OpenConnection();
        MySqlCommand cmd = new MySqlCommand(query, connection);
        if (parameters != null)
        {
            cmd.Parameters.AddRange(parameters);
        }
        cmd.ExecuteNonQuery();
        CloseConnection();
    }
}
