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
        Task RotateRefreshToken(Session session);
        Task CreateSession(Session session);
        Task RevokeSession(Session session);
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
                           SELECT * FROM sessions s JOIN tokens t ON t.SessionID = s.ID WHERE t.RefreshTokenHash = @RefreshTokenHash
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

        public async Task RotateRefreshToken(Session session)
        {
            string query = """
                           START TRANSACTION;

                           INSERT INTO tokens (RefreshTokenHash, CreatedAt, ExpiresAt, SessionID)
                           VALUES (@RefreshTokenHash, NOW(), @ExpiresAt, @ID)

                           UPDATE tokens SET ReplacedByTokenID = LAST_INSERT_ID(), IsRevoked = 1, RevokedAt = NOW(), RevokedReason = 'Refresh' WHERE SessionID = @ID
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { session.RefreshTokenHash, session.CreatedAt, session.ExpiresAt, session.ID });
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
                           START TRANSACTION;

                           INSERT INTO sessions (UserID, DeviceID, CreatedAt, ExpiresAt, IsRevoked)
                           VALUES (@UserID, @DeviceID, @CreatedAt, @ExpiresAt, 0);

                           INSERT INTO tokens (RefreshTokenHash, ReplacedByTokenID, CreatedAt, ExpiresAt, IsRevoked, RevokedAt, RevokedReason, SessionID)
                           VALUES (@RefreshTokenHash, NULL, @CreatedAt, @ExpiresAt, 0, NULL, NULL, LAST_INSERT_ID());

                           COMMIT;
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { session.UserID, session.CreatedAt, session.ExpiresAt, session.RefreshTokenHash, session.DeviceID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        public async Task RevokeSession(Session session)
        {
            string query = """
                           UPDATE sessions SET IsRevoked = 1 WHERE ID = @ID;
                           UPDATE tokens SET IsRevoked = 1, RevokedAt = NOW(), RevokedReason = 'Logged out' WHERE SessionID = @ID;
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                await connection.ExecuteAsync(query, new { session.ID });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
