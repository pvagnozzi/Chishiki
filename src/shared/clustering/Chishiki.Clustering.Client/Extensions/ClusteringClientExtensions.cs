// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Clustering.Client.Services;
using Chishiki.Hub.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Chishiki.Clustering.Client.Extensions;

/// <summary>
/// Extension methods for registering the Chishiki Orleans clustering client and the
/// grain-backed <see cref="IHubResourceService"/> caching decorator.
/// </summary>
public static class ClusteringClientExtensions
{
    /// <summary>
    /// Adds the Orleans clustering client targeting the Chishiki Redis cluster and
    /// registers an <see cref="IHubResourceService"/> backed by Orleans grain caching.
    /// </summary>
    /// <param name="builder">The host application builder to configure.</param>
    /// <param name="innerServiceFactory">
    /// Factory that produces the fallback <see cref="IHubResourceService"/> used on cache misses.
    /// </param>
    /// <returns>The same <paramref name="builder"/> to support call chaining.</returns>
    public static IHostApplicationBuilder AddChishikiClusteringClient(
        this IHostApplicationBuilder builder,
        Func<IServiceProvider, IHubResourceService> innerServiceFactory)
    {
        var redisConnection = builder.Configuration.GetConnectionString("redis") ?? "redis:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnection);
        redisOptions.AbortOnConnectFail = false;

        builder.UseOrleansClient(client =>
        {
            client.Configure<Orleans.Configuration.ClusterOptions>(options =>
            {
                options.ClusterId = "chishiki-cluster";
                options.ServiceId = "chishiki";
            });

            client.UseRedisClustering(options =>
                options.ConfigurationOptions = redisOptions);
        });

        builder.Services.AddSingleton<IHubResourceService>(sp =>
            new OrleansHubResourceService(
                sp.GetRequiredService<IGrainFactory>(),
                innerServiceFactory(sp),
                sp.GetRequiredService<ILogger<OrleansHubResourceService>>()));

        return builder;
    }
}
