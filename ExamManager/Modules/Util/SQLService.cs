using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using Windows.System;

// Initializes the SQL Database stuff that god knows what it actually does in the background

namespace ExamManager.Modules.Util
{
    internal class SQLService
    {
        string connectionString = @"Data Source=users.db;Version=3;";

        public void Initialize()
        {;

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string createTableQuery = @"CREATE TABLE IF NOT EXISTS Users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserName TEXT NOT NULL,
            Password TEXT NOT NULL,
            UserType TEXT NOT NULL
        );";

                    using (var command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Debug.WriteLine("Create table command executed.");
                    }
                }

                Debug.WriteLine("Started SQLService.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        public void AddUserToDB(string username, string password, string userType)
        {
        using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO Users (UserName, Password, UserType) VALUES (@username, @password, @userType);";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@userType", userType);

                    int result = command.ExecuteNonQuery();
                    Debug.WriteLine($"Inserted {result} row(s) into Users.");
                }
            }
        }

        public bool CheckTestUser(string username)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username;";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    long count = (long)command.ExecuteScalar();
                    Debug.WriteLine($"Found {count} matching user(s).");

                    return count > 0;
                }
            }
        }

        public bool CheckUserCredentials(string username, string password)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password;";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    long count = (long)command.ExecuteScalar();
                    Debug.WriteLine($"Found {count} matching user(s).");

                    return count > 0;
                }
            }
        }
    }
}
