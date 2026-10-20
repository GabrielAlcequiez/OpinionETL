using OpinionETL.Data;
using OpinionETL.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddDataServices(builder.Configuration);

var host = builder.Build();
host.Run();
