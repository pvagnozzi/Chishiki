// -----------------------------------------------------------------------------
// File:        MessagingRequestContractsTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers constructor and property behavior for shared messaging request contracts.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Models;
using Chishiki.Data.Specifications;
using Chishiki.Messaging.Abstractions.Commands;
using Chishiki.Messaging.Abstractions.Queries;
using Chishiki.Messaging.Common;
using Chishiki.Messaging.Common.Commands;
using Chishiki.Messaging.Common.Queries;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Messaging.Tests;

/// <summary>Provides coverage for simple messaging contracts, generic base records, and publisher plumbing.</summary>
public sealed class MessagingRequestContractsTests
{
    [Test]
    public void CommandRequests_PreserveCorrelationIdentifierIdAndModel()
    {
        var correlationId = Guid.NewGuid();
        var model = new SampleModel(7, "Ada");

        var command = new TestCommand(correlationId);
        var idCommand = new TestIdCommand(42, correlationId);
        var modelCommand = new TestModelCommand(model, correlationId);
        var createCommand = new TestCreateCommand(model, correlationId);
        var updateCommand = new TestUpdateCommand(42, model, correlationId);
        var deleteCommand = new TestDeleteCommand(42, correlationId);

        Assert.Multiple(() =>
        {
            Assert.That(command.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(idCommand.Id, Is.EqualTo(42));
            Assert.That(idCommand.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(modelCommand.Model, Is.SameAs(model));
            Assert.That(modelCommand.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(createCommand.Model, Is.SameAs(model));
            Assert.That(createCommand.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(updateCommand.Id, Is.EqualTo(42));
            Assert.That(updateCommand.Model, Is.SameAs(model));
            Assert.That(deleteCommand.Id, Is.EqualTo(42));
            Assert.That(deleteCommand.CorrelationId, Is.EqualTo(correlationId));
        });
    }

    [Test]
    public void QueryRequests_PreserveCorrelationIdentifierAndSpecializedArguments()
    {
        var correlationId = Guid.NewGuid();
        var specification = new TestSpecification();

        var query = new TestQuery(correlationId);
        var byIdQuery = new TestGetByIdQuery(13, correlationId);
        var firstOrDefaultQuery = new TestGetFirstOrDefaultQuery(specification, correlationId);
        var listQuery = new TestListQuery(correlationId);
        var pagedListQuery = new TestPagedListQuery(correlationId);

        Assert.Multiple(() =>
        {
            Assert.That(query.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(byIdQuery.Id, Is.EqualTo(13));
            Assert.That(byIdQuery.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(firstOrDefaultQuery.Specification, Is.SameAs(specification));
            Assert.That(firstOrDefaultQuery.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(listQuery.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(pagedListQuery.CorrelationId, Is.EqualTo(correlationId));
        });
    }

    [Test]
    public void RequestModelBase_PreservesInitializedState()
    {
        var createdOn = DateTimeOffset.Parse("2026-06-10T08:00:00+00:00");
        var updatedOn = createdOn.AddMinutes(5);

        var model = new RequestModelBase<int>
        {
            Id = 99,
            CreatedOn = createdOn,
            UpdatedOn = updatedOn,
        };

        Assert.Multiple(() =>
        {
            Assert.That(model.Id, Is.EqualTo(99));
            Assert.That(model.CreatedOn, Is.EqualTo(createdOn));
            Assert.That(model.UpdatedOn, Is.EqualTo(updatedOn));
        });
    }

    [Test]
    public async Task RequestPublisher_StoresLoggerAndDispatchesOverriddenMembers()
    {
        var logger = Substitute.For<ILogger>();
        var publisher = new TestRequestPublisher(logger);
        var query = new TestStringQuery(Guid.NewGuid());
        var command = new TestCommand(Guid.NewGuid());

        var result = await publisher.SendQueryAsync<TestStringQuery, string>(query);
        await publisher.SendCommandAsync(command);

        Assert.Multiple(() =>
        {
            Assert.That(publisher.CapturedLogger, Is.SameAs(logger));
            Assert.That(result, Is.EqualTo("query-result"));
            Assert.That(publisher.LastCommand, Is.SameAs(command));
            Assert.That(publisher.LastQuery, Is.SameAs(query));
        });
    }

    private sealed record SampleModel(int Id, string Name);

    private sealed class TestEntity : IEntity<int>
    {
        public int Id { get; init; }
    }

    private sealed class TestSpecification : ISpecification<int, TestEntity>
    {
        public System.Linq.Expressions.Expression<Func<TestEntity, bool>>? Where => null;

        public IIncludeExpression[] Includes => [];

        public IFilterConditionExpression[] Filters => [];

        public ISortExpression[] Sort => [];
    }

    private sealed record TestCommand(Guid CorrelationId) : CommandRequest(CorrelationId);

    private sealed record TestIdCommand(int Id, Guid CorrelationId) : IdCommandRequest<int>(Id, CorrelationId);

    private sealed record TestModelCommand(SampleModel Model, Guid CorrelationId) : ModelCommandRequest<int, SampleModel>(Model, CorrelationId);

    private sealed record TestCreateCommand(SampleModel Model, Guid CorrelationId) : CreateCommandRequest<int, SampleModel>(Model, CorrelationId);

    private sealed record TestDeleteCommand(int Id, Guid CorrelationId) : DeleteByIdCommandRequest<int, TestEntity>(Id, CorrelationId);

    private sealed record TestUpdateCommand(int Id, SampleModel Model, Guid CorrelationId) : UpdateByIdCommandRequest<int, SampleModel>(Id, Model, CorrelationId);

    private sealed record TestQuery(Guid CorrelationId) : QueryRequest<TestEntity>(CorrelationId);

    private sealed record TestStringQuery(Guid CorrelationId) : QueryRequest<string>(CorrelationId), IQueryRequest<string>;

    private sealed record TestGetByIdQuery(int Id, Guid CorrelationId) : GetByIdQueryRequest<int, TestEntity>(Id, CorrelationId);

    private sealed record TestGetFirstOrDefaultQuery(ISpecification<int, TestEntity> Specification, Guid CorrelationId)
        : GetFirstOrDefaultQueryRequest<int, TestEntity>(Specification, CorrelationId);

    private sealed record TestListQuery(Guid CorrelationId) : ListQueryRequest<SampleModel>(CorrelationId);

    private sealed record TestPagedListQuery(Guid CorrelationId) : PagedListQueryRequest<SampleModel>(CorrelationId);

    private sealed class TestRequestPublisher(ILogger logger) : RequestPublisher(logger)
    {
        public ILogger CapturedLogger => Logger;

        public ICommandRequest? LastCommand { get; private set; }

        public IQueryRequest<string>? LastQuery { get; private set; }

        public override Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        {
            LastQuery = query as IQueryRequest<string>;
            return Task.FromResult((TResult)(object)"query-result");
        }

        public override Task SendCommandAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        {
            LastCommand = command;
            return Task.CompletedTask;
        }
    }
}
