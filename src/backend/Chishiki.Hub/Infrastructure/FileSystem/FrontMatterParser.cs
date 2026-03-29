// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Chishiki.Hub.Infrastructure.FileSystem;

/// <summary>
/// Parses YAML front-matter blocks from Markdown and YAML resource files.
/// </summary>
internal static class FrontMatterParser
{
    /// <summary>
    /// Parses the YAML front-matter from <paramref name="fileContent"/> and returns the extracted
    /// metadata, tag list, and the remaining body content.
    /// </summary>
    /// <param name="fileContent">Raw text content of the resource file.</param>
    /// <returns>
    /// A tuple of: a case-insensitive metadata dictionary, a read-only list of tags,
    /// and the body content that follows the front-matter delimiter.
    /// </returns>
    public static (IReadOnlyDictionary<string, string> Meta, IReadOnlyList<string> Tags, string Content) Parse(
        string fileContent)
    {
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var tags = new List<string>();

        if (!fileContent.StartsWith("---", StringComparison.Ordinal))
            return (meta, tags, fileContent);

        var endIndex = fileContent.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
            return (meta, tags, fileContent);

        var frontMatter = fileContent[3..endIndex].Trim();
        var content = fileContent[(endIndex + 4)..].TrimStart('\n', '\r');

        var inTagsList = false;
        foreach (var rawLine in frontMatter.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            if (inTagsList)
            {
                if (line.TrimStart().StartsWith("- ", StringComparison.Ordinal))
                {
                    tags.Add(line.TrimStart()[2..].Trim().Trim('"', '\''));
                    continue;
                }

                inTagsList = false;
            }

            var colonIdx = line.IndexOf(':');
            if (colonIdx <= 0) continue;

            var key = line[..colonIdx].Trim();
            var value = line[(colonIdx + 1)..].Trim();

            if (string.IsNullOrEmpty(key)) continue;

            if (key.Equals("tags", StringComparison.OrdinalIgnoreCase))
            {
                if (value.StartsWith("[", StringComparison.Ordinal))
                {
                    foreach (var item in value.Trim('[', ']').Split(','))
                    {
                        var tag = item.Trim().Trim('"', '\'');
                        if (!string.IsNullOrEmpty(tag)) tags.Add(tag);
                    }
                }
                else if (string.IsNullOrEmpty(value))
                {
                    inTagsList = true;
                }
            }
            else if (!string.IsNullOrEmpty(value))
            {
                meta[key] = value.Trim('"', '\'');
            }
        }

        return (meta, tags, content);
    }
}
