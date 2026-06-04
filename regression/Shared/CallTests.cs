using FluentAssertions;

using Refit;

using WireMock.Server;

using WireMockRequest = WireMock.RequestBuilders.Request;
using WireMockResponse = WireMock.ResponseBuilders.Response;

namespace Regression;

public class CallTests
{
    readonly IApi api;
    readonly WireMockServer wiremock;

    public CallTests()
    {
        wiremock = WireMockServer.Start();

        wiremock
            .Given(WireMockRequest.Create().WithPath("/call").UsingPost())
            .RespondWith(WireMockResponse.Create().WithStatusCode(200));

        api = RestService.For<IApi>(wiremock.Url!);
    }

    [Fact]
    public async Task Call_with_interface_request()
    {
        // Given
        InterfaceRequest request = new Request { Name = "regression" };

        // When
        await api.CallWithInterfaceRequest(request);

        // Then
        BodySent()
            .Should()
            .Be(
                """
                {"name":"regression"}
                """);
    }

    [Fact]
    public async Task Call_with_abstract_request()
    {
        // Given
        AbstractRequest request = new ConcreteRequest { Name = "regression" };

        // When
        await api.CallWithAbstractRequest(request);

        // Then
        BodySent()
            .Should()
            .Be(
                """
                {"name":"regression"}
                """);
    }

    string BodySent() => wiremock.LogEntries[0].RequestMessage!.Body!;
}
