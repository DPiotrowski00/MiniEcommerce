using System.Data;
using System.Configuration;
using System.Data.Common;
using MySql.Data.MySqlClient;
using Dapper;
using API.Helpers;

namespace API.Services
{
    //Interfejs serwisu SQL do wsrtzykiwania zależności
    public interface ILoggingSqlService
    {
        Task<int> ValidateLogIn(string Username, string Password);
        Task<bool> CreateUser(string Username, string Password);

        Task UpdateRefreshToken(int UserID, string Token);
        Task<bool> ValidateRefreshToken(int UserID, string Token);
        Task DeleteRefreshToken(int UserID);

        Task<int> GetIdFromRefreshToken(string Token);
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

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
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

        //Funkcja tworząca użytkownika.
        //Zwraca wartość true, jeśli wszystko przebiegło pomyślnie i wartość false, jeśli coś poszło nie tak.
        public async Task<bool> CreateUser(string Username, string Password)
        {
            string query = """
                           INSERT INTO logindata (Username, Password) VALUES (@Username, @HashedPassword)
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var HashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                await connection.ExecuteAsync(query, new { Username, HashedPassword });
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Funkcja aktualizująca refresh token dla danego użytkownika
        public async Task UpdateRefreshToken(int UserID, string Token)
        {
            string query = """
                           UPDATE logindata SET RefreshToken = @Token TokenExpiresAt = @TokenExpiresAt WHERE ID = @UserID
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var TokenHash = BCrypt.Net.BCrypt.HashPassword(Token);
                await connection.ExecuteAsync(query, new { UserID, TokenHash, TokenExpiresAt = DateTime.Now.AddDays(30) });
            }
            catch
            {
                return;
            }
        }

        //Funkcja walidująca refresh token przekazany przez użytkownika
        public async Task<bool> ValidateRefreshToken(int UserID, string Token)
        {
            string query = """
                           SELECT RefreshToken, TokenExpiresAt FROM logindata WHERE ID = @UserID
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var TokenData = await connection.QuerySingleAsync<KeyValuePair<string, DateTime>>(query, new { UserID });
                return BCrypt.Net.BCrypt.Verify(Token, TokenData.Key) && TokenData.Value > DateTime.Now;
            }
            catch
            {
                return false;
            }
        }

        //Funkcja usuwająca refresh token użytkownika (np. przy wylogowaniu)
        public async Task DeleteRefreshToken(int UserID)
        {
            string query = """
                           DELETE RefreshToken FROM logindata WHERE ID = @UserID
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { UserID });
            }
            catch
            {
                return;
            }
        }

        public async Task<int> GetIdFromRefreshToken(string Token)
        {
            string query = """
                           SELECT ID FROM logindata WHERE RefreshToken = @tokenHash
                           """;

            var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var TokenHash = BCrypt.Net.BCrypt.HashPassword(Token);
                return await connection.ExecuteAsync(query, new { TokenHash });
            }
            catch
            {
                return 0;
            }
        }
    }
}
