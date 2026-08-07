using System.Text.Json.Serialization;
using DiffingApi.Api;
using DiffingApi.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IDiffEntryStore, InMemoryDiffEntryStore>();

var app = builder.Build();

app.MapHealthChecks("/health");
DiffEndpoints.Map(app);

app.Run();