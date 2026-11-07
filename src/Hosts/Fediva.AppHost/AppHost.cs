var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.Fediva_WebHost>("webhost");
builder.Build().Run();
