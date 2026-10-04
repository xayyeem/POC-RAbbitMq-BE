using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ReportRepository
    {
        private readonly IConfiguration _configuration;

        public ReportRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString =>
            _configuration.GetConnectionString("DefaultConnection")!;

        public async Task InsertRequestAsync(
            Guid requestId,
            string reportName)
        {
            try
            {
                const string sql = @"
                INSERT INTO ReportRequests
                (
                    RequestId,
                    ReportName,
                    Status,
                    RequestedOn
                )
                VALUES
                (
                    @RequestId,
                    @ReportName,
                    @Status,
                    @RequestedOn
                )";

                await using var connection =
                    new SqlConnection(ConnectionString);

                await using var command =
                    new SqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@RequestId", requestId);

                command.Parameters.AddWithValue(
                    "@ReportName", reportName);

                command.Parameters.AddWithValue(
                    "@Status", "In-Queue");

                command.Parameters.AddWithValue(
                    "@RequestedOn", DateTime.UtcNow);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }catch(Exception ex)
            {
                               // Log the exception or handle it as needed
                Console.WriteLine(ex.Message);
                throw new Exception("An error occurred while inserting the report request.", ex);
            }
        }

        public async Task<List<ReportRequestStatus>> GetRequestsAsync()
        {
            const string sql = @"
                SELECT
                    RequestId,
                    ReportName,
                    Status,
                    RequestedOn,
                    ProcessingOn,
                    CompletedOn,
                    ExpiresOn,
                    FilePath
                FROM ReportRequests
                ORDER BY RequestedOn DESC";

            var requests = new List<ReportRequestStatus>();

            await using var connection =
                new SqlConnection(ConnectionString);

            await using var command =
                new SqlCommand(sql, connection);

            await connection.OpenAsync();

            await using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                requests.Add(new ReportRequestStatus
                {
                    RequestId = reader.GetGuid(0),
                    ReportName = reader.GetString(1),
                    Status = reader.GetString(2),
                    RequestedOn = reader.GetDateTime(3),

                    CompletedOn = reader.IsDBNull(5)
                        ? null
                        : reader.GetDateTime(5),

                    ExpiresOn = reader.IsDBNull(6)
                        ? null
                        : reader.GetDateTime(6),

                    FilePath = reader.IsDBNull(7)
                        ? null
                        : reader.GetString(7)
                });
            }

            return requests;
        }

        public async Task UpdateProcessingAsync(Guid requestId)
        {
            const string sql = @"
                UPDATE ReportRequests
                SET
                    Status = @Status,
                    ProcessingOn = @ProcessingOn
                WHERE RequestId = @RequestId";

            await using var connection =
                new SqlConnection(ConnectionString);

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Status", "Processing");

            command.Parameters.AddWithValue(
                "@ProcessingOn", DateTime.UtcNow);

            command.Parameters.AddWithValue(
                "@RequestId", requestId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateCompletedAsync(
            Guid requestId,
            string filePath)
        {
            const string sql = @"
                UPDATE ReportRequests
                SET
                    Status = @Status,
                    CompletedOn = @CompletedOn,
                    ExpiresOn = @ExpiresOn,
                    FilePath = @FilePath
                WHERE RequestId = @RequestId";

            await using var connection =
                new SqlConnection(ConnectionString);

            await using var command =
                new SqlCommand(sql, connection);

            var completedOn = DateTime.UtcNow;
            var expiresOn = completedOn.AddHours(24);

            command.Parameters.AddWithValue(
                "@Status", "Completed");

            command.Parameters.AddWithValue(
                "@CompletedOn", completedOn);

            command.Parameters.AddWithValue(
                "@ExpiresOn", expiresOn);

            command.Parameters.AddWithValue(
                "@FilePath", filePath);

            command.Parameters.AddWithValue(
                "@RequestId", requestId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task<ReportRequestStatus?> GetRequestByIdAsync(Guid requestId)
        {
            const string sql = @"
        SELECT
            RequestId,
            ReportName,
            Status,
            RequestedOn,
            ProcessingOn,
            CompletedOn,
            ExpiresOn,
            FilePath
        FROM ReportRequests
        WHERE RequestId = @RequestId";

            await using var connection =
                new SqlConnection(ConnectionString);

            await using var command =
                new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@RequestId",
                requestId);

            await connection.OpenAsync();

            await using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new ReportRequestStatus
            {
                RequestId = reader.GetGuid(0),
                ReportName = reader.GetString(1),
                Status = reader.GetString(2),
                RequestedOn = reader.GetDateTime(3),

                //ProcessingOn = reader.IsDBNull(4)
                //    ? null
                //    : reader.GetDateTime(4),

                CompletedOn = reader.IsDBNull(5)
                    ? null
                    : reader.GetDateTime(5),

                ExpiresOn = reader.IsDBNull(6)
                    ? null
                    : reader.GetDateTime(6),

                FilePath = reader.IsDBNull(7)
                    ? null
                    : reader.GetString(7)
            };
        }
    }
}
