// -----------------------------------------------------------------------------
// File:        DataEfCoreInfrastructureTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers EF Core repository internals, base-entity helpers, and design-time factory behavior.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Data.Abstractions;
using Chishiki.Data.EFCore.Models;
using Chishiki.Data.Models;
using Chishiki.Data.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Data.EFCore.Tests;

/// <summary>Provides focused tests for EF Core infrastructure internals and public model helpers.</summary>
[TestFixture]
public sealed class DataEfCoreInfrastructureTests
{
    [Test]
    public async Task InternalEfCoreRepositorySupportsAsyncCrudOperations()
    {
        await using var context = CreateDbContext();
        var repository = CreateMutableRepository(context);

        await repository.AddAsync(new RepositoryEntity { Id = 1, Name = "Ada" });
        await repository.AddRangeAsync([new RepositoryEntity { Id = 2, Name = "Grace" }]);
        _ = await context.SaveChangesAsync();

        var first = await repository.GetByIdAsync(1);
        Assert.That(first, Is.Not.Null);

        context.ChangeTracker.Clear();
        await repository.UpdateAsync(new RepositoryEntity { Id = 1, Name = "Updated Ada" });
        await repository.DeleteByIdAsync(2);
        _ = await context.SaveChangesAsync();

        var remaining = await repository.ListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(first!.Name, Is.EqualTo("Ada"));
            Assert.That(remaining.Select(entity => entity.Id), Is.EqualTo([1]));
            Assert.That(remaining[0].Name, Is.EqualTo("Updated Ada"));
        });
    }

    [Test]
    public async Task InternalEfCoreRepositorySupportsSyncCrudAndDeleteRangeByIdOperations()
    {
        await using var context = CreateDbContext();
        var repository = CreateMutableRepository(context);

        repository.Add(new RepositoryEntity { Id = 10, Name = "One" });
        repository.AddRange(
        [
            new RepositoryEntity { Id = 11, Name = "Two" },
            new RepositoryEntity { Id = 12, Name = "Three" }
        ]);
        _ = await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        repository.Update(new RepositoryEntity { Id = 10, Name = "Updated" });
        repository.DeleteRangeById([11, 12]);
        _ = await context.SaveChangesAsync();

        var entity = repository.GetById(10);
        var items = repository.List();

        Assert.Multiple(() =>
        {
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity!.Name, Is.EqualTo("Updated"));
            Assert.That(items.Select(item => item.Id), Is.EqualTo([10]));
        });
    }

    [Test]
    public async Task InternalEfCoreReadOnlyRepositoryAppliesSpecificationsAndPaging()
    {
        await using var context = CreateDbContext();
        context.RepositoryEntities.AddRange(
            new RepositoryEntity { Id = 1, Name = "Charlie" },
            new RepositoryEntity { Id = 2, Name = "Ada" },
            new RepositoryEntity { Id = 3, Name = "Grace" });
        _ = await context.SaveChangesAsync();

        var repository = CreateReadOnlyRepository(context);
        var specification = new Specification<int, RepositoryEntity>(
            entity => entity.Id >= 2,
            sortExpressions: [new SortExpression(nameof(RepositoryEntity.Name))]);
        var pagedSpecification = new PagedSpecification<int, RepositoryEntity>(
            entity => entity.Id >= 1,
            sortExpressions: [new SortExpression(nameof(RepositoryEntity.Name))],
            pageIndex: 0,
            pageSize: 2);

        var first = await repository.FirstOrDefaultAsync(specification);
        var single = repository.SingleOrDefault(new Specification<int, RepositoryEntity>(entity => entity.Id == 2));
        var list = repository.List(specification);
        var paged = await repository.ListPagedAsync(pagedSpecification);
        repository.Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Name, Is.EqualTo("Ada"));
            Assert.That(single, Is.Not.Null);
            Assert.That(single!.Id, Is.EqualTo(2));
            Assert.That(list.Select(entity => entity.Name), Is.EqualTo(["Ada", "Grace"]));
            Assert.That(paged.Items.Select(entity => entity.Name), Is.EqualTo(["Ada", "Charlie"]));
        });
    }

    [Test]
    public void InternalEfCoreRepositoryFactoryReturnsMappedRepositoryOrFallsBackWhenActivationFails()
    {
        using var context = CreateDbContext();
        var loggerFactory = CreateLoggerFactory();

        var successfulMapper = new RepositoryMapper();
        _ = successfulMapper.Register(typeof(FactoryMappedRepository));
        var successfulFactory = CreateRepositoryFactory(successfulMapper, loggerFactory, context);
        var successfulRepository = successfulFactory.CreateRepository<int, FactoryMappedEntity>();

        var failingMapper = new RepositoryMapper();
        _ = failingMapper.Register(typeof(BadFactoryMappedRepository));
        var failingFactory = CreateRepositoryFactory(failingMapper, loggerFactory, context);
        var fallbackRepository = failingFactory.CreateRepository<int, BadFactoryMappedEntity>();

        Assert.Multiple(() =>
        {
            Assert.That(successfulRepository.GetType(), Is.EqualTo(typeof(FactoryMappedRepository)));
            Assert.That(fallbackRepository.GetType().Name, Is.EqualTo("EFCoreRepository`2"));
        });
    }

    [Test]
    public async Task InternalEfCoreUnitOfWorkCreatesRepositoriesAndPersistsChanges()
    {
        await using var context = CreateDbContext();
        var loggerFactory = CreateLoggerFactory();
        var repositoryMapper = new RepositoryMapper();
        var unitOfWork = CreateUnitOfWork(context, repositoryMapper, loggerFactory);

        var repository = unitOfWork.GetRepository<int, RepositoryEntity>();
        await repository.AddAsync(new RepositoryEntity { Id = 21, Name = "Persisted" });
        await unitOfWork.SaveChangesAsync();

        var verification = unitOfWork.GetReadOnlyRepository<int, RepositoryEntity>();
        var entity = await verification.GetByIdAsync(21);
        unitOfWork.Dispose();

        Assert.That(entity, Is.Not.Null);
        Assert.That(entity!.Name, Is.EqualTo("Persisted"));
    }

    [Test]
    public void EfBaseEntityHelpersConfigureModelAndSetFluentValues()
    {
        var modelBuilder = new ModelBuilder();
        _ = modelBuilder.Configure<Guid, AuditedEntity>("audited_entities", builder => builder.Property(entity => entity.Name).HasMaxLength(32));
        var entityType = modelBuilder.Model.FindEntityType(typeof(AuditedEntity));

        var createdOn = DateTimeOffset.Parse("2026-06-10T12:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture);
        var updatedOn = createdOn.AddMinutes(5);
        var entity = new AuditedEntity()
            .WithId(Guid.Parse("11111111-1111-1111-1111-111111111111"))
            .WithCreatedAt(createdOn)
            .WithUpdatedAt(updatedOn);

        var sameId = new AuditedEntity().WithId(entity.Id);
        var guidEntity = new EFGuidEntity();
        var stringEntity = new EFStringEntity();

        Assert.Multiple(() =>
        {
            Assert.That(entityType, Is.Not.Null);
            Assert.That(entityType!.GetTableName(), Is.EqualTo("audited_entities"));
            Assert.That(entityType!.FindProperty(nameof(AuditedEntity.Name))!.GetMaxLength(), Is.EqualTo(32));
            Assert.That(entity.CreatedAt, Is.EqualTo(createdOn));
            Assert.That(entity.UpdatedAt, Is.EqualTo(updatedOn));
            Assert.That(entity.Equals(sameId), Is.True);
            Assert.That(entity.GetHashCode(), Is.EqualTo(entity.Id.GetHashCode()));
            Assert.That(guidEntity.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(stringEntity.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(stringEntity.Id.Length, Is.GreaterThan(0));
        });
    }

    [Test]
    public void DesignTimeDbContextFactoryUsesDefaultOrExplicitConnectionString()
    {
        var factoryWithDefault = new TestDesignTimeDbContextFactory(string.Empty, "DefaultConnection");
        var defaultContext = factoryWithDefault.CreateDbContext([]);

        var factoryWithExplicit = new TestDesignTimeDbContextFactory("ExplicitConnection", "DefaultConnection");
        var explicitContext = factoryWithExplicit.CreateDbContext([]);

        Assert.Multiple(() =>
        {
            Assert.That(factoryWithDefault.UsedConnectionString, Is.EqualTo("DefaultConnection"));
            Assert.That(factoryWithExplicit.UsedConnectionString, Is.EqualTo("ExplicitConnection"));
            Assert.That(defaultContext, Is.Not.Null);
            Assert.That(explicitContext, Is.Not.Null);
        });
    }

    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        _ = loggerFactory.CreateLogger(Arg.Any<string>()).Returns(NullLogger.Instance);
        return loggerFactory;
    }

    private static IReadOnlyRepository<int, RepositoryEntity> CreateReadOnlyRepository(TestDbContext context)
    {
        var type = typeof(ConfigurationExtensions).Assembly
            .GetType("Chishiki.Data.EFCore.EFCoreReadOnlyRepository`2", throwOnError: true)!
            .MakeGenericType(typeof(int), typeof(RepositoryEntity));

        return (IReadOnlyRepository<int, RepositoryEntity>)Activator.CreateInstance(type, context, NullLogger.Instance)!;
    }

    private static IRepository<int, RepositoryEntity> CreateMutableRepository(TestDbContext context)
    {
        var type = typeof(ConfigurationExtensions).Assembly
            .GetType("Chishiki.Data.EFCore.EFCoreRepository`2", throwOnError: true)!
            .MakeGenericType(typeof(int), typeof(RepositoryEntity));

        return (IRepository<int, RepositoryEntity>)Activator.CreateInstance(type, context, NullLogger.Instance)!;
    }

    private static IRepositoryFactory CreateRepositoryFactory(IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory, DbContext context)
    {
        var type = typeof(ConfigurationExtensions).Assembly
            .GetType("Chishiki.Data.EFCore.EfCoreRepositoryFactory", throwOnError: true)!;

        return (IRepositoryFactory)Activator.CreateInstance(type, repositoryMapper, loggerFactory, context)!;
    }

    private static IUnitOfWork CreateUnitOfWork(DbContext context, IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory)
    {
        var type = typeof(ConfigurationExtensions).Assembly
            .GetType("Chishiki.Data.EFCore.EFCoreUnitOfWork", throwOnError: true)!;

        return (IUnitOfWork)Activator.CreateInstance(type, context, repositoryMapper, loggerFactory)!;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<RepositoryEntity> RepositoryEntities => Set<RepositoryEntity>();
        public DbSet<FactoryMappedEntity> FactoryMappedEntities => Set<FactoryMappedEntity>();
        public DbSet<BadFactoryMappedEntity> BadFactoryMappedEntities => Set<BadFactoryMappedEntity>();
        public DbSet<AuditedEntity> AuditedEntities => Set<AuditedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<RepositoryEntity>();
            _ = modelBuilder.Entity<FactoryMappedEntity>();
            _ = modelBuilder.Entity<BadFactoryMappedEntity>();
            _ = modelBuilder.Entity<AuditedEntity>(builder => builder.SetGuidBaseEntity());
        }
    }

    public sealed class RepositoryEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class FactoryMappedEntity : IEntity<int>
    {
        public int Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class BadFactoryMappedEntity : IEntity<int>
    {
        public int Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    private sealed class FactoryMappedRepository(DbContext context, ILogger logger) : IRepository<int, FactoryMappedEntity>
    {
        public IQueryable<FactoryMappedEntity> AsQuery() => context.Set<FactoryMappedEntity>().AsQueryable();
        public Task<FactoryMappedEntity?> GetByIdAsync(int key, CancellationToken cancellationToken = default) => context.Set<FactoryMappedEntity>().FindAsync([key], cancellationToken).AsTask();
        public Task<FactoryMappedEntity?> FirstOrDefaultAsync(ISpecification<int, FactoryMappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<FactoryMappedEntity?> SingleOrDefaultAsync(ISpecification<int, FactoryMappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IList<FactoryMappedEntity>> ListAsync(ISpecification<int, FactoryMappedEntity>? specification = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IPagedList<FactoryMappedEntity>> ListPagedAsync(IPagedSpecification<int, FactoryMappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public FactoryMappedEntity? GetById(int key) => context.Set<FactoryMappedEntity>().Find(key);
        public FactoryMappedEntity? FirstOrDefault(ISpecification<int, FactoryMappedEntity> specification) => throw new NotImplementedException();
        public FactoryMappedEntity? SingleOrDefault(ISpecification<int, FactoryMappedEntity> specification) => throw new NotImplementedException();
        public IList<FactoryMappedEntity> List(ISpecification<int, FactoryMappedEntity>? specification = null) => throw new NotImplementedException();
        public IPagedList<FactoryMappedEntity> ListPaged(IPagedSpecification<int, FactoryMappedEntity> specification) => throw new NotImplementedException();
        public Task AddAsync(FactoryMappedEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(FactoryMappedEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(FactoryMappedEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<FactoryMappedEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateRangeAsync(IEnumerable<FactoryMappedEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteRangeAsync(IEnumerable<FactoryMappedEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteRangeByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Add(FactoryMappedEntity entity) { }
        public void Update(FactoryMappedEntity entity) { }
        public void Delete(FactoryMappedEntity entity) { }
        public void DeleteById(int id) { }
        public void AddRange(IEnumerable<FactoryMappedEntity> entities) { }
        public void UpdateRange(IEnumerable<FactoryMappedEntity> entities) { }
        public void DeleteRange(IEnumerable<FactoryMappedEntity> entities) { }
        public void DeleteRangeById(IEnumerable<int> ids) { }
        public void Dispose() => GC.KeepAlive(logger);
    }

    private sealed class BadFactoryMappedRepository : IRepository<int, BadFactoryMappedEntity>
    {
        public IQueryable<BadFactoryMappedEntity> AsQuery() => throw new NotImplementedException();
        public Task<BadFactoryMappedEntity?> GetByIdAsync(int key, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<BadFactoryMappedEntity?> FirstOrDefaultAsync(ISpecification<int, BadFactoryMappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<BadFactoryMappedEntity?> SingleOrDefaultAsync(ISpecification<int, BadFactoryMappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IList<BadFactoryMappedEntity>> ListAsync(ISpecification<int, BadFactoryMappedEntity>? specification = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IPagedList<BadFactoryMappedEntity>> ListPagedAsync(IPagedSpecification<int, BadFactoryMappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public BadFactoryMappedEntity? GetById(int key) => throw new NotImplementedException();
        public BadFactoryMappedEntity? FirstOrDefault(ISpecification<int, BadFactoryMappedEntity> specification) => throw new NotImplementedException();
        public BadFactoryMappedEntity? SingleOrDefault(ISpecification<int, BadFactoryMappedEntity> specification) => throw new NotImplementedException();
        public IList<BadFactoryMappedEntity> List(ISpecification<int, BadFactoryMappedEntity>? specification = null) => throw new NotImplementedException();
        public IPagedList<BadFactoryMappedEntity> ListPaged(IPagedSpecification<int, BadFactoryMappedEntity> specification) => throw new NotImplementedException();
        public Task AddAsync(BadFactoryMappedEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(BadFactoryMappedEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(BadFactoryMappedEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task AddRangeAsync(IEnumerable<BadFactoryMappedEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateRangeAsync(IEnumerable<BadFactoryMappedEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteRangeAsync(IEnumerable<BadFactoryMappedEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteRangeByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void Add(BadFactoryMappedEntity entity) => throw new NotImplementedException();
        public void Update(BadFactoryMappedEntity entity) => throw new NotImplementedException();
        public void Delete(BadFactoryMappedEntity entity) => throw new NotImplementedException();
        public void DeleteById(int id) => throw new NotImplementedException();
        public void AddRange(IEnumerable<BadFactoryMappedEntity> entities) => throw new NotImplementedException();
        public void UpdateRange(IEnumerable<BadFactoryMappedEntity> entities) => throw new NotImplementedException();
        public void DeleteRange(IEnumerable<BadFactoryMappedEntity> entities) => throw new NotImplementedException();
        public void DeleteRangeById(IEnumerable<int> ids) => throw new NotImplementedException();
        public void Dispose() { }
    }

    private sealed class AuditedEntity : EFBaseEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDesignTimeDbContextFactory(string connectionString, string defaultConnectionString)
        : DesignTimeDbContextFactory<TestDbContext>(connectionString, defaultConnectionString)
    {
        public string? UsedConnectionString { get; private set; }

        protected override DbContextOptionsBuilder<TestDbContext> BuildDbContextOptionsBuilder(DbContextOptionsBuilder<TestDbContext> builder, string connectionString)
        {
            UsedConnectionString = connectionString;
            return builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        protected override TestDbContext CreateDbContext(DbContextOptions<TestDbContext> options) => new(options);
    }
}
