using OpinionETL.Core.Interfaces;
using OpinionETL.Core.Services;
using OpinionETL.Data;
using OpinionETL.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddScoped<IEtlPipeline, EtlPipeline>();

var host = builder.Build();
host.Run();
