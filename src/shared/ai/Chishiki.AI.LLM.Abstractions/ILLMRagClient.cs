// -----------------------------------------------------------------------------
// File:        ILLMRagClient.cs
// Author:      Piergiorgio Vagnozzi
// Description: Defines the contract for RAG (Retrieval-Augmented Generation) operations: ingest, search, and grounded ask.
// Created:     2026-05-10
// Modified:    2026-05-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.AI.LLM.Abstractions;

/// <summary>Defines the contract for RAG (Retrieval-Augmented Generation) operations: ingest, search, and grounded ask.</summary>
public interface ILLMRagClient
{
    /// <summary>Ingests a single document into the specified collection in the vector store.</summary>
    /// <param name="collectionName">The target collection name.</param>
    /// <param name="document">The document to embed and store.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task IngestAsync(
        string collectionName,
        LLMRagDocument document,
        CancellationToken cancellationToken = default);

    /// <summary>Ingests a batch of documents into the specified collection.</summary>
    /// <param name="collectionName">The target collection name.</param>
    /// <param name="documents">The documents to embed and store.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task IngestBatchAsync(
        string collectionName,
        IReadOnlyList<LLMRagDocument> documents,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a document from the specified collection by its identifier.</summary>
    /// <param name="collectionName">The target collection name.</param>
    /// <param name="documentId">The unique identifier of the document to remove.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    Task DeleteAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>Performs a semantic similarity search against the specified collection.</summary>
    /// <param name="collectionName">The collection to search.</param>
    /// <param name="query">The natural-language query to embed and search.</param>
    /// <param name="options">Optional search parameters. When <c>null</c> defaults apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>An ordered list of <see cref="LLMRagSearchResult"/> ranked by relevance score descending.</returns>
    Task<IReadOnlyList<LLMRagSearchResult>> SearchAsync(
        string collectionName,
        string query,
        LLMRagSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves relevant context from the vector store and asks the LLM to produce a grounded answer.</summary>
    /// <param name="collectionName">The collection to retrieve context from.</param>
    /// <param name="question">The natural-language question to answer.</param>
    /// <param name="searchOptions">Optional search parameters for context retrieval.</param>
    /// <param name="chatOptions">Optional inference parameters for the answer generation step.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A <see cref="LLMRagAskResult"/> containing the grounded answer and source citations.</returns>
    Task<LLMRagAskResult> AskAsync(
        string collectionName,
        string question,
        LLMRagSearchOptions? searchOptions = null,
        LLMChatOptions? chatOptions = null,
        CancellationToken cancellationToken = default);
}
