// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Hub.Contracts;
using Chishiki.Hub.Contracts.Requests;

namespace Chishiki.Hub.Infrastructure.FileSystem;

/// <summary>
/// File-system backed implementation of <see cref="IHubResourceService"/> that reads and writes
/// hub resources from the repository's well-known directory layout.
/// </summary>
internal sealed partial class FileSystemHubResourceService : IHubResourceService
{
    private static readonly IReadOnlyDictionary<HubResourceType, string> ResourcePaths =
        new Dictionary<HubResourceType, string>
        {
            [HubResourceType.Template]    = "hub/templates",
            [HubResourceType.Agent]       = ".github/agents",
            [HubResourceType.Hook]        = ".github",
            [HubResourceType.Instruction] = ".github/instructions",
            [HubResourceType.Skill]       = ".github/skills",
            [HubResourceType.Collection]  = "hub/collections",
        };

    private static readonly IReadOnlyDictionary<HubResourceType, string[]> ResourcePatterns =
        new Dictionary<HubResourceType, string[]>
        {
            [HubResourceType.Template]    = ["*.md", "*.json"],
            [HubResourceType.Agent]       = ["*.chatmode.md"],
            [HubResourceType.Hook]        = ["hooks.json"],
            [HubResourceType.Instruction] = ["*.instructions.md"],
            [HubResourceType.Skill]       = ["SKILL.md"],
            [HubResourceType.Collection]  = ["*.yaml", "*.yml"],
        };

    private readonly string _repoRoot;
    private readonly ILogger<FileSystemHubResourceService> _logger;

