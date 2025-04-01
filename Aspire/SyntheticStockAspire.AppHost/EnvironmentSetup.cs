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

        //public static void SetRabbitMQEnvironment()
        //{
        //    Environment.SetEnvironmentVariable("RABBITMQ_HOST", "localhost");
        //    Environment.SetEnvironmentVariable("RABBITMQ_PORT", "5672");
        //    Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", "guest");
        //    Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", "guest");
        //    Environment.SetEnvironmentVariable("RABBITMQ_VIRTUALHOST", "/");
        //}
    }
}
