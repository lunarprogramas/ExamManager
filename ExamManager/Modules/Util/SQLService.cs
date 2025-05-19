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
        string connectionString = @"Data Source=C:\Users\janko\source\repos\ExamManager\ExamManager\users1.db;Version=3;";

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

                    string createStudentTable = @"CREATE TABLE IF NOT EXISTS Students (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StudentName TEXT NOT NULL,
                        CandidateNumber TEXT NOT NULL,
                        AgeGroup TEXT NOT NULL
                    );";

                    string createExamHall = @"CREATE TABLE IF NOT EXISTS ExamHall (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Seats INTEGER NOT NULL,
                        CentreNumber INTEGER NOT NULL
                    );";


                    using (var command = new SQLiteCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                        Debug.WriteLine("Create user table command executed.");
                    }

                    using (var command = new SQLiteCommand(createStudentTable, connection))
                    {
                        command.ExecuteNonQuery();
                        Debug.WriteLine("Create student table command executed.");
                    }

                    using (var command = new SQLiteCommand(createExamHall, connection))
                    {
                        command.ExecuteNonQuery();
                        Debug.WriteLine("Create exam table command executed.");
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

        public object GetCandidates(string ageGroup) // now works :tada:
        {
            var candidates = new List<(string StudentName, string CandidateNumber, string AgeGroup)>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT StudentName, CandidateNumber, AgeGroup FROM Students WHERE AgeGroup = @ageGroup";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ageGroup", ageGroup);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var studentName = reader.GetString(0);
                            var candidateNumber = reader.GetString(1);
                            var group = reader.GetString(2);

                            var candidate = (studentName, candidateNumber, group);

                            if (!candidates.Contains(candidate))
                            {
                                candidates.Add(candidate);
                                Debug.WriteLine(studentName);
                                Debug.WriteLine(candidateNumber);
                                Debug.WriteLine(group);
                            }
                        }

                        return candidates;
                    }
                }
            }
        }

        public void AddCandidate(string name, string number, string ageGroup)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO Students (StudentName, CandidateNumber, AgeGroup) VALUES (@name, @number, @ageGroup);";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@number", number);
                    command.Parameters.AddWithValue("@ageGroup", ageGroup);

                    int result = command.ExecuteNonQuery();
                    Debug.WriteLine($"Inserted {result} row(s) into Students.");
                }
            }
        }
    }
}
