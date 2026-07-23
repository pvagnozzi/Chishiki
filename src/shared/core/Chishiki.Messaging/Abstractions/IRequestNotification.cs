// -----------------------------------------------------------------------------
// File:        IRequestNotification.cs
// Author:      Piergiorgio Vagnozzi
// Description: Request notification interface for capturing request execution results and errors.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

namespace Chishiki.Messaging.Abstractions;

/// <summary>Request notification interface.</summary>
public interface IRequestNotification
{
    /// <summary>Gets the request. .</summary>
    IRequestMessage Request { get; }

    /// <summary>Gets the exception if the request failed. .</summary>
    Exception? Exception { get; }

    /// <summary>Gets a value indicating whether this request resulted in an error. .</summary>
    bool IsError { get; }

    /// <summary>Gets the message describing the result. .</summary>
    string? Message { get; }

    /// <summary>Gets the time stamp when the request was issued. .</summary>
    DateTimeOffset TimeStamp { get; }

    /// <summary>Gets the time stamp when the notification was marked as processed. .</summary>
    DateTimeOffset? MarkTimeStamp { get; }
}
