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
        Task<bool> VerifyEmail(string token);
        Task<UserModel> GetUser(int UserID);
    }

    //Implementacja interfejsu ILoggingSqlService
    public class LoggingSqlService(IConfiguration configuration) : ILoggingSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        //Funkcja walidująca logowanie, sprawdza czy hasło zgadza się z hashem w bazie danych.
        //Jeśli walidacja przebiegła pomyślnie, zwraca id użytkownika, jeśli nie przebiegła pomyślnie, zwraca 0.
        public async Task<int> ValidateLogIn(string Username, string Password)
        {
            string query = """
                           SELECT ID, Password FROM logindata WHERE BINARY Username = @Username
                           """;

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

        public async Task<UserModel> GetUser(int UserID)
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
    }
}
