using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SongTheSame.Application;
using SongTheSame.Infrastructure;
using SongTheSame.Ingest;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System.Net.Http", LogLevel.Warning);

builder.Services.AddApplication().AddInfrastructure(builder.Configuration);
builder.Services.AddIngestCommands();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

return await scope.ServiceProvider.GetRequiredService<CommandRouter>().RunAsync(args);
