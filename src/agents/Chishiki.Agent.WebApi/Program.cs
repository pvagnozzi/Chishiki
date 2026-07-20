// -----------------------------------------------------------------------------
// File:        Program.cs
// Author:      Piergiorgio Vagnozzi
// Description: ASP.NET Core entry point for the Chishiki Agent Web API.
// Created:     2026-06-28
// Modified:    2026-06-28
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Agent.Core.Extensions;
using Chishiki.Agent.WebApi.Endpoints;
using Chishiki.Agent.WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddChishikiAgent(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ── App ───────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapOpenApi();
app.MapHub<ChatHub>("/ws/chat");
app.MapAgentEndpoints();

app.Run();
