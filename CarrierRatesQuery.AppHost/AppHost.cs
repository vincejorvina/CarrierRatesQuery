var builder = DistributedApplication.CreateBuilder(args);

var fedEx = builder.AddProject<Projects.CarrierRatesQuery_MockFedEx>("mockfedex");

builder.AddProject<Projects.CarrierRatesQuery_Api>("api")
    .WithReference(fedEx)
    .WaitFor(fedEx);

builder.Build().Run();
