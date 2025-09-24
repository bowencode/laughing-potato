using k8s.Models;

var builder = DistributedApplication.CreateBuilder(args);

var identity = builder.AddProject<Projects.IdentityServer>("identity")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.IdentityDemos_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(identity)
    .WaitFor(identity);

builder.AddProject<Projects.IdentityDemos_WebClient>("webclient")
    .WithHttpHealthCheck("/health")
    .WithReference(identity)
    .WithReference(apiService)
    .WaitFor(identity)
    .WaitFor(apiService);

builder.Build().Run();
