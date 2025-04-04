using DataWorkService.Models;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;

namespace DataWorkService.Service
{
    public class PostgreService : IPostgreService
    {
        private readonly ILogger<PostgreService> _logger;
        private readonly IConfiguration _configuration;

        public PostgreService(ILogger<PostgreService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void SaveDataToPostgres(string message)
        {
            var connectionString = GetPostgreSqlConnectionString();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    var futuresDataList = JsonConvert.DeserializeObject<List<FuturesPriceDifference>>(message) ?? throw new Exception("Parameter futuresData cannot be null");

                    //Валидация (предполагается, что метод IsValid существует и проверяет данные)
                    if (!IsValid(futuresDataList))
                    {
                        _logger.LogError("Invalid message format received: " + message);
                        throw new Exception("Invalid message format");
                    }

                    // SQL запрос для пакетной вставки
                    using (var writer = conn.BeginBinaryImport("COPY \"FuturesPriceDifferences\" (\"symbol1\", \"symbol2\", \"time\", \"difference\", \"interval\") FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (var futuresData in futuresDataList)
                        {
                            writer.StartRow();
                            writer.Write(futuresData.symbol1, NpgsqlDbType.Text);
                            writer.Write(futuresData.symbol2, NpgsqlDbType.Text);
                            writer.Write(futuresData.time, NpgsqlDbType.TimestampTz);
                            writer.Write(futuresData.difference, NpgsqlDbType.Numeric);
                            writer.Write(futuresData.interval, NpgsqlDbType.Text);
                        }

                        writer.Complete(); // Ensure data is fully written
                    }

                    _logger.LogInformation($"Bulk inserted {futuresDataList.Count} rows into PostgreSQL");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving data to PostgreSQL: " + ex.Message);
                    throw; // Re-throw to Nack the message
                }
            }
        }

        private string GetPostgreSqlConnectionString()
        {
            var host = _configuration["POSTGRESQL_HOST"];
            var port = _configuration["POSTGRESQL_PORT"];
            var database = _configuration["POSTGRESQL_DATABASE"];
            var userId = _configuration["POSTGRESQL_USERID"];
            var password = _configuration["POSTGRESQL_PASSWORD"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port)
                || string.IsNullOrEmpty(database) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("One or more PostgreSQL environment variables are missing.");
            }

            return $"Host={host};Port={port};Database={database};User Id={userId};Password={password}";
        }

        //Валидация
        private bool IsValid(object obj)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj);
            return Validator.TryValidateObject(obj, context, results, true);
        }
    }
}
