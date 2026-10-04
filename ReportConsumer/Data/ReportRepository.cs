using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportConsumer.Data
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

            var completedOn = DateTime.UtcNow;
            var expiresOn = completedOn.AddHours(24);

            await using var connection =
                new SqlConnection(ConnectionString);

            await using var command =
                new SqlCommand(sql, connection);

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
    }
}
