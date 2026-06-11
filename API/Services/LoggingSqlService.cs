using Dapper;
using API.DataModels;

namespace API.Services
{
    //Interfejs serwisu SQL do wsrtzykiwania zależności
    public interface ILoggingSqlService
    {
        Task<int> ValidateLogIn(string Username, string Password);
        Task<bool> IsVerified(string Username);
        Task<bool> CreateUser(LogInData data);
        Task<bool> ChangePassword(int UserID, string oldPass, string newPass);
        Task<bool> ChangePasswordExplicit(int UserID, string password);
        Task<bool> VerifyEmail(string token);
        Task<UserModel> GetUserById(int UserID);
        Task<UserModel> GetUserByEmail(string email);
    }

    //Implementacja interfejsu ILoggingSqlService
    public class LoggingSqlService(IConfiguration configuration) : ILoggingSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        //Funkcja walidująca logowanie, sprawdza czy hasło zgadza się z hashem w bazie danych.
        //Jeśli walidacja przebiegła pomyślnie, zwraca id użytkownika, jeśli nie przebiegła pomyślnie, zwraca 0.
        public async Task<int> ValidateLogIn(string Username, string Password)
        {
            string query;

            //if (Username.Contains('@'))
            //{
            //    query = """
            //            SELECT ID, Password FROM logindata WHERE BINARY Email = @Username
            //            """;
            //}
            //else
            //{
            query = """
                    SELECT ID, Password FROM logindata WHERE BINARY Username = @Username
                    """;
            //}

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var data = await connection.QuerySingleAsync<LogInData>(query, new { Username });
                if(BCrypt.Net.BCrypt.Verify(Password, data.Password))
                {
                    return data.ID;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> IsVerified(string Username)
        {
            string query = """
                           SELECT Verified FROM logindata WHERE BINARY Username = @Username
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return await connection.QuerySingleAsync<bool>(query, new { Username });
            }
            catch
            {
                return false;
            }
        }

        //Funkcja tworząca użytkownika.
        //Zwraca wartość true, jeśli wszystko przebiegło pomyślnie i wartość false, jeśli coś poszło nie tak.
        public async Task<bool> CreateUser(LogInData data)
        {
            string query = """
                           INSERT INTO logindata (Username, Password, DisplayName, Verified, Email, VerificationToken) VALUES (@Login, @HashedPassword, @DisplayName, 0, @Email, @VerificationToken)
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var HashedPassword = BCrypt.Net.BCrypt.HashPassword(data.Password);
                await connection.ExecuteAsync(query, new { data.Login, HashedPassword, data.DisplayName, data.Email, data.VerificationToken });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> VerifyEmail(string token)
        {
            string query = """
                           UPDATE logindata SET Verified = 1, VerificationToken = NULL WHERE VerificationToken = @token
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { token });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> ChangePassword(int UserID, string oldPass, string newPass)
        {
            string checker = """
                             SELECT Password FROM logindata WHERE ID = @UserID
                             """;

            string setter = """
                            UPDATE logindata SET Password = @Password WHERE ID = @UserID
                            """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var passhash = await connection.QuerySingleAsync<string>(checker, new { UserID });
                if (BCrypt.Net.BCrypt.Verify(oldPass, passhash))
                {
                    var Password = BCrypt.Net.BCrypt.HashPassword(newPass);
                    await connection.ExecuteAsync(setter, new { Password, UserID });
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        
        public async Task<bool> ChangePasswordExplicit(int UserID, string password)
        {
            string query = """
                           UPDATE logindata SET Password = @Password WHERE ID = @UserID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var Password = BCrypt.Net.BCrypt.HashPassword(password);
                await connection.ExecuteAsync(query, new { Password, UserID });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<UserModel> GetUserById(int UserID)
        {
            string query = """
                           SELECT ID, Username, DisplayName, Email FROM logindata WHERE ID = @ID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return await connection.QuerySingleAsync<UserModel>(query, new { ID = UserID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new() { DisplayName = "", Email = "", Username = "" };
            }
        }
        
        public async Task<UserModel> GetUserByEmail(string email)
        {
            string query = """
                           SELECT ID, Username, DisplayName, Email FROM logindata WHERE Email = @email
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var result = await connection.QuerySingleAsync<UserModel>(query, new { email });
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new NotImplementedException();
            }
        }
    }
}
