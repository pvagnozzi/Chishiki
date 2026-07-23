// -----------------------------------------------------------------------------
// File:        MessagingSurfaceTests.cs
// Author:      Piergiorgio Vagnozzi
// Description: Covers focused tests for the Chishiki.Messaging shared library surface.
// Created:     2026-06-10
// Modified:    2026-06-10
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Globalization;
using Chishiki.Data.Abstractions;
using Chishiki.Mapping;
using Chishiki.Messaging.Abstractions;
using Chishiki.Messaging.Common;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace Chishiki.Messaging.Tests;

/// <summary>Provides focused tests for the messaging helper surface.</summary>
[TestFixture]
public sealed class MessagingSurfaceTests
{
    [Test]
    public void RequestNotificationUsesExceptionMessageAndErrorFlagByDefault()
    {
        var request = new TestRequest(Guid.NewGuid());
        var exception = new InvalidOperationException("boom");

        var result = new RequestNotification(request, exception: exception);

        Assert.That(result.Request, Is.SameAs(request));
        Assert.That(result.Message, Is.EqualTo("boom"));
        Assert.That(result.Exception, Is.SameAs(exception));
        Assert.That(result.IsError, Is.True);
    }

    [Test]
    public void RequestNotificationAllowsExplicitMessageAndErrorOverride()
    {
        var request = new TestRequest(Guid.NewGuid());
        var exception = new InvalidOperationException("boom");

        var result = new RequestNotification(request, "handled", exception, error: false);

        Assert.That(result.Message, Is.EqualTo("handled"));
        Assert.That(result.IsError, Is.False);
    }

    [Test]
    public void SetMarkedUsesProvidedTimestampAndReturnsSameInstance()
    {
        var notification = new RequestNotification(new TestRequest(Guid.NewGuid()));
        var markTime = DateTimeOffset.Parse("2026-06-10T10:15:00+00:00", CultureInfo.InvariantCulture);

        var result = notification.SetMarked(markTime);

        Assert.That(result, Is.SameAs(notification));
        Assert.That(notification.MarkTimeStamp, Is.EqualTo(markTime));
    }

    [Test]
    public void RequestMessageHandlerContextResolvesServicesLazilyAndCachesInstances()
    {
        var mapper = Substitute.For<IMapper>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IMapper)).Returns(mapper);
        serviceProvider.GetService(typeof(IUnitOfWork)).Returns(unitOfWork);
        var context = new RequestMessageHandlerContext(serviceProvider);

        var firstMapper = context.Mapper;
        var secondMapper = context.Mapper;
        var firstUnitOfWork = context.UnitOfWork;
        var secondUnitOfWork = context.UnitOfWork;

        Assert.That(firstMapper, Is.SameAs(mapper));
        Assert.That(secondMapper, Is.SameAs(mapper));
        Assert.That(firstUnitOfWork, Is.SameAs(unitOfWork));
        Assert.That(secondUnitOfWork, Is.SameAs(unitOfWork));
        serviceProvider.Received(1).GetService(typeof(IMapper));
        serviceProvider.Received(1).GetService(typeof(IUnitOfWork));
    }

    [Test]
    public void AddRequestMessageContextRegistersHandlerContextOnlyOnce()
    {
        var services = new ServiceCollection();

        _ = services.AddRequestMessageContext();
        _ = services.AddRequestMessageContext();

        Assert.That(services.Count(descriptor => descriptor.ServiceType == typeof(IRequestMessageHandlerContext)), Is.EqualTo(1));
    }

    [Test]
    public void HandlerContextExtensionsResolveServicesFromServiceProvider()
    {
        var dependency = new MarkerService();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var context = Substitute.For<IRequestMessageHandlerContext>();
        context.ServiceProvider.Returns(serviceProvider);
        serviceProvider.GetService(typeof(MarkerService)).Returns(dependency);

        var required = context.GetRequiredService<MarkerService>();
        var optional = context.GetService<MarkerService>();

        Assert.That(required, Is.SameAs(dependency));
        Assert.That(optional, Is.SameAs(dependency));
    }

    private sealed record TestRequest(Guid CorrelationId) : RequestMessage(CorrelationId);

    private sealed class MarkerService;
}
