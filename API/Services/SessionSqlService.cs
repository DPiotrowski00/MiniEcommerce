using API.Helpers;
using Dapper;
using System.Security.Policy;

namespace API.Services
{
    public class Session()
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? RefreshTokenHash { get; set; }
        public string? DeviceID { get; set; }
    }
    
    public interface ISessionSqlService
    {
        Task<Session?> GetSessionByDeviceId(int UserID, string DeviceID);
        Task<Session?> GetSessionByToken(string RefreshToken);
        Task CreateSession(Session session);
        Task UpdateSession(Session session);
        Task DeleteSession(string RefreshToken);
    }

    public class SessionSqlService (IConfiguration configuration) : ISessionSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task<Session?> GetSessionByDeviceId(int UserID, string DeviceID)
        {
            string query = """
                           SELECT * FROM sessions WHERE DeviceID = @DeviceID AND UserID = @UserID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return await connection.QuerySingleAsync<Session?>(query, new { DeviceID, UserID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Session?> GetSessionByToken(string RefreshToken)
        {
            string query = """
                           SELECT * FROM sessions WHERE RefreshTokenHash = @RefreshTokenHash
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return await connection.QuerySingleAsync<Session?>(query, new { RefreshTokenHash = HashHelper.ComputeSha256(RefreshToken) });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task UpdateSession(Session session)
        {
            string query = """
                           UPDATE sessions SET ExpiresAt = @ExpiresAt, RefreshTokenHash = @RefreshTokenHash WHERE ID = @ID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { session.ID, session.ExpiresAt, session.RefreshTokenHash });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public async Task CreateSession(Session session)
        {
            string query = """
                           INSERT INTO sessions (UserID, CreatedAt, ExpiresAt, RefreshTokenHash, DeviceID) VALUES (@UserID, @CreatedAt, @ExpiresAt, @RefreshTokenHash, @DeviceID)
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { session.UserID, session.CreatedAt, session.ExpiresAt, session.RefreshTokenHash, session.DeviceID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public async Task DeleteSession(string RefreshToken)
        {
            string query = """
                           DELETE FROM sessions WHERE RefreshTokenHash = @RefreshTokenHash
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { RefreshTokenHash = HashHelper.ComputeSha256(RefreshToken) });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
