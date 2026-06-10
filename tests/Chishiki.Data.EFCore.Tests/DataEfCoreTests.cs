// -----------------------------------------------------------------------------
// File:        DataEfCoreTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers focused tests for the Chishiki.Data.EFCore public configuration and persistence helpers.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using Chishiki.Data.Abstractions;
using Chishiki.Data.Audit;
using Chishiki.Data.EFCore;
using Chishiki.Data.EFCore.Audit;
using Chishiki.Data.EFCore.Models;
using Chishiki.Data.Models;
using Chishiki.Data.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Chishiki.Data.EFCore.Tests;

/// <summary>Provides focused tests for EF Core configuration helpers and public persistence behavior.</summary>
[TestFixture]
public sealed class DataEfCoreTests
{
    [Test]
    public void GetRequiredConnectionStringReturnsConfiguredValue()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Main"] = "Host=localhost;Database=test;"
            })
            .Build();

        var result = configuration.GetRequiredConnectionString("Main");

        Assert.That(result, Is.EqualTo("Host=localhost;Database=test;"));
    }

    [Test]
    public void GetRequiredConnectionStringWhenMissingThrowsInvalidDataException()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        Assert.That(() => configuration.GetRequiredConnectionString("Missing"), Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public async Task SeedTableAsyncAddsEntitiesOnlyWhenTableIsEmpty()
    {
        await using var context = CreateDbContext();

        await context.SeedTableAsync(context.RepositoryEntities,
        [
            new RepositoryEntity { Id = 1, Name = "Ada" },
            new RepositoryEntity { Id = 2, Name = "Grace" }
        ]);
        await context.SeedTableAsync(context.RepositoryEntities,
        [
            new RepositoryEntity { Id = 3, Name = "Ignored" }
        ]);

        var result = await context.RepositoryEntities.OrderBy(x => x.Id).ToListAsync();

        Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task GetChangesReturnsInsertedEntityDetails()
    {
        await using var context = CreateDbContext();
        context.RepositoryEntities.Add(new RepositoryEntity { Id = 1, Name = "Ada" });

        var result = context.GetChanges().Single();

        Assert.That(result.Operation, Is.EqualTo(ChangeAction.Inserted));
        Assert.That(result.EntityId, Is.EqualTo("1"));
        Assert.That(result.Properties.Any(property => property.PropertyName == nameof(RepositoryEntity.Name) && Equals(property.NewValue, "Ada")), Is.True);
    }

    [Test]
    public async Task GetChangesReturnsUpdatedAndDeletedEntityDetails()
    {
        await using var context = CreateDbContext();
        context.RepositoryEntities.Add(new RepositoryEntity { Id = 1, Name = "Ada" });
        _ = await context.SaveChangesAsync();

        var existing = await context.RepositoryEntities.SingleAsync();
        existing.Name = "Grace";

        var updated = context.GetChanges().Single();
        Assert.That(updated.Operation, Is.EqualTo(ChangeAction.Updated));
        Assert.That(updated.Properties.Any(property => property.PropertyName == nameof(RepositoryEntity.Name)), Is.True);

        context.Remove(existing);

        var deleted = context.GetChanges().Single(change => change.Operation == ChangeAction.Deleted);
        Assert.That(deleted.EntityId, Is.EqualTo("1"));
    }

    [Test]
    public void SetGuidBaseEntityConfiguresPrimaryKeyAndRequiredAuditProperties()
    {
        using var context = CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(AuditedEntity));

        Assert.That(entityType, Is.Not.Null);
        Assert.That(entityType!.FindPrimaryKey()!.Properties.Select(x => x.Name), Is.EqualTo(new[] { nameof(EFBaseEntity<Guid>.Id) }));
        Assert.That(entityType.FindProperty(nameof(EFBaseEntity<Guid>.Id))!.GetMaxLength(), Is.EqualTo(64));
        Assert.That(entityType.FindProperty(nameof(EFBaseEntity<Guid>.CreatedOn))!.IsNullable, Is.False);
        Assert.That(entityType.FindProperty(nameof(EFBaseEntity<Guid>.UpdatedOn))!.IsNullable, Is.False);
    }

    [Test]
    public async Task AddEFUnitOfWorkFactoryRegistersScopedUnitOfWorkThatPersistsEntities()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddDbContextFactory<TestDbContext>(options => options.UseInMemoryDatabase(databaseName));
        _ = services.AddEFUnitOfWorkFactory<TestDbContext>();

        await using (var provider = services.BuildServiceProvider())
        {
            await using (var scope = provider.CreateAsyncScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var repository = unitOfWork.GetRepository<int, RepositoryEntity>();
                await repository.AddAsync(new RepositoryEntity { Id = 10, Name = "Persisted" });
                await unitOfWork.SaveChangesAsync();
            }

            await using var verificationScope = provider.CreateAsyncScope();
            var verificationUnitOfWork = verificationScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var verificationRepository = verificationUnitOfWork.GetReadOnlyRepository<int, RepositoryEntity>();
            var result = await verificationRepository.GetByIdAsync(10);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Persisted"));
        }
    }

    [Test]
    public async Task AddEFRepositoryMapperAllowsCustomRepositoryResolution()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        _ = services.AddLogging();
        _ = services.AddDbContextFactory<TestDbContext>(options => options.UseInMemoryDatabase(databaseName));
        _ = services.AddEFRepositoryMapper([typeof(CustomMappedRepository).Assembly]);
        _ = services.AddEFUnitOfWorkFactory<TestDbContext>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var repository = unitOfWork.GetRepository<int, MappedEntity>();

        Assert.That(repository.GetType(), Is.EqualTo(typeof(CustomMappedRepository)));
    }

    [Test]
    public void ToEntityAuditAndDeserializePropertyAuditRoundTripChangeData()
    {
        var change = new EntityChange(
            typeof(RepositoryEntity).FullName!,
            "15",
            ChangeAction.Updated,
            [new PropertyChange("Name", "Before", "After")]);

        var audit = change.ToEntityAudit<TestEntityAudit>(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var properties = audit.Properties.DeserializePropertyAudit();

        Assert.That(audit.EntityName, Is.EqualTo(typeof(RepositoryEntity).FullName));
        Assert.That(audit.EntityId, Is.EqualTo("15"));
        Assert.That(audit.Action, Is.EqualTo(ChangeAction.Updated));
        Assert.That(properties, Has.Length.EqualTo(1));
        Assert.That(properties[0].PropertyName, Is.EqualTo("Name"));
        Assert.That(properties[0].OldValue, Is.EqualTo("Before"));
        Assert.That(properties[0].NewValue, Is.EqualTo("After"));
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
        public DbSet<RepositoryEntity> RepositoryEntities => Set<RepositoryEntity>();

        public DbSet<MappedEntity> MappedEntities => Set<MappedEntity>();

        public DbSet<AuditedEntity> AuditedEntities => Set<AuditedEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RepositoryEntity>();
            modelBuilder.Entity<MappedEntity>();
            modelBuilder.Entity<AuditedEntity>(builder => builder.SetGuidBaseEntity());
        }
    }

    private sealed class RepositoryEntity : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class MappedEntity : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class AuditedEntity : EFBaseEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestEntityAudit : EntityAudit
    {
        public TestEntityAudit() : base(Guid.Empty, string.Empty, string.Empty, ChangeAction.Inserted, string.Empty)
        {
        }
    }

    private sealed class CustomMappedRepository(DbContext context, Microsoft.Extensions.Logging.ILogger logger) : IRepository<int, MappedEntity>
    {
        public IQueryable<MappedEntity> AsQuery() => context.Set<MappedEntity>().AsQueryable();

        public Task<MappedEntity?> GetByIdAsync(int key, CancellationToken cancellationToken = default) => context.Set<MappedEntity>().FindAsync([key], cancellationToken).AsTask();

        public Task<MappedEntity?> FirstOrDefaultAsync(ISpecification<int, MappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<MappedEntity?> SingleOrDefaultAsync(ISpecification<int, MappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IList<MappedEntity>> ListAsync(ISpecification<int, MappedEntity>? specification = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IPagedList<MappedEntity>> ListPagedAsync(IPagedSpecification<int, MappedEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public MappedEntity? GetById(int key) => context.Set<MappedEntity>().Find(key);

        public MappedEntity? FirstOrDefault(ISpecification<int, MappedEntity> specification) => throw new NotImplementedException();

        public MappedEntity? SingleOrDefault(ISpecification<int, MappedEntity> specification) => throw new NotImplementedException();

        public IList<MappedEntity> List(ISpecification<int, MappedEntity>? specification = null) => throw new NotImplementedException();

        public IPagedList<MappedEntity> ListPaged(IPagedSpecification<int, MappedEntity> specification) => throw new NotImplementedException();

        public Task AddAsync(MappedEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateAsync(MappedEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteAsync(MappedEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task AddRangeAsync(IEnumerable<MappedEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateRangeAsync(IEnumerable<MappedEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteRangeAsync(IEnumerable<MappedEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteRangeByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public void Add(MappedEntity entity) => throw new NotImplementedException();

        public void Update(MappedEntity entity) => throw new NotImplementedException();

        public void Delete(MappedEntity entity) => throw new NotImplementedException();

        public void DeleteById(int id) => throw new NotImplementedException();

        public void AddRange(IEnumerable<MappedEntity> entities) => throw new NotImplementedException();

        public void UpdateRange(IEnumerable<MappedEntity> entities) => throw new NotImplementedException();

        public void DeleteRange(IEnumerable<MappedEntity> entities) => throw new NotImplementedException();

        public void DeleteRangeById(IEnumerable<int> ids) => throw new NotImplementedException();

        public void Dispose()
        {
            GC.KeepAlive(logger);
        }
    }
}
