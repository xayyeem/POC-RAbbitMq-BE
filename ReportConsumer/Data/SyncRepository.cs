using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportConsumer.Data
{
    public class SyncRepository
    {
        private readonly IConfiguration _configuration;

        public SyncRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString =>
            _configuration.GetConnectionString("DefaultConnection")!;

        public async Task<bool> IsDataSyncedAsync()
        {
            const string sql = @"
                SELECT IsSynced
                FROM SyncStatus
                WHERE Id = 1";

            await using var connection =
                new SqlConnection(ConnectionString);

            await using var command =
                new SqlCommand(sql, connection);

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
            {
                return false;
            }

            return Convert.ToBoolean(result);
        }
    }
}
