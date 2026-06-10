// -----------------------------------------------------------------------------
// File:        DataInfrastructureTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers annotations, factories, unit of work defaults, and audit contracts in Chishiki.Data.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Chishiki.Data;
using Chishiki.Data.Abstractions;
using Chishiki.Data.Annotations;
using Chishiki.Data.Audit;
using Chishiki.Data.Models;
using Chishiki.Data.Query;
using Chishiki.Data.Specifications;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Data.Tests;

/// <summary>Provides focused tests for annotations, factory abstractions, and unit-of-work helpers.</summary>
[TestFixture]
public sealed class DataInfrastructureTests
{
    [Test]
    public void AnnotationExtensionsReadMetadataAndAccessPropertyValues()
    {
        var settings = typeof(AnnotatedDerivedEntity).GetTypePropertiesSettings();
        var idSettings = settings.Single(setting => setting.Name == nameof(AnnotatedBaseEntity.Id));
        var nameSettings = settings.Single(setting => setting.Name == nameof(AnnotatedDerivedEntity.Name));
        var activeSettings = settings.Single(setting => setting.Name == nameof(AnnotatedDerivedEntity.IsActive));
        var createdOnSettings = settings.Single(setting => setting.Name == nameof(AnnotatedDerivedEntity.CreatedOn));
        var statusSettings = settings.Single(setting => setting.Name == nameof(AnnotatedDerivedEntity.Status));
        var statusForFilterSettings = settings.Single(setting => setting.Name == nameof(AnnotatedDerivedEntity.StatusForFilter));
        var notesSettings = settings.Single(setting => setting.Name == nameof(AnnotatedDerivedEntity.Notes));

        var entity = new AnnotatedDerivedEntity
        {
            Id = 7,
            Name = "Ada",
            IsActive = true,
            CreatedOn = DateTimeOffset.Parse("2026-06-10T12:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            UpdatedOn = DateTimeOffset.Parse("2026-06-10T12:05:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
            Status = InfrastructureStatus.Active,
        };

        nameSettings.SetValue(entity, "Grace");

        Assert.Multiple(() =>
        {
            Assert.That(settings.Select(setting => setting.Name), Does.Contain(nameof(AnnotatedBaseEntity.Id)));
            Assert.That(idSettings.IsKey, Is.True);
            Assert.That(idSettings.DisplayName, Is.EqualTo("Identifier"));
            Assert.That(idSettings.Sortable, Is.True);
            Assert.That(idSettings.SortExpression, Is.EqualTo("IdSort"));
            Assert.That(idSettings.Filterable, Is.True);
            Assert.That(idSettings.FilterField, Is.EqualTo("IdFilter"));
            Assert.That(idSettings.FilterQueryOperator, Is.EqualTo(QueryOperator.Equal));
            Assert.That(idSettings.FilterQueryValueType, Is.EqualTo(QueryValueType.Integer));
            Assert.That(idSettings.DefaultValue, Is.EqualTo(99));
            Assert.That(nameSettings.Required, Is.True);
            Assert.That(nameSettings.GetValue(entity), Is.EqualTo("Grace"));
            Assert.That(nameSettings.GetDisplayValue(entity), Is.EqualTo("Grace"));
            Assert.That(activeSettings.EditorType, Is.EqualTo(EditorType.Checkbox));
            Assert.That(createdOnSettings.EditorType, Is.EqualTo(EditorType.DateTime));
            Assert.That(statusSettings.EditorType, Is.EqualTo(EditorType.Text));
            Assert.That(statusForFilterSettings.EditorType, Is.EqualTo(EditorType.Select));
            Assert.That(statusForFilterSettings.FilterQueryValueType, Is.EqualTo(QueryValueType.Enum));
            Assert.That(statusForFilterSettings.FilterQueryTypeEnum, Is.EqualTo(typeof(InfrastructureStatus)));
            Assert.That(notesSettings.GetDisplayValue(entity), Is.EqualTo("<none>"));
            Assert.That(typeof(AnnotatedDerivedEntity).GetVisibleProperties().Select(property => property.Name), Does.Not.Contain(nameof(AnnotatedDerivedEntity.Hidden)));
        });
    }

    [Test]
    public void AttributeAndAuditContractsPreserveConfiguredValues()
    {
        var editorTypeAttribute = new EditorTypeAttribute(EditorType.Color);
        var filterableAttribute = new FilterableAttribute(true, "StatusField", QueryOperator.NotEqual, QueryValueType.Enum, true, typeof(InfrastructureStatus));
        var sortableAttribute = new SortableAttribute(true, "CustomSort");
        var entityAttribute = new EntityAttribute(isSoftDeletable: true, isAuditable: true);
        var propertyChange = new PropertyChange("Name", "Old", "New");
        var entityChange = new EntityChange("AnnotatedDerivedEntity", 7, ChangeAction.Updated, [propertyChange]);
        var timeBefore = DateTimeOffset.UtcNow.AddSeconds(-5);

        Assert.Multiple(() =>
        {
            Assert.That(editorTypeAttribute.EditorType, Is.EqualTo(EditorType.Color));
            Assert.That(filterableAttribute.Filterable, Is.True);
            Assert.That(filterableAttribute.FilterField, Is.EqualTo("StatusField"));
            Assert.That(filterableAttribute.FilterQueryOperator, Is.EqualTo(QueryOperator.NotEqual));
            Assert.That(filterableAttribute.FilterQueryValueType, Is.EqualTo(QueryValueType.Enum));
            Assert.That(filterableAttribute.FilterIgnoreCasing, Is.True);
            Assert.That(filterableAttribute.FilterQueryTypeEnum, Is.EqualTo(typeof(InfrastructureStatus)));
            Assert.That(sortableAttribute.Sortable, Is.True);
            Assert.That(sortableAttribute.SortExpression, Is.EqualTo("CustomSort"));
            Assert.That(entityAttribute.IsSoftDeletable, Is.True);
            Assert.That(entityAttribute.IsAuditable, Is.True);
            Assert.That(propertyChange.PropertyName, Is.EqualTo("Name"));
            Assert.That(propertyChange.OldValue, Is.EqualTo("Old"));
            Assert.That(propertyChange.NewValue, Is.EqualTo("New"));
            Assert.That(entityChange.EntityName, Is.EqualTo("AnnotatedDerivedEntity"));
            Assert.That(entityChange.EntityId, Is.EqualTo(7));
            Assert.That(entityChange.Operation, Is.EqualTo(ChangeAction.Updated));
            Assert.That(entityChange.Properties, Has.Length.EqualTo(1));
            Assert.That(entityChange.TimeStamp, Is.GreaterThanOrEqualTo(timeBefore));
        });
    }

    [Test]
    public void SpecificationRecordsPreserveConfiguredArguments()
    {
        var filter = new FilterConditionExpression(nameof(AnnotatedDerivedEntity.Name), FilterConditionOperator.Contains, "Ada", IgnoreCasing: true);
        var sort = new SortExpression(nameof(AnnotatedDerivedEntity.Name), Descending: true);
        var include = new IncludeExpressionStub(nameof(AnnotatedDerivedEntity.Children), []);
        var specification = new Specification<int, AnnotatedDerivedEntity>(
            entity => entity.Id > 0,
            [filter],
            [include],
            [sort]);
        var pagedSpecification = new PagedSpecification<int, AnnotatedDerivedEntity>(
            entity => entity.Id > 0,
            [filter],
            [include],
            [sort],
            pageIndex: 2,
            pageSize: 25);

        Assert.Multiple(() =>
        {
            Assert.That(specification.Where, Is.Not.Null);
            Assert.That(specification.Filters, Is.EqualTo(new IFilterConditionExpression[] { filter }));
            Assert.That(specification.Includes, Is.EqualTo(new IIncludeExpression[] { include }));
            Assert.That(specification.Sort, Is.EqualTo(new ISortExpression[] { sort }));
            Assert.That(pagedSpecification.PageIndex, Is.EqualTo(2));
            Assert.That(pagedSpecification.PageSize, Is.EqualTo(25));
            Assert.That(sort.PropertyName, Is.EqualTo(nameof(AnnotatedDerivedEntity.Name)));
            Assert.That(sort.Descending, Is.True);
            Assert.That(filter.PropertyName, Is.EqualTo(nameof(AnnotatedDerivedEntity.Name)));
            Assert.That(filter.Value, Is.EqualTo("Ada"));
            Assert.That(filter.IgnoreCasing, Is.True);
            Assert.That(include.PropertyName, Is.EqualTo(nameof(AnnotatedDerivedEntity.Children)));
        });
    }

    [Test]
    public void RepositoryFactoryUsesMappedRepositoryOrFallsBackToDefaultInstances()
    {
        var mapper = new RepositoryMapper();
        mapper.Register(typeof(CustomRepository));
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());

        var mappedInstance = new CustomRepository();
        var mappedFactory = new TestRepositoryFactory(mapper, loggerFactory, mappedInstance);
        var mappedReadOnly = mappedFactory.CreateReadOnlyRepository<int, RepoEntity>();
        var mappedRepository = mappedFactory.CreateRepository<int, RepoEntity>();

        var fallbackFactory = new TestRepositoryFactory(new RepositoryMapper(), loggerFactory, instanceToReturn: null);
        var fallbackReadOnly = fallbackFactory.CreateReadOnlyRepository<int, RepoEntity>();
        var fallbackRepository = fallbackFactory.CreateRepository<int, RepoEntity>();

        Assert.Multiple(() =>
        {
            Assert.That(mappedReadOnly, Is.SameAs(mappedInstance));
            Assert.That(mappedRepository, Is.SameAs(mappedInstance));
            Assert.That(fallbackReadOnly, Is.TypeOf<FallbackReadOnlyRepository<int, RepoEntity>>());
            Assert.That(fallbackRepository, Is.TypeOf<FallbackRepository<int, RepoEntity>>());
            Assert.That(fallbackFactory.CreateInstanceCallCount, Is.EqualTo(0));
            Assert.That(mappedFactory.CreateInstanceCallCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void UnitOfWorkDelegatesRepositoryCreationSaveAndDisposeToSubclassHooks()
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var repositoryMapper = Substitute.For<IRepositoryMapper>();
        var factory = new TestRepositoryFactory(new RepositoryMapper(), loggerFactory, instanceToReturn: null);
        var unitOfWork = new TestUnitOfWork(repositoryMapper, loggerFactory, factory);

        var readOnlyRepository = unitOfWork.GetReadOnlyRepository<int, RepoEntity>();
        var repository = unitOfWork.GetRepository<int, RepoEntity>();
        var audits = unitOfWork.GetAuditsAsync<TestAudit>(Guid.NewGuid()).Result;

        unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
        ((IDisposable)unitOfWork).Dispose();

        Assert.Multiple(() =>
        {
            Assert.That(readOnlyRepository, Is.TypeOf<FallbackReadOnlyRepository<int, RepoEntity>>());
            Assert.That(repository, Is.TypeOf<FallbackRepository<int, RepoEntity>>());
            Assert.That(unitOfWork.BuildRepositoryFactoryCallCount, Is.EqualTo(1));
            Assert.That(unitOfWork.SaveUnitOfWorkCallCount, Is.EqualTo(1));
            Assert.That(unitOfWork.DisposeUnitOfWorkCallCount, Is.EqualTo(1));
            Assert.That(audits, Is.Empty);
        });
    }

    [Test]
    public void UnitOfWorkFactoryUsesRegisteredRepositoryMapperOrCreatesDefaultOne()
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        var serviceProviderWithMapper = Substitute.For<IServiceProvider>();
        var registeredMapper = Substitute.For<IRepositoryMapper>();
        serviceProviderWithMapper.GetService(typeof(ILoggerFactory)).Returns(loggerFactory);
        serviceProviderWithMapper.GetService(typeof(IRepositoryMapper)).Returns(registeredMapper);
        var factoryWithMapper = new TestUnitOfWorkFactory(serviceProviderWithMapper);

        var unitOfWorkWithMapper = factoryWithMapper.CreateUnitOfWork();

        var serviceProviderWithoutMapper = Substitute.For<IServiceProvider>();
        serviceProviderWithoutMapper.GetService(typeof(ILoggerFactory)).Returns(loggerFactory);
        var factoryWithoutMapper = new TestUnitOfWorkFactory(serviceProviderWithoutMapper);

        var unitOfWorkWithoutMapper = factoryWithoutMapper.CreateUnitOfWork();

        Assert.Multiple(() =>
        {
            Assert.That(factoryWithMapper.LastRepositoryMapper, Is.SameAs(registeredMapper));
            Assert.That(factoryWithMapper.LastLoggerFactory, Is.SameAs(loggerFactory));
            Assert.That(factoryWithoutMapper.LastRepositoryMapper, Is.TypeOf<RepositoryMapper>());
            Assert.That(factoryWithoutMapper.LastLoggerFactory, Is.SameAs(loggerFactory));
            Assert.That(unitOfWorkWithMapper, Is.Not.Null);
            Assert.That(unitOfWorkWithoutMapper, Is.Not.Null);
        });

        unitOfWorkWithMapper.Dispose();
        unitOfWorkWithoutMapper.Dispose();
    }

    [Entity(isSoftDeletable: true, isAuditable: true)]
    private class AnnotatedBaseEntity : IEntityWithDates<int>
    {
        [Key]
        [DisplayName("Identifier")]
        [DefaultValue(99)]
        [Sortable(true, "IdSort")]
        [Filterable(true, "IdFilter", QueryOperator.Equal, QueryValueType.Integer)]
        public int Id { get; set; }

        public DateTimeOffset CreatedOn { get; init; }

        public DateTimeOffset UpdatedOn { get; init; }
    }

    private sealed class AnnotatedDerivedEntity : AnnotatedBaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; init; }

        public InfrastructureStatus Status { get; init; }

        [EditorType(EditorType.Select)]
        [Filterable(true, filterQueryValueType: QueryValueType.Enum, filterQueryTypeEnum: typeof(InfrastructureStatus))]
        public InfrastructureStatus StatusForFilter => Status;

        [DisplayFormat(NullDisplayText = "<none>")]
        public string? Notes { get; init; }

        [ScaffoldColumn(true)]
        public string Hidden { get; init; } = string.Empty;

        public List<RepoEntity> Children { get; init; } = [];
    }

