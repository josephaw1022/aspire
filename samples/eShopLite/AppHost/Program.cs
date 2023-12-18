var builder = DistributedApplication.CreateBuilder(args);

var catalogDb = builder.AddPostgresContainer("postgres")
                       .WithNamedVolume("VolumeMount.example.data");

// Add another Postgres container with the same named volume to see if named volumes name validation works correctly
builder.AddPostgresContainer("randomdb")
                      .WithNamedVolume("VolumeMount.example.data");

var basketCache = builder.AddRedisContainer("basketcache")
                         .WithNamedVolume();

var catalogService = builder.AddProject<Projects.CatalogService>("catalogservice")
                     .WithReference(catalogDb)
                     .WithReplicas(2);

var messaging = builder.AddRabbitMQContainer("messaging")
                       .WithNamedVolume();

var basketService = builder.AddProject("basketservice", @"..\BasketService\BasketService.csproj")
                    .WithReference(basketCache)
                    .WithReference(messaging);

builder.AddProject<Projects.MyFrontend>("frontend")
       .WithReference(basketService)
       .WithReference(catalogService.GetEndpoint("http"));

builder.AddProject<Projects.OrderProcessor>("orderprocessor")
       .WithReference(messaging)
       .WithLaunchProfile("OrderProcessor");

builder.AddProject<Projects.ApiGateway>("apigateway")
       .WithReference(basketService)
       .WithReference(catalogService);

builder.AddProject<Projects.CatalogDb>("catalogdbapp")
       .WithReference(catalogDb);

builder.Build().Run();
