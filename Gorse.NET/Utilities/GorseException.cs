using System.Net;

namespace Gorse.NET.Utilities;

/// <summary>
/// A request to Gorse failed. <see cref="Exception.Message"/> carries the response
/// body when the server answered, or the transport error when it did not
/// (<see cref="StatusCode"/> is then 0).
/// </summary>
public class GorseException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public GorseException(string? message, HttpStatusCode statusCode, Exception? innerException = null)
        : base(string.IsNullOrEmpty(message) ? $"Gorse request failed with status {(int)statusCode}" : message, innerException)
    {
        StatusCode = statusCode;
    }
}
