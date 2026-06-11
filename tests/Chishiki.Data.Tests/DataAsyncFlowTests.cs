// -----------------------------------------------------------------------------
// File:        DataAsyncFlowTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers data seeding flows, repository extension helpers, and async query paging behavior.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.Linq.Expressions;
using Chishiki.Data;
using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;
using Chishiki.Data.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Data.Tests;

/// <summary>Provides focused tests for async repository helpers, seeding flows, and async query paging.</summary>
[TestFixture]
public sealed class DataAsyncFlowTests
{
    [Test]
    public async Task RepositoryListAsyncWrapsExpressionInSpecification()
    {
        var repository = Substitute.For<IReadOnlyRepository<int, TestEntity>>();
        IList<TestEntity> expected = [new() { Id = 1, Name = "Ada" }];
        ISpecification<int, TestEntity>? captured = null;
        Expression<Func<TestEntity, bool>> filter = entity => entity.Name == "Ada";

        _ = repository.ListAsync(Arg.Do<ISpecification<int, TestEntity>>(spec => captured = spec), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await repository.ListAsync(filter);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.Where!.Compile()(expected[0]), Is.True);
        });
    }

    [Test]
    public async Task UnitOfWorkListAsyncUsesReadOnlyRepositoryAndDisposesIt()
    {
        var repository = Substitute.For<IReadOnlyRepository<int, TestEntity>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        IList<TestEntity> expected = [new() { Id = 2, Name = "Grace" }];
        Expression<Func<TestEntity, bool>> filter = entity => entity.Id == 2;

        _ = unitOfWork.GetReadOnlyRepository<int, TestEntity>().Returns(repository);
        _ = repository.ListAsync(Arg.Any<ISpecification<int, TestEntity>>(), Arg.Any<CancellationToken>()).Returns(expected);

        var result = await unitOfWork.ListAsync<int, TestEntity>(filter);

        Assert.That(result, Is.SameAs(expected));
        repository.Received(1).Dispose();
    }

    [Test]
    public async Task RepositoryPagedListAsyncWrapsExpressionInPagedSpecification()
    {
        var repository = Substitute.For<IReadOnlyRepository<int, TestEntity>>();
        IPagedSpecification<int, TestEntity>? captured = null;
        IPagedList<TestEntity> expected = new PagedList<TestEntity>([], 0, 2, 5);
        Expression<Func<TestEntity, bool>> filter = entity => entity.Score > 10;

        _ = repository.ListPagedAsync(Arg.Do<IPagedSpecification<int, TestEntity>>(spec => captured = spec), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await repository.ListAsync(filter, pageIndex: 2, pageSize: 5);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(expected));
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured!.PageIndex, Is.EqualTo(2));
            Assert.That(captured.PageSize, Is.EqualTo(5));
            Assert.That(captured.Where, Is.Not.Null);
        });
    }

    [Test]
    public async Task InsertAndDeleteRepositoryExtensionHelpersCallExpectedRepositoryMethods()
    {
        var repository = Substitute.For<IRepository<int, TestEntity>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Ada" },
            new TestEntity { Id = 2, Name = "Grace" },
        };
        _ = unitOfWork.GetRepository<int, TestEntity>().Returns(repository);

        var callbackEntity = default(TestEntity);
        await unitOfWork.InsertMasterDetailAsync<int, TestEntity>(entities[0], (entity, _) =>
        {
            callbackEntity = entity;
            return Task.CompletedTask;
        });
        await unitOfWork.InsertAsync<int, TestEntity>(entities);
        await unitOfWork.DeleteAsync<int, TestEntity>(entities);

        await repository.Received(1).AddAsync(entities[0], Arg.Any<CancellationToken>());
        await repository.Received(1).AddRangeAsync(entities, Arg.Any<CancellationToken>());
        await repository.Received(1).DeleteRangeAsync(entities, Arg.Any<CancellationToken>());
        Assert.That(callbackEntity, Is.SameAs(entities[0]));
    }

    [Test]
    public async Task ToPagedListAsyncAppliesDefaultOrderingByEntityId()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Entities.AddRange(
            new TestEntity { Id = 3, Name = "Three" },
            new TestEntity { Id = 1, Name = "One" },
            new TestEntity { Id = 2, Name = "Two" });
        _ = await dbContext.SaveChangesAsync();

        var result = await dbContext.Entities.ToPagedListAsync<int, TestEntity>(pageSize: 2, pageNumber: 0);

        Assert.That(result.Items.Select(entity => entity.Id), Is.EqualTo([1, 2]));
    }

    [Test]
    public async Task ToPagedListWithSelectAsyncProjectsPagedItemsWithIndex()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Entities.AddRange(
            new TestEntity { Id = 10, Name = "Ten" },
            new TestEntity { Id = 20, Name = "Twenty" },
            new TestEntity { Id = 30, Name = "Thirty" });
        _ = await dbContext.SaveChangesAsync();

        var result = await dbContext.Entities
            .OrderBy(entity => entity.Id)
            .ToPagedListWithSelectAsync<int, TestEntity, string>((entity, index) => $"{index}:{entity.Name}", pageSize: 2, pageNumber: 0);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.EqualTo(["0:Ten", "1:Twenty"]));
            Assert.That(result.TotalCount, Is.EqualTo(3));
            Assert.That(result.TotalPages, Is.EqualTo(2));
        });
    }

    [Test]
    public Task DataSeederSeedDataAsyncSwallowsSeedEntitiesException()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<DataSeeder>>();
        var sut = new ThrowingDataSeeder(unitOfWork, logger);

        Assert.DoesNotThrowAsync(async () => await sut.SeedDataAsync());
        return Task.CompletedTask;
    }

    [Test]
    public async Task DataSeederSeedEntityAsyncAddsEntitiesAndSavesOnce()
    {
        var repository = Substitute.For<IRepository<int, SeedEntity>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<DataSeeder>>();
        _ = unitOfWork.GetRepository<int, SeedEntity>().Returns(repository);
        var sut = new TestDataSeeder(unitOfWork, logger);
        var entities = new[]
        {
            new SeedEntity { Id = 1, Name = "Ada" },
            new SeedEntity { Id = 2, Name = "Grace" },
        };

        await sut.SeedEntitiesDirectAsync(entities);

        await repository.Received(1).AddAsync(entities[0], Arg.Any<CancellationToken>());
        await repository.Received(1).AddAsync(entities[1], Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task DataSeederSeedEntityAsyncWithFinderReturnsExistingAndInsertedEntities()
    {
        var repository = Substitute.For<IRepository<int, SeedEntity>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<DataSeeder>>();
        _ = unitOfWork.GetRepository<int, SeedEntity>().Returns(repository);
        _ = repository.ListAsync(null, Arg.Any<CancellationToken>()).Returns(
            [new() { Id = 1, Name = "Ada" }]);
        var sut = new TestDataSeeder(unitOfWork, logger);
        var entities = new[]
        {
            new SeedEntity { Id = 1, Name = "Ada" },
            new SeedEntity { Id = 2, Name = "Grace" },
        };

        var result = await sut.SeedEntitiesDirectAsync(entities, (current, incoming) => current.Name == incoming.Name);

        Assert.Multiple(() =>
        {
            Assert.That(result.Select(entity => entity.Id), Is.EqualTo([1, 2]));
            Assert.That(result[0].Name, Is.EqualTo("Ada"));
            Assert.That(result[1].Name, Is.EqualTo("Grace"));
        });
        await repository.Received(1).AddAsync(Arg.Is<SeedEntity>(entity => entity.Id == 2), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }

    public sealed class TestEntity : IEntity<int>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int Score { get; init; }
    }

    public sealed class SeedEntity : IEntityWithDates<int>
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public DateTimeOffset CreatedOn { get; init; }
        public DateTimeOffset UpdatedOn { get; init; }
    }

    private sealed class ThrowingDataSeeder(IUnitOfWork unitOfWork, ILogger<DataSeeder> logger) : DataSeeder(unitOfWork, logger)
    {
        protected override Task SeedEntitiesAsync(CancellationToken cancellationToken = default) => throw new InvalidOperationException("boom");
    }

    private sealed class TestDataSeeder(IUnitOfWork unitOfWork, ILogger<DataSeeder> logger) : DataSeeder(unitOfWork, logger)
    {
        protected override Task SeedEntitiesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SeedEntitiesDirectAsync(IEnumerable<SeedEntity> entities, CancellationToken cancellationToken = default) =>
            SeedEntityAsync<int, SeedEntity>(entities, cancellationToken);

        public Task<IList<SeedEntity>> SeedEntitiesDirectAsync(IEnumerable<SeedEntity> entities, Func<SeedEntity, SeedEntity, bool> finder, CancellationToken cancellationToken = default) =>
            SeedEntityAsync<int, SeedEntity>(entities, finder, cancellationToken);
    }
}
