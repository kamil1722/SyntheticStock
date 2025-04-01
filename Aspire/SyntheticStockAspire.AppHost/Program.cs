using SyntheticStockAspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Параметры окружения
EnvironmentSetup.SetRabbitMQEnvironment();
EnvironmentSetup.SetBinanceEnvironment();
EnvironmentSetup.SetPostgreSQLEnvironment();

// Проекты
builder.AddProject<Projects.WebApi>("webapi");
builder.AddProject<Projects.DataWorkService>("dataworkservice");


builder.Build().Run();