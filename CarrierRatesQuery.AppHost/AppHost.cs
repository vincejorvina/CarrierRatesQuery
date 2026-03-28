var builder = DistributedApplication.CreateBuilder(args);

var lbc = builder.AddProject<Projects.CarrierRatesQuery_MockLbc>("mocklbc");
var dhl = builder.AddProject<Projects.CarrierRatesQuery_MockDhl>("mockdhl");
var fedEx = builder.AddProject<Projects.CarrierRatesQuery_MockFedEx>("mockfedex");
var ups = builder.AddProject<Projects.CarrierRatesQuery_MockUps>("mockups");

builder.AddProject<Projects.CarrierRatesQuery_Api>("api")
    .WithReference(lbc)
    .WithReference(dhl)
    .WithReference(fedEx)
    .WithReference(ups)
    .WaitFor(lbc)
    .WaitFor(dhl)
    .WaitFor(fedEx)
    .WaitFor(ups);

builder.Build().Run();