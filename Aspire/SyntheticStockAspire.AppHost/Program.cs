using SyntheticStockAspire.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

// ��������� ���������
EnvironmentSetup.SetRabbitMQEnvironment();
EnvironmentSetup.SetBinanceEnvironment();

// �������
builder.AddProject<Projects.WebApi>("webapi");
builder.AddProject<Projects.DataWorkService>("dataworkservice");


builder.Build().Run();