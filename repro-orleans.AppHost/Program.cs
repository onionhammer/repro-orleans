using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    //.RunAsEmulator(); // https://github.com/dotnet/aspire/issues/4646
    .RunAsEmulator(c => c.WithImageTag("3.30.0"));

var clustering = storage.AddTables("clustering");
var grainStorage = storage.AddBlobs("grainstate");

var orleans = builder.AddOrleans("orleans")
    .WithClustering(clustering)
    .WithGrainStorage("Default", grainStorage);

var api = builder
    .AddProject<repro_orleans_API>("api")
    .WithReference(orleans);

builder.Build().Run();
