using IdempotentApiDemo.API.Common.Filters;
using IdempotentApiDemo.API.Common.Settings;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Idempotent API Demo",
            Version = "v1",
            Description = "An API demonstrating idempotency with Redis caching.",
            Contact = new OpenApiContact
            {
                Name = "Furkan SARIKAYA",
                Url = new Uri("https://github.com/furkansarikaya")
            },
        };
        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = "string" },
            Description = "Unique key to ensure idempotency of the request."
        });
        return Task.CompletedTask;
    });
});

// Servis eklemesi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Idempotent API Demo",
        Version = "v1",
        Description = "An API demonstrating idempotency with Redis caching.",
        Contact = new OpenApiContact
        {
            Name = "Furkan SARIKAYA",
            Url = new Uri("https://github.com/furkansarikaya")
        }
    });

    // Tüm endpoint'lere Idempotency-Key header'ı ekle
    options.OperationFilter<IdempotencyKeyHeaderOperationFilter>();
});

builder.Services.AddControllers();

var redisSettings = builder.Configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>()!;
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = $"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password}";
    options.InstanceName = redisSettings.InstanceName;
});
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisSettings.Host}:{redisSettings.Port},password={redisSettings.Password}"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
await app.RunAsync();