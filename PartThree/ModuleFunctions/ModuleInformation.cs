using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Text;

namespace ModuleFunctions
{
    public class ModuleInformation
    {

        // Method for hasing password.
        // @author https://www.sean-lloyd.com/post/hash-a-string/
        public string HashString(string text, string salt = "")
        {
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            // Uses SHA256 to create the hash
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                // Convert the string to a byte array first, to be processed
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                // Convert back to a string, removing the '-' that BitConverter adds
                string hash = BitConverter
                    .ToString(hashBytes)
                    .Replace("-", String.Empty);

                return hash;
            }
        }

        // Method to register a user and save the users username and password (hashed) into the database.
        public void registerUser(string username, string hashed)
        {
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // sql command to enter the username and password hash into the table titled users.
                string insertSql = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";

                using (SqlCommand command = new SqlCommand(insertSql, connection))
                {
                    // Add parameters for username and hashed password
                    // @author openAI
                    command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 255) { Value = username });
                    command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = hashed });

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // getLogIn method authenticates whether a user's entered log in credentials match that of those found in the database. Bool type chose to allow for
        // a quicker means of attaining authentication.
        public bool getLogIn(String username, String password)
        {
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // sql query to retrieve the username and password hash from the database.
                string selectSql = "SELECT PasswordHash FROM Users WHERE Username = @Username";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    // Define parameters for the query.
                    // @author openAI
                    command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 255) { Value = username });

                    // Execute the query and retrieve the password.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string retrievedHashedPassword = reader.GetString(0);

                            // Compare the database stored hash with a real time hash of the user's entered password.
                            if (retrievedHashedPassword == HashString(password))
                            {
                                // Success.
                                return true;
                            }
                        }
                    }
                }
            }
            // Failed.
            return false;
        }

        // Method to add a new module using the database.
        public void addModule(string moduleCode, string moduleName, int credits, int classHoursPerWeek, int semesterWeeks, DateTime semesterStartDate)
        {
            // Connection String.
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open sql Connection.
                connection.Open();

                // sql query to add the fields into the database.
                string insertSql = "INSERT INTO Modules (ModuleCode, ModuleName, Credits, ClassHoursPerWeek, SemesterWeeks, SemesterStartDate) " +
                                    "VALUES (@ModuleCode, @ModuleName, @Credits, @ClassHoursPerWeek, @SemesterWeeks, @SemesterStartDate)";

                using (SqlCommand command = new SqlCommand(insertSql, connection))
                {
                    // Add parameters for module data.
                    // @author openAI.
                    command.Parameters.Add(new SqlParameter("@ModuleCode", SqlDbType.NVarChar, 50) { Value = moduleCode });
                    command.Parameters.Add(new SqlParameter("@ModuleName", SqlDbType.NVarChar, 255) { Value = moduleName });
                    command.Parameters.Add(new SqlParameter("@Credits", SqlDbType.Int) { Value = credits });
                    command.Parameters.Add(new SqlParameter("@ClassHoursPerWeek", SqlDbType.Int) { Value = classHoursPerWeek });
                    command.Parameters.Add(new SqlParameter("@SemesterWeeks", SqlDbType.Int) { Value = semesterWeeks });
                    command.Parameters.Add(new SqlParameter("@SemesterStartDate", SqlDbType.Date) { Value = semesterStartDate });

                    command.ExecuteNonQuery();
                }
            }
        }

        // Method to get the module code from the database.
        public List<String> getModuleCodeFromDatabase()
        {
            // Creates a list of type string that stores module codes.
            List<string> moduleCodes = new List<string>();

            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // SQL query to retrieve module codes from the database
                string selectSql = "SELECT ModuleCode FROM Modules";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // get the index of 0.
                            string moduleCode = reader.GetString(0);
                            // add the module code from the database into the moduleCode list.
                            moduleCodes.Add(moduleCode);
                        }
                    }
                }
            }
            // return module code.
            return moduleCodes;

        }

        // Method that adds the self study hours per week of a user to the database.
        public void AddSelfStudyHours(int userId, string moduleId, int weekIndex, double hoursStudied)
        {
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertSql = "INSERT INTO SelfStudyHours (UserID, ModuleID, WeekIndex, HoursStudied) " +
                                   "VALUES (@UserID, @ModuleID, @WeekIndex, @HoursStudied)";

                using (SqlCommand command = new SqlCommand(insertSql, connection))
                {
                    // Add parameters for user ID, module ID, week index, and hours studied
                    command.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });
                    command.Parameters.Add(new SqlParameter("@ModuleID", SqlDbType.NVarChar) { Value = moduleId });
                    command.Parameters.Add(new SqlParameter("@WeekIndex", SqlDbType.Int) { Value = weekIndex });
                    command.Parameters.Add(new SqlParameter("@HoursStudied", SqlDbType.Float) { Value = hoursStudied });

                    command.ExecuteNonQuery();
                }
            }
        }

        // Method that finds the userID in the database by comparing the hashes found in the login page and the database.
        public int FindUserIdByPasswordHash(string hashedPassword, string username)
        {
            int userId = 0;
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // SQL query to retrieve the UserId based on both username and password hash.
                string selectSql = "SELECT UserId FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 255) { Value = username });
                    command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 255) { Value = hashedPassword });

                    // Execute the query and retrieve the UserId.
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = reader.GetInt32(reader.GetOrdinal("UserID"));

                        }
                    }
                }
            }

            return userId;
        }
        // Method that gets the start date of the semester from the database.
        public DateTime GetSemesterStartDate(string moduleCode)
        {
            DateTime semesterStartDate = DateTime.MinValue;

            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // sql query that finds the start date using the module code.
                string selectSql = "SELECT SemesterStartDate FROM Modules WHERE ModuleCode = @ModuleCode";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@ModuleCode", SqlDbType.NVarChar, 50) { Value = moduleCode });

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            semesterStartDate = reader.GetDateTime(0);
                        }

                    }
                }
            }

            // Returns the semester start date using the specific module code.
            return semesterStartDate;
        }

        // Method to calculate the remaining self study hours per week. this method will only showcase the remaining hours for a specific user.
        public string CalculateAndDisplayRemainingSelfStudyHours(string moduleCode, int totalWeeks, int classHoursPerWeek, int userId)
        {

            // Declare the variables to store method return values.
            double moduleCredits = GetModuleCreditsFromDatabase(moduleCode);
            DateTime semesterStartDate = GetSemesterStartDate(moduleCode);
            // List that will get the self study hours from the database and store the data.
            List<double> userSelfStudyHours = GetUserSelfStudyHoursFromDatabase(moduleCode, userId);

            // @author openAI
            StringBuilder result = new StringBuilder();
            double totalSelfStudyHours = (moduleCredits * 10) - (totalWeeks * classHoursPerWeek);

            if (userSelfStudyHours.Count < totalWeeks)
            {
                // Ensure the userSelfStudyHours list has enough elements
                for (int i = userSelfStudyHours.Count; i < totalWeeks; i++)
                {
                    userSelfStudyHours.Add(0.0);
                }
            }

            for (int weekIndex = 0; weekIndex < totalWeeks; weekIndex++)
            {
                double remainingHours = totalSelfStudyHours - userSelfStudyHours[weekIndex];

                // Append the information to the result StringBuilder
                result.AppendLine($"Module Code: {moduleCode}, Week: {weekIndex}, Remaining Hours: {remainingHours}");
            }

            return result.ToString();
        }

        // Method to get credits for a specific module from the database.
        public int GetModuleCreditsFromDatabase(string moduleCode)
        {
            int moduleCredits = 0;
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectSql = "SELECT Credits FROM Modules WHERE ModuleCode = @ModuleCode";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@ModuleCode", SqlDbType.NVarChar, 50) { Value = moduleCode });

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            moduleCredits = reader.GetInt32(0);
                        }
                    }
                }
            }
            // returns the module credits.
            return moduleCredits;
        }
        // get the selfstudy hours from database
        public List<double> GetUserSelfStudyHoursFromDatabase(string moduleCode, int userId)
        {
            List<double> userSelfStudyHours = new List<double>();
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectSql = "SELECT HoursStudied FROM SelfStudyHours WHERE ModuleID = @ModuleCode AND UserID = @UserId";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@ModuleCode", SqlDbType.NVarChar, 50) { Value = moduleCode });
                    command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            double hoursStudied = reader.GetDouble(0);
                            userSelfStudyHours.Add(hoursStudied);
                        }
                    }
                }
            }
            // return the self study hours.
            return userSelfStudyHours;
        }


        // get the total weeks from the database of a specific semester.
        public int GetTotalWeeksFromDatabase(string moduleCode)
        {
            int totalWeeks = 0;
            // connection string
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectSql = "SELECT SemesterWeeks FROM Modules WHERE ModuleCode = @ModuleCode";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@ModuleCode", SqlDbType.NVarChar) { Value = moduleCode });

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalWeeks = reader.GetInt32(0);
                        }
                    }
                }
            }
            // weeks in a semester
            return totalWeeks;
        }



        // Get the class hours per week
        public int GetClassHoursPerWeekFromDatabase(string moduleCode)
        {
            int classHoursPerWeek = 0;
            // connection string
            string connectionString = "Server=tcp:alikhazem.database.windows.net,1433;Initial Catalog=progpart2;Persist Security Info=False;User ID=alikhazem;Password=Drogo101;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectSql = "SELECT ClassHoursPerWeek FROM Modules WHERE ModuleCode = @ModuleCode";

                using (SqlCommand command = new SqlCommand(selectSql, connection))
                {
                    command.Parameters.Add(new SqlParameter("@ModuleCode", SqlDbType.NVarChar, 50) { Value = moduleCode });

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            classHoursPerWeek = reader.GetInt32(0);
                        }
                    }
                }
            }
            // return the class hours per week
            return classHoursPerWeek;
        }


    }
}





