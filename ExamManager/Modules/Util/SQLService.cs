﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using Windows.System;
using Microsoft.UI.Xaml.Controls;
using System.Xml.Linq;

// Initializes the SQL Database stuff that god knows what it actually does in the background

namespace ExamManager.Modules.Util
{
    internal class SQLService
    {
        string connectionString = @$"Data Source={App.DatabaseLocation};Version=3;";

        public class Candidate
        {
            public string name { get; set; }
            public string candidateNumber { get; set; }
            public string group { get; set; }
        }
        public void Initialize()
        {;

            try
            {
                Debug.WriteLine(App.DatabaseLocation);
                if (!File.Exists(App.DatabaseLocation))
                {
                    Debug.WriteLine("[SQL Service] Could'nt find the database file, creating a new one.");
                    File.Create(App.DatabaseLocation);
                    Debug.WriteLine("[SQL Service] Created database at the specified directory.");
                }

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

        public object CheckUserCredentials(string username, string password)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT UserName, Password, UserType FROM Users WHERE Username = @username AND Password = @password;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var usr = reader.GetString(0);
                            var pswrd = reader.GetString(1);
                            var usrLevel = reader.GetString(2);

                            return (
                                isUser: true, Level: int.Parse(usrLevel)
                                );
                        }
                    }

                    return (isUser: false, Level: 0);
                }
            }
        }

        public object GetUsers() // now works :tada:
        {
            var users = new HashSet<(string userName, string userType)>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT UserName, Password, UserType FROM Users;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userName = reader.GetString(0);
                            var password = reader.GetString(1);
                            var userType = reader.GetString(2);

                            var user = (userName, userType);

                            if (!users.Contains(user))
                            {
                                users.Add(user);
                            }
                        }

                        return users;
                    }
                }
            }
        }

        public bool RemoveUser(string user)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Users WHERE UserName = @user";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user", user);
                    var count = command.ExecuteNonQuery();

                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public object GetCandidates(string ageGroup) // now works :tada:
        {
            var candidates = new HashSet<(string StudentName, string CandidateNumber, string AgeGroup)>();

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
                            }
                        }

                        return candidates;
                    }
                }
            }
        }

        public object GetCandidateByNameAndYear(string name, string age)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT StudentName, CandidateNumber, AgeGroup FROM Students WHERE StudentName = @name AND AgeGroup = @ageGroup";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@ageGroup", age);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var studentName = reader.GetString(0);
                            var candidateNumber = reader.GetString(1);
                            var group = reader.GetString(2);

                            return (
                                name: studentName, number: candidateNumber, year: group);
                        }
                    }

                    return (name: "N/A", number: "N/A", year: "N/A");
                }
            }
        }

        public bool RemoveCandidate(string number)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM Students WHERE CandidateNumber = @number";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@number", number);
                    var count = command.ExecuteNonQuery();

                    if (count > 0)
                    {
                        return true;
                    } else
                    {
                        return false;
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
