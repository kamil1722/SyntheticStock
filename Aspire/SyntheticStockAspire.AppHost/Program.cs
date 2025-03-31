var builder = DistributedApplication.CreateBuilder(args);

// Ресурсы
var postgres = builder.AddPostgres("postgres").WithDataBindMount("PostgreSQLData");
var rabbitMq = builder.AddRabbitMQ("rabbitmq");

// Проекты
var webApi = builder.AddProject<Projects.WebApi>("webapi");
var dataWorkService = builder.AddProject<Projects.DataWorkService>("dataworkservice");

// Связи
webApi.WithReference(postgres)
      .WithReference(rabbitMq);

dataWorkService.WithReference(postgres)
               .WithReference(rabbitMq);

builder.Build().Run();

builder.Build().Run();