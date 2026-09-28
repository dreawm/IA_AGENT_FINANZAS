using TutorPreClase.Infrastructure;
using TutorPreClase.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AgregarInfraestructura(builder.Configuration);
builder.Services.AddHostedService<ReprocesadorPendientes>();

var host = builder.Build();
host.Run();
