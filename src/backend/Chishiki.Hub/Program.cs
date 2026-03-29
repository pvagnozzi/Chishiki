// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Clustering.Client.Extensions;
using Chishiki.Hub.Infrastructure.FileSystem;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var hubRoot = builder.Configuration["HUB_ROOT"]
    ?? Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../.."));

builder.AddChishikiClusteringClient(sp =>
    new FileSystemHubResourceService(
        hubRoot,
        sp.GetRequiredService<ILogger<FileSystemHubResourceService>>()));

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapMcp("/mcp");

app.Run();
