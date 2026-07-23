// -----------------------------------------------------------------------------
// File:        LLMChatMessageTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers the highest-value abstractions factory and default option behaviors.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.AI.LLM.Abstractions;
using NUnit.Framework;

namespace Chishiki.AI.LLM.Abstractions.Tests;

/// <summary>Provides focused tests for LLM abstraction factory methods and defaults.</summary>
public sealed class LLMChatMessageTests
{
    [TestCase("System", LLMChatRole.System)]
    [TestCase("User", LLMChatRole.User)]
    [TestCase("Assistant", LLMChatRole.Assistant)]
    public void ChatMessageFactoryMethods_CreateMessagesWithExpectedRoles(string content, LLMChatRole expectedRole)
    {
        var message = expectedRole switch
        {
            LLMChatRole.System => LLMChatMessage.System(content),
            LLMChatRole.User => LLMChatMessage.User(content),
            LLMChatRole.Assistant => LLMChatMessage.Assistant(content),
            _ => throw new ArgumentOutOfRangeException(nameof(expectedRole))
        };

        Assert.Multiple(() =>
        {
            Assert.That(message.Role, Is.EqualTo(expectedRole));
            Assert.That(message.Content, Is.EqualTo(content));
        });
    }

    [Test]
    public void RagSearchOptions_DefaultToTopKFiveAndZeroMinimumRelevance()
    {
        var options = new LLMRagSearchOptions();

        Assert.Multiple(() =>
        {
            Assert.That(options.TopK, Is.EqualTo(5));
            Assert.That(options.MinRelevanceScore, Is.EqualTo(0d));
            Assert.That(options.Filters, Is.Null);
        });
    }

    [Test]
    public void StreamingChatChunk_DefaultsIsFinalToFalse()
    {
        var chunk = new LLMStreamingChatChunk("partial", ModelId: "demo-model");

        Assert.Multiple(() =>
        {
            Assert.That(chunk.Content, Is.EqualTo("partial"));
            Assert.That(chunk.ModelId, Is.EqualTo("demo-model"));
            Assert.That(chunk.IsFinal, Is.False);
        });
    }
}
