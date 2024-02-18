var builder = DistributedApplication.CreateBuilder(args);

var HelloWorldApp_Port = 8000;
var HelloWorldApp = builder.AddContainer("HelloWorldApp", "mgsair/hello-world-java", "0.0.1.RELEASE")
    .WithEnvironment("SERVER_PORT", $"{HelloWorldApp_Port}")
    .WithEndpoint(containerPort: HelloWorldApp_Port, scheme: "http", name: "endpoint");
    
var helloWorldAppEndpoint = HelloWorldApp.GetEndpoint("endpoint");

var apiService = builder.AddProject<Projects.AspireWithContainers_ApiService>("apiservice")
    .WithReference(helloWorldAppEndpoint);

builder.AddProject<Projects.AspireWithContainers_Web>("webfrontend")
    .WithReference(apiService);


builder.Build().Run();
