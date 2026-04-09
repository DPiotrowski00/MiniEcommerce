using API.Helpers;
using Dapper;
using MySqlX.XDevAPI;
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
        public bool IsRevoked { get; set; }
    }
    
    public interface ISessionSqlService
    {
        Task<Session?> GetSessionByDeviceId(int UserID, string DeviceID);
        Task<Session?> GetSessionByToken(string RefreshToken);
        Task RotateRefreshToken(Session session, string OldTokenHash);
        Task<int> CreateSession(Session session);
        Task RevokeSession(int SessionID);
        Task<int> CheckForTokenReuse(string token);
    }

    public class SessionSqlService (IConfiguration configuration) : ISessionSqlService
    {
        private readonly string _connectionString = configuration.GetConnectionString("Default")!;

        public async Task<Session?> GetSessionByDeviceId(int UserID, string DeviceID)
        {
            string query = """
                           SELECT * FROM sessions WHERE DeviceID = @DeviceID AND UserID = @UserID AND ExpiresAt < NOW() AND IsRevoked = 0
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
                           SELECT s.ID, s.UserID, s.CreatedAt, s.ExpiresAt, t.RefreshTokenHash, s.DeviceID, s.IsRevoked FROM sessions s JOIN tokens t ON t.SessionID = s.ID WHERE t.RefreshTokenHash = @RefreshTokenHash AND s.ExpiresAt < NOW() AND s.IsRevoked = 0
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

        public async Task RotateRefreshToken(Session session, string OldToken)
        {
            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync("INSERT INTO tokens (RefreshTokenHash, CreatedAt, ExpiresAt, SessionID) VALUES (@RefreshTokenHash, NOW(), @ExpiresAt, @ID)", new { session.RefreshTokenHash, session.CreatedAt, session.ExpiresAt, session.ID }, transaction);
                await connection.ExecuteAsync("UPDATE tokens SET ReplacedByTokenID = LAST_INSERT_ID(), IsRevoked = 1, RevokedAt = NOW(), RevokedReason = 'Refresh' WHERE RefreshTokenHash = @OldTokenHash ", new { session.RefreshTokenHash, session.CreatedAt, session.ExpiresAt, session.ID, OldTokenHash = HashHelper.ComputeSha256(OldToken) }, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transaction.Rollback();
                return;
            }
        }

        public async Task<int> CreateSession(Session session)
        {
            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            using var transaction = connection.BeginTransaction();
            try
            {
                var SessionID = await connection.ExecuteScalarAsync<int>("INSERT INTO sessions (UserID, DeviceID, CreatedAt, ExpiresAt, IsRevoked) VALUES (@UserID, @DeviceID, @CreatedAt, @ExpiresAt, 0); SELECT LAST_INSERT_ID();", new { session.UserID, session.CreatedAt, session.ExpiresAt, session.RefreshTokenHash, session.DeviceID }, transaction);
                await connection.ExecuteAsync("INSERT INTO tokens (RefreshTokenHash, ReplacedByTokenID, CreatedAt, ExpiresAt, IsRevoked, RevokedAt, RevokedReason, SessionID) VALUES (@RefreshTokenHash, NULL, @CreatedAt, @ExpiresAt, 0, NULL, NULL, @SessionID)", new { session.UserID, session.CreatedAt, session.ExpiresAt, session.RefreshTokenHash, session.DeviceID, SessionID }, transaction);

                transaction.Commit();

                return SessionID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                transaction.Rollback();
                return 0;
            }
        }

        public async Task RevokeSession(int SessionID)
        {
            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync("UPDATE sessions SET IsRevoked = 1 WHERE ID = @SessionID", new { SessionID }, transaction);
                await connection.ExecuteAsync("UPDATE tokens SET IsRevoked = 1, RevokedAt = NOW(), RevokedReason = 'Logged out' WHERE SessionID = @SessionID;", new { SessionID }, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                transaction.Rollback();
                return;
            }
        }

        public async Task<int> CheckForTokenReuse(string token)
        {
            string query = """
                           SELECT IFNULL((SELECT SessionId FROM tokens WHERE RefreshTokenHash = @RefreshTokenHash AND IsRevoked = 1 AND ReplacedByTokenID IS NOT NULL LIMIT 1), 0) AS SessionId;
                           """;

            using var connection = CreateSqlConnection.CreateConnection(_connectionString);
            try
            {
                return await connection.QuerySingleAsync<int>(query, new { RefreshTokenHash = HashHelper.ComputeSha256(token) });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}