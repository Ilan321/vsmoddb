using Microsoft.AspNetCore.Http;
using System.Net;

namespace VsModDb.Models.Exceptions;

public class StatusCodeException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ErrorCode { get; }

    public StatusCodeException(HttpStatusCode statusCode, string errorCode)
        : this(statusCode)
    {
        ErrorCode = errorCode;
    }

    public StatusCodeException(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
}