    private enum InfrastructureStatus
    {
        Inactive,
        Active,
    }

    private sealed class IncludeExpressionStub(string propertyName, IIncludeExpression[] nestedExpressions) : IIncludeExpression
    {
        public string PropertyName { get; } = propertyName;

        public IIncludeExpression[] NestedExpressions { get; } = nestedExpressions;
    }

    private sealed class RepoEntity : IEntity<int>
    {
        public int Id { get; init; }
    }

    private sealed class TestAudit : IEntityAudit
    {
        public Guid Id { get; init; }
        public DateTimeOffset CreatedOn { get; init; }
        public DateTimeOffset UpdatedOn { get; init; }
        public Guid UserId { get; init; }
        public string EntityId { get; init; } = string.Empty;
        public string EntityName { get; init; } = string.Empty;
        public ChangeAction Action { get; init; }
        public string Properties { get; init; } = string.Empty;
    }

    private sealed class CustomRepository : FallbackRepository<int, RepoEntity>;

    private sealed class TestRepositoryFactory(IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory, object? instanceToReturn)
        : RepositoryFactory(repositoryMapper, loggerFactory)
    {
        public int CreateInstanceCallCount { get; private set; }

        protected override object? CreateInstance(Type type, ILogger logger)
        {
            CreateInstanceCallCount++;
            return instanceToReturn;
        }

        protected override IReadOnlyRepository<TKey, TEntity> CreateReadOnlyRepositoryInstance<TKey, TEntity>(ILogger logger) =>
            new FallbackReadOnlyRepository<TKey, TEntity>();

        protected override IRepository<TKey, TEntity> CreateRepositoryInstance<TKey, TEntity>(ILogger logger) =>
            new FallbackRepository<TKey, TEntity>();
    }

