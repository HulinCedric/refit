// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Diagnostics.CodeAnalysis;

namespace Refit;

/// <summary>Signals, between Refit's response helpers, that the response body could not be downloaded.</summary>
/// <remarks>
/// Never reaches callers: the helpers turn it into the exception produced by
/// <see cref="RefitSettings.TransportExceptionFactory"/>, from its <see cref="Exception.InnerException"/>.
/// </remarks>
[SuppressMessage("Design", "CA1064", Justification = "Internal signal between Refit's response helpers; it is never thrown to callers.")]
internal sealed class ResponseBodyReadException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="ResponseBodyReadException"/> class.</summary>
    public ResponseBodyReadException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ResponseBodyReadException"/> class with a message.</summary>
    /// <param name="message">The message that describes the error.</param>
    public ResponseBodyReadException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ResponseBodyReadException"/> class with the failure that stopped the download.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that made downloading the response body fail.</param>
    public ResponseBodyReadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
