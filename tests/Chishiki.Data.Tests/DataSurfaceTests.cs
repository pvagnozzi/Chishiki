// -----------------------------------------------------------------------------
// File:        DataSurfaceTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers focused unit tests for the Chishiki.Data shared library surface.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Linq.Expressions;
using System.Reflection;
using Chishiki.Data;
using Chishiki.Data.Abstractions;
using Chishiki.Data.Models;
using Chishiki.Data.Query;
using Chishiki.Data.Specifications;
using Chishiki.Exceptions;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Data.Tests;

/// <summary>Provides focused tests for the public Chishiki.Data surface.</summary>
[TestFixture]
public sealed class DataSurfaceTests
{
    [Test]
    public void ImportFromCsvParsesRecordsWithDefaultDelimiter()
    {
        const string csv = "Id;Name\n1;Ada\n2;Grace";

        var result = csv.ImportFromCsv<CsvRow>();

        Assert.That(result.Select(x => x.Id), Is.EqualTo([1, 2]));
        Assert.That(result.Select(x => x.Name), Is.EqualTo(["Ada", "Grace"]));
    }

    [Test]
    public void ImportFromCsvParsesRecordsWithCustomDelimiter()
    {
        const string csv = "Id,Name\n1,Ada";

        var result = csv.ImportFromCsv<CsvRow>(",");

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("Ada"));
    }

    [TestCase(QueryOperator.Equal, FilterConditionOperator.Equal)]
    [TestCase(QueryOperator.Contains, FilterConditionOperator.Contains)]
    [TestCase(QueryOperator.In, FilterConditionOperator.In)]
    public void GetFilterConditionOperatorMapsKnownOperators(QueryOperator value, FilterConditionOperator expected)
    {
        var result = value.GetFilterConditionOperator();

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void GetAndCastTypeForInOperatorReturnsTypedArray()
    {
        var filter = new PropertyFilterStub("1,2,3", QueryOperator.In, QueryValueType.Integer);

        var result = filter.GetAndCastType();

        Assert.That(result, Is.TypeOf<object[]>());
        Assert.That(((object[])result!).Cast<int>(), Is.EqualTo([1, 2, 3]));
    }

    [Test]
    public void GetAndCastTypeForEnumShortNameResolvesLoadedEnumType()
    {
        var filter = new PropertyFilterStub(nameof(TestStatus.Active), QueryOperator.Equal, QueryValueType.Enum, nameof(TestStatus));

        var result = filter.GetAndCastType();

        Assert.That(result, Is.EqualTo(TestStatus.Active));
    }

    [Test]
    public void GetAndCastTypeWhenValueIsNullReturnsNull()
    {
        var filter = new PropertyFilterStub(null, QueryOperator.Equal, QueryValueType.String);

        var result = filter.GetAndCastType();

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetAndCastTypeWhenEnumTypeIsInvalidThrowsArgumentException()
    {
        var filter = new PropertyFilterStub("Active", QueryOperator.Equal, QueryValueType.Enum, "MissingEnumType");

        Assert.That(() => filter.GetAndCastType(), Throws.ArgumentException);
    }

    [Test]
    public void RepositoryMapperRegisterStoresConcreteRepositoryForEntityType()
    {
        var mapper = new RepositoryMapper();

        _ = mapper.Register(typeof(TestRepository));

        Assert.That(mapper.GetRepositoryType(typeof(TestEntity)), Is.EqualTo(typeof(TestRepository)));
    }

    [Test]
    public void RepositoryMapperRegisterFromAssemblyIgnoresNonRepositoryTypes()
    {
        var mapper = new RepositoryMapper();

        _ = mapper.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        Assert.That(mapper.GetRepositoryType(typeof(TestEntity)), Is.EqualTo(typeof(TestRepository)));
        Assert.That(mapper.GetRepositoryType(typeof(UnmappedEntity)), Is.Null);
    }

    [Test]
    public async Task FirstOrDefaultAsyncOnRepositoryWrapsExpressionInSpecification()
    {
        var repository = Substitute.For<IReadOnlyRepository<int, TestEntity>>();
        var expected = new TestEntity { Id = 7, Name = "Ada" };
        ISpecification<int, TestEntity>? capturedSpecification = null;
        Expression<Func<TestEntity, bool>> filter = entity => entity.Name == "Ada";
        var cancellationToken = new CancellationTokenSource().Token;

        _ = repository.FirstOrDefaultAsync(Arg.Do<ISpecification<int, TestEntity>>(spec => capturedSpecification = spec), cancellationToken)
            .Returns(expected);

        var result = await repository.FirstOrDefaultAsync(filter, cancellationToken);

        Assert.That(result, Is.SameAs(expected));
        Assert.That(capturedSpecification, Is.Not.Null);
        Assert.That(capturedSpecification!.Where!.Compile()(expected), Is.True);
    }

    [Test]
    public async Task FirstOrDefaultAsyncOnUnitOfWorkUsesReadOnlyRepositoryAndDisposesIt()
    {
        var repository = Substitute.For<IReadOnlyRepository<int, TestEntity>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var expected = new TestEntity { Id = 9, Name = "Ada" };
        Expression<Func<TestEntity, bool>> filter = entity => entity.Id == 9;

        _ = unitOfWork.GetReadOnlyRepository<int, TestEntity>().Returns(repository);
        _ = repository.FirstOrDefaultAsync(Arg.Any<ISpecification<int, TestEntity>>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await unitOfWork.FirstOrDefaultAsync<int, TestEntity>(filter);

        Assert.That(result, Is.SameAs(expected));
        _ = unitOfWork.Received(1).GetReadOnlyRepository<int, TestEntity>();
        repository.Received(1).Dispose();
    }

    [Test]
    public void DeleteMasterDetailAsyncWhenEntityIsMissingThrowsNotFoundDomainException()
    {
        var repository = Substitute.For<IRepository<int, TestEntity>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var deleteDetailCalled = false;

        _ = unitOfWork.GetRepository<int, TestEntity>().Returns(repository);
        _ = repository.GetByIdAsync(42, Arg.Any<CancellationToken>()).Returns((TestEntity?)null);

        Assert.That(
            async () => await unitOfWork.DeleteMasterDetailAsync<int, TestEntity>(42, (_, _) =>
            {
                deleteDetailCalled = true;
                return Task.CompletedTask;
            }),
            Throws.TypeOf<NotFoundDomainException>());

        Assert.That(deleteDetailCalled, Is.False);
        _ = repository.DidNotReceive().DeleteByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    private static readonly int[] second = [3];
    private static readonly int[] secondArray = [1];
    private static readonly int[] secondArray0 = [2];

    [Test]
    public async Task UpdateAsyncPartitionsInsertedUpdatedAndDeletedEntities()
    {
        var repository = Substitute.For<IRepository<int, TestEntity>>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var incoming = new[]
        {
            new TestEntity { Id = 1, Name = "Updated" },
            new TestEntity { Id = 3, Name = "Inserted" }
        };
        var existing = new[]
        {
            new TestEntity { Id = 1, Name = "Old" },
            new TestEntity { Id = 2, Name = "Deleted" }
        };

        _ = unitOfWork.GetRepository<int, TestEntity>().Returns(repository);

        await unitOfWork.UpdateAsync<int, TestEntity>(incoming, existing);

        await repository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<TestEntity>>(entities => entities.Select(x => x.Id).SequenceEqual(second)),
            Arg.Any<CancellationToken>());
        await repository.Received(1).UpdateRangeAsync(
            Arg.Is<IEnumerable<TestEntity>>(entities => entities.Select(x => x.Id).SequenceEqual(secondArray)),
            Arg.Any<CancellationToken>());
        await repository.Received(1).DeleteRangeAsync(
            Arg.Is<IEnumerable<TestEntity>>(entities => entities.Select(x => x.Id).SequenceEqual(secondArray0)),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void SortOrdersBySinglePropertyExpression()
    {
        var source = new[]
        {
            new TestEntity { Id = 1, Name = "Charlie" },
            new TestEntity { Id = 2, Name = "Ada" }
        }.AsQueryable();

        var result = source.Sort([new SortExpression(nameof(TestEntity.Name))]).ToList();

        Assert.That(result.Select(x => x.Name), Is.EqualTo(["Ada", "Charlie"]));
    }

    [Test]
    public void WhereAppliesCaseInsensitiveStringFilter()
    {
        var source = new[]
        {
            new TestEntity { Id = 1, Name = "Ada" },
            new TestEntity { Id = 2, Name = "Grace" }
        }.AsQueryable();

        var result = source.Where(
            [new FilterConditionExpression(nameof(TestEntity.Name), FilterConditionOperator.Contains, "AD", IgnoreCasing: true)])
            .ToList();

        Assert.That(result.Select(x => x.Id), Is.EqualTo([1]));
    }

    [Test]
    public void ToExpressionSupportsEnumerablePropertyChains()
    {
        var filter = new FilterConditionExpression("Children.Name", FilterConditionOperator.Contains, "needle");
        var candidate = new ParentEntity
        {
            Id = 1,
            Children =
            [
                new ChildEntity { Name = "haystack" },
                new ChildEntity { Name = "needle-value" }
            ]
        };

        var expression = filter.ToExpression<ParentEntity>().Compile();

        Assert.That(expression(candidate), Is.True);
    }

    [Test]
    public void ToPagedListBuildsExpectedMetadata()
    {
        var source = Enumerable.Range(1, 5)
            .Select(value => new TestEntity { Id = value, Name = $"Entity {value}" })
            .AsQueryable();

        var result = source.ToPagedList<int, TestEntity>(pageSize: 2, pageNumber: 1);

        Assert.That(result.Items.Select(x => x.Id), Is.EqualTo([3, 4]));
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.PageIndex, Is.EqualTo(1));
        Assert.That(result.TotalPages, Is.EqualTo(3));
    }

    [Test]
    public void OrderMethodExistsDetectsOrderedQueries()
    {
        var unordered = new[]
        {
            new TestEntity { Id = 1, Name = "B" },
            new TestEntity { Id = 2, Name = "A" }
        }.AsQueryable();
        var ordered = unordered.OrderBy(x => x.Name);

        Assert.That(unordered.OrderMethodExists(), Is.False);
        Assert.That(ordered.OrderMethodExists(), Is.True);
    }

    [Test]
    public void ApplySpecificationAppliesWhereAndSortExpressions()
    {
        var source = new[]
        {
            new TestEntity { Id = 1, Name = "Charlie", Score = 10 },
            new TestEntity { Id = 2, Name = "Ada", Score = 25 },
            new TestEntity { Id = 3, Name = "Grace", Score = 25 }
        }.AsQueryable();
        var specification = new Specification<int, TestEntity>(
            where: entity => entity.Score >= 20,
            sortExpressions: [new SortExpression(nameof(TestEntity.Name))]);

        var result = source.ApplySpecification(specification).ToList();

        Assert.That(result.Select(x => x.Name), Is.EqualTo(["Ada", "Grace"]));
    }

    public sealed class CsvRow
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class PropertyFilterStub(string? value, QueryOperator filterOperator, QueryValueType valueType, string? typeValueEnum = null)
        : IPropertyFilter
    {
        public string? GetValue() => value;

        public QueryOperator FilterOperator { get; } = filterOperator;

        public QueryValueType ValueType { get; } = valueType;

        public string? TypeValueEnum { get; } = typeValueEnum;
    }

    public sealed class TestEntity : IEntity<int>
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public int Score { get; init; }
    }

    public sealed class UnmappedEntity : IEntity<int>
    {
        public int Id { get; init; }
    }

    public sealed class ParentEntity : IEntity<int>
    {
        public int Id { get; init; }

        public List<ChildEntity> Children { get; init; } = [];
    }

    public sealed class ChildEntity
    {
        public string Name { get; init; } = string.Empty;
    }

    private enum TestStatus
    {
        Inactive,
        Active
    }

    public sealed class TestRepository : IRepository<int, TestEntity>
    {
        public IQueryable<TestEntity> AsQuery() => throw new NotImplementedException();

        public Task<TestEntity?> GetByIdAsync(int key, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<TestEntity?> FirstOrDefaultAsync(ISpecification<int, TestEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<TestEntity?> SingleOrDefaultAsync(ISpecification<int, TestEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IList<TestEntity>> ListAsync(ISpecification<int, TestEntity>? specification = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<IPagedList<TestEntity>> ListPagedAsync(IPagedSpecification<int, TestEntity> specification, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public TestEntity? GetById(int key) => throw new NotImplementedException();

        public TestEntity? FirstOrDefault(ISpecification<int, TestEntity> specification) => throw new NotImplementedException();

        public TestEntity? SingleOrDefault(ISpecification<int, TestEntity> specification) => throw new NotImplementedException();

        public IList<TestEntity> List(ISpecification<int, TestEntity>? specification = null) => throw new NotImplementedException();

        public IPagedList<TestEntity> ListPaged(IPagedSpecification<int, TestEntity> specification) => throw new NotImplementedException();

        public Task AddAsync(TestEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateAsync(TestEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteAsync(TestEntity entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task AddRangeAsync(IEnumerable<TestEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task UpdateRangeAsync(IEnumerable<TestEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteRangeAsync(IEnumerable<TestEntity> entities, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task DeleteRangeByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public void Add(TestEntity entity) => throw new NotImplementedException();

        public void Update(TestEntity entity) => throw new NotImplementedException();

        public void Delete(TestEntity entity) => throw new NotImplementedException();

        public void DeleteById(int id) => throw new NotImplementedException();

        public void AddRange(IEnumerable<TestEntity> entities) => throw new NotImplementedException();

        public void UpdateRange(IEnumerable<TestEntity> entities) => throw new NotImplementedException();

        public void DeleteRange(IEnumerable<TestEntity> entities) => throw new NotImplementedException();

        public void DeleteRangeById(IEnumerable<int> ids) => throw new NotImplementedException();

        public void Dispose()
        {
        }
    }
}
