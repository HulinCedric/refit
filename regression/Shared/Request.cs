namespace Regression;

public sealed class Request : InterfaceRequest
{
    public string Name { get; set; } = string.Empty;
}

public sealed class ConcreteRequest : AbstractRequest
{
    public string Name { get; set; } = string.Empty;
}
