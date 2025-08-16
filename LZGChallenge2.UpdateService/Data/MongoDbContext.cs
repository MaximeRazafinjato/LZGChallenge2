using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using LZGChallenge2.UpdateService.Models;

namespace LZGChallenge2.UpdateService.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<Player> Players => _database.GetCollection<Player>("players");
}

public static class MongoDbExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, string connectionString, string databaseName)
    {
        services.AddSingleton<IMongoClient>(sp =>
        {
            return new MongoClient(connectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        services.AddSingleton<MongoDbContext>();

        return services;
    }
}