// -----------------------------------------------------------------------------
// File:        RequestNotification.cs
// Author:      Piergiorgio Vagnozzi
// Description: Request notification record for handling request outcomes.
// Created:     2024-07-08
// Modified:    2024-07-08
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------

using Chishiki.Messaging.Abstractions;

namespace Chishiki.Messaging.Common;

/// <summary>Request notification.</summary>
/// <seealso cref="IRequestNotification" />
/// <seealso cref="IEquatable&lt;RequestNotification&gt;" />
public record RequestNotification : IRequestNotification
{
    /// <summary>Initializes a new instance of the <see cref="RequestNotification"/> class. .</summary>
    /// <param name="request">The request.</param>
    /// <param name="message">The message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="error">The error.</param>
    public RequestNotification(IRequestMessage request, string? message = null, Exception? exception = null,
        bool? error = null)
    {
        Request = request;
        Message = message ?? exception?.Message;
        Exception = exception;
        IsError = error ?? exception != null;
    }

    /// <summary>Gets the request. .</summary>
    /// <value>
    /// The request.
    /// </value>
    public IRequestMessage Request { get; init; }

    /// <summary>Gets the exception. .</summary>
    /// <value>
    /// The exception.
    /// </value>
    public Exception? Exception { get; init; }

    /// <summary>Gets a value indicating whether this <see cref="RequestNotification"/> is error. .</summary>
    /// <value>
    ///   <c>true</c> if error; otherwise, <c>false</c>.
    /// </value>
    public bool IsError { get; init; }

    /// <summary>Gets the message. .</summary>
    /// <value>
    /// The message.
    /// </value>
    public string? Message { get; init; }

    /// <summary>Gets the time stamp. .</summary>
    /// <value>
    /// The time stamp.
    /// </value>
    public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the mark time stamp. .</summary>
    /// <value>
    /// The mark time stamp.
    /// </value>
    public DateTimeOffset? MarkTimeStamp { get; protected internal set; }
}

/// <summary>Request notification extensions.</summary>
public static class RequestNotificationExtensions
{
    /// <summary>Sets the marked. .</summary>
    /// <param name="notification">The notification.</param>
    /// <param name="markedTime">The marked time.</param>
    /// <returns>The marked notification.</returns>
    public static RequestNotification SetMarked(this RequestNotification notification,
        DateTimeOffset? markedTime = null)
    {
        notification.MarkTimeStamp = markedTime ?? DateTimeOffset.UtcNow;
        return notification;
    }
}

