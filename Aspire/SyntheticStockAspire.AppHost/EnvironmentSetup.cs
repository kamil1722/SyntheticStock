using System.Security;

namespace SyntheticStockAspire.AppHost
{
    public static class EnvironmentSetup
    {
        public static void SetRabbitMQEnvironment()
        {
            Environment.SetEnvironmentVariable("RABBITMQ_HOST", "localhost");
            Environment.SetEnvironmentVariable("RABBITMQ_PORT", "5672");
            Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
            Environment.SetEnvironmentVariable("RABBITMQ_VIRTUALHOST", "/");
        }

        public static void SetBinanceEnvironment()
        {
            Environment.SetEnvironmentVariable("BINANCE_APIKEY", "YOUR_BINANCE_API_KEY");
            Environment.SetEnvironmentVariable("BINANCE_APISECRET", "YOUR_BINANCE_API_SECRET");

        }

        public static void SetPostgreSQLEnvironment()
        {
            Environment.SetEnvironmentVariable("POSTGRESQL_HOST", "localhost");
            Environment.SetEnvironmentVariable("POSTGRESQL_PORT", "5432");
            Environment.SetEnvironmentVariable("POSTGRESQL_DATABASE", "DB_SyntheticStock");
            Environment.SetEnvironmentVariable("POSTGRESQL_USERID", "postgres");
            Environment.SetEnvironmentVariable("POSTGRESQL_PASSWORD", "1722");
        }
    }
}
