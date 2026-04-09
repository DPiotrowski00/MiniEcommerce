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

        //Funkcja tworząca użytkownika.
        //Zwraca wartość true, jeśli wszystko przebiegło pomyślnie i wartość false, jeśli coś poszło nie tak.
        public async Task<bool> CreateUser(string Username, string Password)
        {
            string query = """
                           INSERT INTO logindata (Username, Password) VALUES (@Username, @HashedPassword)
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                var HashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
                await connection.ExecuteAsync(query, new { Username, HashedPassword });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
