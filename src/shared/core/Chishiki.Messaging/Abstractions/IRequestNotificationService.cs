// -----------------------------------------------------------------------------
// File:        IRequestNotificationService.cs
// Author:      Piergiorgio Vagnozzi
// Description: Service interface for managing request notification persistence and lifecycle.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions;

/// <summary>Request notification service interface.</summary>
public interface IRequestNotificationService : IDisposable
{
    /// <summary>Saves the notification. .</summary>
    /// <param name="requestNotification">Request notification to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveNotificationAsync(IRequestNotification requestNotification, CancellationToken cancellationToken = default);

    /// <summary>Marks the notification as processed. .</summary>
    /// <param name="correlationId">Correlation ID of the notification to mark.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task MarkNotificationAsync(Guid correlationId, CancellationToken cancellationToken = default);
}
