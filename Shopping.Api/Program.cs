using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json.Serialization;
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
//builder.Services.Configure<ICosmosDbConfiguration>(builder.Configuration.GetSection("CosmosDbSettings"));

builder.Services.AddTransient<ICommandHandler, ProductCommandHandler>();
builder.Services.AddTransient<IRepository<Shopping.Product.Persistence.Product>, Repository>();
builder.Services.AddTransient<IProduct, Product>();
builder.Services.AddTransient<ITransformer<ProductAggregate, Shopping.Product.Persistence.Product>, Transformer>();
builder.Services.TryAddSingleton(async x =>
{
    CosmosClientOptions clientOptions = new CosmosClientOptions()
    {
        ConnectionMode = ConnectionMode.Gateway,
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    CosmosDbConfiguration cosmosDbConfiguration = new();
    builder.Configuration
        .GetSection("CosmosDbSettings")
        .Bind(cosmosDbConfiguration);

    CosmosClient cosmosClient = new CosmosClient(cosmosDbConfiguration.ConnectionString, clientOptions);
    var db = cosmosClient.GetDatabase("Shopping");

    ContainerProperties properties = new ContainerProperties();
    properties.PartitionKeyPaths = new[] { "/partnerId", "/accountId", "/profileId"};
    properties.Id = "Profiles";
    
    await db.CreateContainerIfNotExistsAsync(properties);

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
    public string ConnectionString { get; set; }
}