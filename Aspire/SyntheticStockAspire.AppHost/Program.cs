using SyntheticStockAspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// Параметры окружения
EnvironmentSetup.SetRabbitMQEnvironment();

// Проекты
var webApi = builder.AddProject<Projects.WebApi>("webapi");
var dataWorkService = builder.AddProject<Projects.DataWorkService>("dataworkservice");


builder.Build().Run();