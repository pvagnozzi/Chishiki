// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Chishiki.Clustering.Grains;
using Chishiki.Hub.Contracts;

namespace Chishiki.Clustering.UnitTests.Grains;

[TestFixture, Category("Unit")]
public class IHubResourceGrainContractTests
{
    [Test]
    public void IHubResourceGrain_InheritsFromIGrainWithStringKey()
    {
        Assert.That(
            typeof(IHubResourceGrain).IsAssignableTo(typeof(IGrainWithStringKey)),
            Is.True);
    }

    [Test]
    public void IHubResourceGrain_GetAsync_ReturnsNullableHubResourceDto()
    {
        var method = typeof(IHubResourceGrain).GetMethod(nameof(IHubResourceGrain.GetAsync));

        Assert.That(method, Is.Not.Null);
        Assert.That(
            method!.ReturnType,
            Is.EqualTo(typeof(Task<HubResourceDto?>)));
    }

    [Test]
    public void IHubResourceGrain_SetAsync_AcceptsHubResourceDto()
    {
        var method = typeof(IHubResourceGrain).GetMethod(nameof(IHubResourceGrain.SetAsync));

        Assert.That(method, Is.Not.Null);

        var parameters = method!.GetParameters();
        Assert.That(parameters, Has.Length.EqualTo(1));
        Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(HubResourceDto)));
    }

    [Test]
    public void IHubResourceGrain_InvalidateAsync_HasNoParameters()
    {
        var method = typeof(IHubResourceGrain).GetMethod(nameof(IHubResourceGrain.InvalidateAsync));

        Assert.That(method, Is.Not.Null);
        Assert.That(method!.GetParameters(), Is.Empty);
    }
}
