// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Net;
using System.Text;

namespace Refit.Tests;

/// <summary>Tests covering transport failures that happen while the response body is downloading.</summary>
public partial class GeneratedRequestRunnerTests
{
    /// <summary>Verifies a connection reset while downloading the body surfaces as <see cref="ApiRequestException"/>, not as a deserialization error.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Test]
    public async Task SendAsyncThrowsRequestExceptionWhenConnectionResetWhileDownloadingBody()
    {
        var handler = new CapturingHandler(
            static (_, _) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new ConnectionResetContent(), }));
        using var client = CreateClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, RelativeResourcePath);

        var thrown = await Assert
            .That(
                () => GeneratedRequestRunner.SendAsync<GeneratedResult, GeneratedResult>(
                    client,
                    request,
                    CreateSettings(new SystemTextJsonContentSerializer()),
                    isApiResponse: false,
                    shouldDisposeResponse: true,
                    bufferBody: false,
                    CancellationToken.None))
            .ThrowsExactly<ApiRequestException>();

        await Assert.That(HasInnerException<IOException>(thrown)).IsTrue();
    }

    /// <summary>Verifies a connection reset while downloading the body is reported as a request error on an API response.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Test]
    public async Task SendAsyncReturnsRequestErrorWhenConnectionResetWhileDownloadingBody()
    {
        var handler = new CapturingHandler(
            static (_, _) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new ConnectionResetContent(), }));
        using var client = CreateClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, RelativeResourcePath);

        var response = await GeneratedRequestRunner.SendAsync<ApiResponse<GeneratedResult>, GeneratedResult>(
            client,
            request,
            CreateSettings(new SystemTextJsonContentSerializer()),
            isApiResponse: true,
            shouldDisposeResponse: true,
            bufferBody: false,
            CancellationToken.None);

        await Assert.That(response!.HasRequestError(out var error)).IsTrue();
        await Assert.That(HasInnerException<IOException>(error)).IsTrue();
        await Assert.That(response.IsSuccessful).IsFalse();
    }

    /// <summary>Verifies buffering stays best-effort: content that can still be read after a failed buffering is deserialized.</summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Test]
    public async Task SendAsyncDeserializesWhenBufferingFailsButContentCanBeReadAgain()
    {
        var handler = new CapturingHandler(
            static (_, _) => Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK) { Content = new ReadableAfterFailedBufferingContent(), }));
        using var client = CreateClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, RelativeResourcePath);

        var result = await GeneratedRequestRunner.SendAsync<GeneratedResult, GeneratedResult>(
            client,
            request,
            CreateSettings(new SystemTextJsonContentSerializer()),
            isApiResponse: false,
            shouldDisposeResponse: true,
            bufferBody: false,
            CancellationToken.None);

        await Assert.That(result!.Value).IsEqualTo(DeserializedResultValue);
    }

    /// <summary>Returns whether an exception of the given type appears in the exception chain.</summary>
    /// <typeparam name="TException">The exception type to look for.</typeparam>
    /// <param name="exception">The outermost exception.</param>
    /// <returns><see langword="true"/> when the chain contains <typeparamref name="TException"/>.</returns>
    private static bool HasInnerException<TException>(Exception? exception)
        where TException : Exception
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TException)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Behaves like content received from the network: the body can be read only once, and the connection
    /// is reset after the first bytes.
    /// </summary>
    private sealed class ConnectionResetContent : HttpContent
    {
        /// <summary>Whether the body has already been read once.</summary>
        private bool _consumed;

        /// <inheritdoc />
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            Consume();
            await stream.WriteAsync("{\"value\":"u8.ToArray());
            throw new IOException("Unable to read data from the transport connection: Connection reset by peer.");
        }

        /// <inheritdoc />
        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            Consume();
            return Task.FromResult<Stream>(new MemoryStream());
        }

        /// <inheritdoc />
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        /// <summary>Marks the body as read, failing like network content when it is read a second time.</summary>
        /// <exception cref="InvalidOperationException">The body has already been read.</exception>
        private void Consume()
        {
            if (_consumed)
            {
                throw new InvalidOperationException("The stream was already consumed. It cannot be read again.");
            }

            _consumed = true;
        }
    }

    /// <summary>Content whose buffering fails but which can still be read afterwards.</summary>
    private sealed class ReadableAfterFailedBufferingContent : HttpContent
    {
        /// <inheritdoc />
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            throw new IOException("Buffering failed.");

        /// <inheritdoc />
        protected override Task<Stream> CreateContentReadStreamAsync() =>
            Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes($"{{\"value\":{DeserializedResultValue}}}")));

        /// <inheritdoc />
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
