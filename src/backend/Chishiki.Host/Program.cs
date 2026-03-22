using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var redisConnectionString = builder.Configuration.GetConnectionString("redis") ?? "redis:6379";

var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
redisOptions.AbortOnConnectFail = false;

builder.UseOrleans(silo =>
{
    silo.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "chishiki-cluster";
        options.ServiceId = "chishiki";
    });

    silo.Configure<EndpointOptions>(options =>
    {
        options.SiloPort = 11111;
        options.GatewayPort = 30000;
    });

    // Clustering via Redis
    silo.UseRedisClustering(options =>
        options.ConfigurationOptions = redisOptions);

    // Grain state persistence via Redis (default + PubSubStore for streams)
    silo.AddRedisGrainStorageAsDefault(options =>
        options.ConfigurationOptions = redisOptions);

    silo.AddRedisGrainStorage("PubSubStore", options =>
        options.ConfigurationOptions = redisOptions);

    // Reminder service via Redis
    silo.UseRedisReminderService(options =>
        options.ConfigurationOptions = redisOptions);
});

var host = builder.Build();
await host.RunAsync();