    /// <summary>Initializes a new instance of <see cref="FileSystemHubResourceService"/>.</summary>
    /// <param name="repoRoot">Absolute path to the repository root directory.</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    public FileSystemHubResourceService(string repoRoot, ILogger<FileSystemHubResourceService> logger)
    {
        _repoRoot = repoRoot;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HubResourceDto>> ListResourcesAsync(
        HubResourceType type, string? tag = null, CancellationToken cancellationToken = default)
    {
        var dir = ResolveDirectory(type);
        if (!Directory.Exists(dir))
        {
            LogResourceDirectoryNotFound(dir);
            return [];
        }

        var resources = new List<HubResourceDto>();
        foreach (var file in EnumerateFiles(type, dir))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dto = await ReadDtoAsync(file, type, cancellationToken);
            if (dto is not null &&
                (tag is null || dto.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
            {
                resources.Add(dto);
            }
        }

        return resources;
    }

    /// <inheritdoc/>
    public async Task<HubResourceDto?> GetResourceAsync(
        HubResourceType type, string name, CancellationToken cancellationToken = default)
    {
        var dir = ResolveDirectory(type);
        foreach (var file in EnumerateFiles(type, dir))
        {
            if (string.Equals(ResourceName(file, type), name, StringComparison.OrdinalIgnoreCase))
                return await ReadDtoAsync(file, type, cancellationToken);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<HubResourceDto> CreateResourceAsync(
        CreateResourceRequest request, CancellationToken cancellationToken = default)
    {
        var dir = ResolveDirectory(request.Type);
        Directory.CreateDirectory(dir);

        string filePath;
        if (request.Type == HubResourceType.Skill)
        {
            var skillDir = Path.Combine(dir, request.Name);
            Directory.CreateDirectory(skillDir);
            filePath = Path.Combine(skillDir, "SKILL.md");
        }
        else
        {
            var ext = request.Type switch
            {
                HubResourceType.Agent       => ".chatmode.md",
                HubResourceType.Instruction => ".instructions.md",
                HubResourceType.Collection  => ".yaml",
                _                           => ".md",
            };
            filePath = Path.Combine(dir, request.Name + ext);
        }
        var tagList  = string.Join(", ", request.Tags.Select(t => $"\"{t}\""));

        var content = $"""
            ---
            name: {request.Name}
            description: {request.Description}
            tags: [{tagList}]
            version: "1.0.0"
            ---

            # {request.Name}

            {request.Description}
            """;

        await File.WriteAllTextAsync(filePath, content, cancellationToken);

        LogCreatedResource(request.Name, request.Type, filePath);

        return new HubResourceDto(
            request.Name,
            request.Description,
            request.Type,
            request.Tags,
            "1.0.0",
            content,
            DateTimeOffset.UtcNow);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HubResourceDto>> ApplyCollectionAsync(
        string collectionName, CancellationToken cancellationToken = default)
    {
        var collection = await GetResourceAsync(HubResourceType.Collection, collectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        var results = new List<HubResourceDto>();

        foreach (var line in collection.Content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("- ", StringComparison.Ordinal)) continue;

            var parts = trimmed[2..].Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;

            if (!Enum.TryParse<HubResourceType>(parts[0], ignoreCase: true, out var type)) continue;

            var dto = await GetResourceAsync(type, parts[1], cancellationToken);
            if (dto is not null) results.Add(dto);
        }

        return results;
    }

    /// <inheritdoc/>
    public Task<string> ScaffoldProjectAsync(
        ScaffoldProjectRequest request, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException("Project scaffolding is planned for Phase 2.");

    /// <inheritdoc/>
    public async Task<IReadOnlyList<HubResourceDto>> SearchResourcesAsync(
        SearchResourcesRequest request, CancellationToken cancellationToken = default)
    {
        var types = request.Type.HasValue
            ? [request.Type.Value]
            : Enum.GetValues<HubResourceType>();

        var results = new List<HubResourceDto>();
        foreach (var type in types)
        {
            var resources = await ListResourcesAsync(type, cancellationToken: cancellationToken);
            results.AddRange(resources.Where(r =>
                r.Name.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                r.Description.Contains(request.Query, StringComparison.OrdinalIgnoreCase) ||
                r.Tags.Any(t => t.Contains(request.Query, StringComparison.OrdinalIgnoreCase))));
        }

        if (request.Tags is { Count: > 0 })
        {
            results = results
                .Where(r => request.Tags.All(t => r.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
                .ToList();
        }

        return results;
    }

    /// <summary>Returns the absolute directory path for resources of the given <paramref name="type"/>.</summary>
    /// <param name="type">Hub resource type to resolve.</param>
    /// <returns>Absolute directory path.</returns>
    private string ResolveDirectory(HubResourceType type) =>
        Path.Combine(_repoRoot, ResourcePaths[type].Replace('/', Path.DirectorySeparatorChar));

    /// <summary>Enumerates file paths matching the resource patterns for <paramref name="type"/> inside <paramref name="dir"/>.</summary>
    /// <param name="type">Resource type whose file patterns are applied.</param>
    /// <param name="dir">Directory to search.</param>
    /// <returns>Sequence of matching file paths.</returns>
    private static IEnumerable<string> EnumerateFiles(HubResourceType type, string dir)
    {
        if (!Directory.Exists(dir)) return [];

        if (type == HubResourceType.Skill)
        {
            return Directory.GetDirectories(dir)
                .Select(d => Path.Combine(d, "SKILL.md"))
                .Where(File.Exists);
        }

        return ResourcePatterns[type]
            .SelectMany(p => Directory.GetFiles(dir, p, SearchOption.TopDirectoryOnly));
    }

    /// <summary>Derives the logical resource name from a file path.</summary>
    /// <param name="filePath">Absolute path to the resource file.</param>
    /// <param name="type">Resource type that determines the naming convention.</param>
    /// <returns>Logical resource name without type-specific extensions.</returns>
    private static string ResourceName(string filePath, HubResourceType type)
    {
        if (type == HubResourceType.Skill)
            return Path.GetFileName(Path.GetDirectoryName(filePath)!) ?? string.Empty;

        return Path.GetFileNameWithoutExtension(filePath)
            .Replace(".chatmode", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(".instructions", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Reads and parses a single resource file into a <see cref="HubResourceDto"/>.</summary>
    /// <param name="filePath">Absolute path to the file to read.</param>
    /// <param name="type">Resource type for name resolution fallback.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>Parsed DTO, or <see langword="null"/> if the file cannot be read or parsed.</returns>
    private async Task<HubResourceDto?> ReadDtoAsync(
        string filePath, HubResourceType type, CancellationToken cancellationToken)
    {
        try
        {
            var raw = await File.ReadAllTextAsync(filePath, cancellationToken);
            var (meta, tags, content) = FrontMatterParser.Parse(raw);
            var lastModified = new DateTimeOffset(File.GetLastWriteTimeUtc(filePath), TimeSpan.Zero);

            var name = meta.TryGetValue("name", out var n) && !string.IsNullOrWhiteSpace(n)
                ? n
                : ResourceName(filePath, type);

            var description = meta.TryGetValue("description", out var d) ? d : string.Empty;
            var version     = meta.TryGetValue("version", out var v)     ? v : "1.0.0";

            return new HubResourceDto(name, description, type, tags, version, content, lastModified);
        }
        catch (Exception ex)
        {
            LogFailedToReadResource(ex, filePath);
            return null;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Resource directory not found: {Directory}")]
    private partial void LogResourceDirectoryNotFound(string directory);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to read resource at {FilePath}")]
    private partial void LogFailedToReadResource(Exception ex, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created resource {Name} ({ResourceType}) at {Path}")]
    private partial void LogCreatedResource(string name, HubResourceType resourceType, string path);
}
