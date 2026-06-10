namespace Chishiki.Web.Client;

public interface IRestClient : IDisposable
{
    Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default);

}
