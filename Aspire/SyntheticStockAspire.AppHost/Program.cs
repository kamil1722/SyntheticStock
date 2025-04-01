using SyntheticStockAspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// ��������� ���������
EnvironmentSetup.SetRabbitMQEnvironment();

// �������
var webApi = builder.AddProject<Projects.WebApi>("webapi");
var dataWorkService = builder.AddProject<Projects.DataWorkService>("dataworkservice");


builder.Build().Run();