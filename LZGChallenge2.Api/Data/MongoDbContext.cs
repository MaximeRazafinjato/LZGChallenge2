using MongoDB.Driver;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<Player> Players => _database.GetCollection<Player>("players");
    public IMongoCollection<Match> Matches => _database.GetCollection<Match>("matches");
    public IMongoCollection<PlayerStats> PlayerStats => _database.GetCollection<PlayerStats>("playerStats");
    public IMongoCollection<ChampionStats> ChampionStats => _database.GetCollection<ChampionStats>("championStats");
    public IMongoCollection<RoleStats> RoleStats => _database.GetCollection<RoleStats>("roleStats");
    
    // Authentication collections
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("refreshTokens");
    public IMongoCollection<EmailVerificationToken> EmailVerificationTokens => _database.GetCollection<EmailVerificationToken>("emailVerificationTokens");
    public IMongoCollection<PasswordResetToken> PasswordResetTokens => _database.GetCollection<PasswordResetToken>("passwordResetTokens");
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