using Refit;

namespace Regression;

public interface IApi
{
    [Post("/call")]
    Task CallWithInterfaceRequest([Body] InterfaceRequest request);

    [Post("/call")]
    Task CallWithAbstractRequest([Body] AbstractRequest request);
}
