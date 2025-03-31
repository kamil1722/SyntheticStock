var builder = DistributedApplication.CreateBuilder(args);

// �������
var postgres = builder.AddPostgres("postgres").WithDataBindMount("PostgreSQLData");
var rabbitMq = builder.AddRabbitMQ("rabbitmq");

// �������
var webApi = builder.AddProject<Projects.WebApi>("webapi");
var dataWorkService = builder.AddProject<Projects.DataWorkService>("dataworkservice");

// �����
webApi.WithReference(postgres)
      .WithReference(rabbitMq);

dataWorkService.WithReference(postgres)
               .WithReference(rabbitMq);

builder.Build().Run();

builder.Build().Run();