    private sealed class TestUnitOfWork(IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory, IRepositoryFactory repositoryFactory)
        : UnitOfWork(repositoryMapper, loggerFactory)
    {
        public int BuildRepositoryFactoryCallCount { get; private set; }
        public int DisposeUnitOfWorkCallCount { get; private set; }
        public int SaveUnitOfWorkCallCount { get; private set; }

        protected override IRepositoryFactory BuildRepositoryFactory(IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory)
        {
            BuildRepositoryFactoryCallCount++;
            return repositoryFactory;
        }

        protected override void DisposeUnitOfWork() => DisposeUnitOfWorkCallCount++;

        protected override Task SaveUnitOfWorkAsync(CancellationToken cancellationToken = default)
        {
            SaveUnitOfWorkCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestUnitOfWorkFactory(IServiceProvider serviceProvider) : UnitOfWorkFactory(serviceProvider)
    {
        public IRepositoryMapper? LastRepositoryMapper { get; private set; }
        public ILoggerFactory? LastLoggerFactory { get; private set; }

        protected override IUnitOfWork CreateUnitOfWorkInstance(IRepositoryMapper repositoryMapper, ILoggerFactory loggerFactory)
        {
            LastRepositoryMapper = repositoryMapper;
            LastLoggerFactory = loggerFactory;
            return new TestUnitOfWork(repositoryMapper, loggerFactory, new TestRepositoryFactory(repositoryMapper, loggerFactory, null));
        }
    }

    private class FallbackReadOnlyRepository<TKey, TEntity> : IReadOnlyRepository<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
    {
        public IQueryable<TEntity> AsQuery() => Array.Empty<TEntity>().AsQueryable();
        public Task<TEntity?> GetByIdAsync(TKey key, CancellationToken cancellationToken = default) => Task.FromResult<TEntity?>(null);
        public Task<TEntity?> FirstOrDefaultAsync(ISpecification<TKey, TEntity> specification, CancellationToken cancellationToken = default) => Task.FromResult<TEntity?>(null);
        public Task<TEntity?> SingleOrDefaultAsync(ISpecification<TKey, TEntity> specification, CancellationToken cancellationToken = default) => Task.FromResult<TEntity?>(null);
        public Task<IList<TEntity>> ListAsync(ISpecification<TKey, TEntity>? specification = null, CancellationToken cancellationToken = default) => Task.FromResult<IList<TEntity>>([]);
        public Task<IPagedList<TEntity>> ListPagedAsync(IPagedSpecification<TKey, TEntity> specification, CancellationToken cancellationToken = default) => Task.FromResult<IPagedList<TEntity>>(new Chishiki.PagedList<TEntity>([], 0, 0, 10));
        public TEntity? GetById(TKey key) => null;
        public TEntity? FirstOrDefault(ISpecification<TKey, TEntity> specification) => null;
        public TEntity? SingleOrDefault(ISpecification<TKey, TEntity> specification) => null;
        public IList<TEntity> List(ISpecification<TKey, TEntity>? specification = null) => [];
        public IPagedList<TEntity> ListPaged(IPagedSpecification<TKey, TEntity> specification) => new Chishiki.PagedList<TEntity>([], 0, 0, 10);
        public void Dispose() { }
    }

    private class FallbackRepository<TKey, TEntity> : FallbackReadOnlyRepository<TKey, TEntity>, IRepository<TKey, TEntity>
        where TEntity : class, IEntity<TKey>
    {
        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteRangeByIdAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Add(TEntity entity) { }
        public void Update(TEntity entity) { }
        public void Delete(TEntity entity) { }
        public void DeleteById(TKey id) { }
        public void AddRange(IEnumerable<TEntity> entities) { }
        public void UpdateRange(IEnumerable<TEntity> entities) { }
        public void DeleteRange(IEnumerable<TEntity> entities) { }
        public void DeleteRangeById(IEnumerable<TKey> ids) { }
    }
}
