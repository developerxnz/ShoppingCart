using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Shopping.Core;
using Shopping.Product;
using Shopping.Product.Handlers;
using Shopping.Product.Persistence;
using Shopping.Product.Services;
using Product = Shopping.Product.Services.Product;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOptions<CosmosDbConfiguration>()
    .BindConfiguration("CosmosDbSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();


builder.Services.AddTransient<ICommandHandler, ProductCommandHandler>();
builder.Services.AddTransient<IRepository<Shopping.Product.Persistence.Product>, Repository>();
builder.Services.AddTransient<IProduct, Product>();
builder.Services
    .AddTransient<ITransformer<ProductAggregate, Shopping.Product.Persistence.Product>, ProductTransformer>();
builder.Services.AddSingleton(x =>
{
    CosmosClientOptions clientOptions = new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Gateway,
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };
    var configuration = x.GetRequiredService<IOptions<CosmosDbConfiguration>>().Value;
    CosmosClient cosmosClient = new CosmosClient(configuration.ConnectionString, clientOptions);

    return cosmosClient;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class CosmosDbConfiguration
{
    [Required] public string ConnectionString { get; set; }
}