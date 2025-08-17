using MongoDB.Driver;
using LZGChallenge2.Api.Models;

namespace LZGChallenge2.Api.Data;

public static class MongoDbIndexConfiguration
{
    public static async Task ConfigureIndexesAsync(MongoDbContext context)
    {
        await ConfigureUserIndexesAsync(context);
        await ConfigureRefreshTokenIndexesAsync(context);
        await ConfigureEmailVerificationTokenIndexesAsync(context);
        await ConfigurePasswordResetTokenIndexesAsync(context);
    }

    private static async Task ConfigureUserIndexesAsync(MongoDbContext context)
    {
        var indexKeysDefinition = Builders<User>.IndexKeys
            .Ascending(user => user.Email);

        var indexOptions = new CreateIndexOptions
        {
            Unique = true,
            Name = "email_unique_index"
        };

        var indexModel = new CreateIndexModel<User>(indexKeysDefinition, indexOptions);
        await context.Users.Indexes.CreateOneAsync(indexModel);

        // Index for active users
        var activeIndexKeys = Builders<User>.IndexKeys
            .Ascending(user => user.IsActive);
        var activeIndexModel = new CreateIndexModel<User>(activeIndexKeys, new CreateIndexOptions { Name = "active_users_index" });
        await context.Users.Indexes.CreateOneAsync(activeIndexModel);
    }

    private static async Task ConfigureRefreshTokenIndexesAsync(MongoDbContext context)
    {
        // Index on userId for faster lookup
        var userIdIndexKeys = Builders<RefreshToken>.IndexKeys
            .Ascending(token => token.UserId);
        var userIdIndexModel = new CreateIndexModel<RefreshToken>(userIdIndexKeys, new CreateIndexOptions { Name = "userId_index" });
        await context.RefreshTokens.Indexes.CreateOneAsync(userIdIndexModel);

        // Index on token for faster lookup
        var tokenIndexKeys = Builders<RefreshToken>.IndexKeys
            .Ascending(token => token.Token);
        var tokenIndexModel = new CreateIndexModel<RefreshToken>(tokenIndexKeys, new CreateIndexOptions { Name = "token_index" });
        await context.RefreshTokens.Indexes.CreateOneAsync(tokenIndexModel);

        // TTL index for automatic expiration
        var expirationIndexKeys = Builders<RefreshToken>.IndexKeys
            .Ascending(token => token.ExpiresAt);
        var expirationIndexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.Zero, // Documents expire at the specified date
            Name = "expiration_ttl_index"
        };
        var expirationIndexModel = new CreateIndexModel<RefreshToken>(expirationIndexKeys, expirationIndexOptions);
        await context.RefreshTokens.Indexes.CreateOneAsync(expirationIndexModel);
    }

    private static async Task ConfigureEmailVerificationTokenIndexesAsync(MongoDbContext context)
    {
        // Index on token for faster lookup
        var tokenIndexKeys = Builders<EmailVerificationToken>.IndexKeys
            .Ascending(token => token.Token);
        var tokenIndexModel = new CreateIndexModel<EmailVerificationToken>(tokenIndexKeys, new CreateIndexOptions { Name = "token_index" });
        await context.EmailVerificationTokens.Indexes.CreateOneAsync(tokenIndexModel);

        // Index on userId
        var userIdIndexKeys = Builders<EmailVerificationToken>.IndexKeys
            .Ascending(token => token.UserId);
        var userIdIndexModel = new CreateIndexModel<EmailVerificationToken>(userIdIndexKeys, new CreateIndexOptions { Name = "userId_index" });
        await context.EmailVerificationTokens.Indexes.CreateOneAsync(userIdIndexModel);

        // TTL index for automatic expiration
        var expirationIndexKeys = Builders<EmailVerificationToken>.IndexKeys
            .Ascending(token => token.ExpiresAt);
        var expirationIndexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.Zero,
            Name = "expiration_ttl_index"
        };
        var expirationIndexModel = new CreateIndexModel<EmailVerificationToken>(expirationIndexKeys, expirationIndexOptions);
        await context.EmailVerificationTokens.Indexes.CreateOneAsync(expirationIndexModel);
    }

    private static async Task ConfigurePasswordResetTokenIndexesAsync(MongoDbContext context)
    {
        // Index on token for faster lookup
        var tokenIndexKeys = Builders<PasswordResetToken>.IndexKeys
            .Ascending(token => token.Token);
        var tokenIndexModel = new CreateIndexModel<PasswordResetToken>(tokenIndexKeys, new CreateIndexOptions { Name = "token_index" });
        await context.PasswordResetTokens.Indexes.CreateOneAsync(tokenIndexModel);

        // Index on userId
        var userIdIndexKeys = Builders<PasswordResetToken>.IndexKeys
            .Ascending(token => token.UserId);
        var userIdIndexModel = new CreateIndexModel<PasswordResetToken>(userIdIndexKeys, new CreateIndexOptions { Name = "userId_index" });
        await context.PasswordResetTokens.Indexes.CreateOneAsync(userIdIndexModel);

        // TTL index for automatic expiration
        var expirationIndexKeys = Builders<PasswordResetToken>.IndexKeys
            .Ascending(token => token.ExpiresAt);
        var expirationIndexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.Zero,
            Name = "expiration_ttl_index"
        };
        var expirationIndexModel = new CreateIndexModel<PasswordResetToken>(expirationIndexKeys, expirationIndexOptions);
        await context.PasswordResetTokens.Indexes.CreateOneAsync(expirationIndexModel);
    }
}