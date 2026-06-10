using System.Net;

namespace Chishiki.Web.Client;

public record RestResult(string Content = "", HttpStatusCode StatusCode = HttpStatusCode.OK, string Message = "")
{
    public RestResult(HttpStatusCode statusCode, string message = "") : this(string.Empty, statusCode, message)
    {
    }

    public HttpStatusCode StatusCode { get; init; } = StatusCode;

    public string Message { get; init; } = Message;

    public string Content { get; init; } = Content;
}
