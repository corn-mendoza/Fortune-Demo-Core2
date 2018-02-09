using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Steeltoe.Management.Endpoint.Health;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using Workshop_UI.Models;

namespace Workshop_UI
{
    public class SqlServerHealthContributor : IHealthContributor
    {
        AttendeeContext _context;
        ILogger<SqlServerHealthContributor> _logger;
        public SqlServerHealthContributor(AttendeeContext dbContext, ILogger<SqlServerHealthContributor> logger)
        {
            _context = dbContext;
            _logger = logger;
        }

        public string Id { get; } = "MSSql";

        public Health Health()
        {
            _logger.LogInformation("Checking MSSql connection health!");

            Health result = new Health();
            result.Details.Add("Database", $"MSSql: {_context.GetType().Name} ");
            SqlConnection _connection = null;
            try
            {
                _connection = _context.Database.GetDbConnection() as SqlConnection;
                if (_connection != null)
                {
                    _connection.Open();
                    DbCommand cmd = new SqlCommand("SELECT 1;", _connection);
                    var qresult = cmd.ExecuteScalar();
                    result.Details.Add("Result", qresult);
                    result.Details.Add("Status", HealthStatus.UP.ToString());
                    result.Status = HealthStatus.UP;
                    _logger.LogInformation($"MSSql Server {_context.GetType().Name} connection up!");
                }

            }
            catch (Exception e)
            {
                _logger.LogInformation("MSSQL Server connection down!");
                result.Details.Add("error", e.GetType().Name + ": " + e.Message);
                result.Details.Add("status", HealthStatus.DOWN.ToString());
                result.Status = HealthStatus.DOWN;
            }
            finally
            {
                if (_connection != null) _connection.Close();
            }

            return result;
        }
    }
}
// Lab11 End
