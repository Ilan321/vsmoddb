using System.Net;

namespace VsModDb.Models.Exceptions;

public class LegacyApiEnabledException() : StatusCodeException(HttpStatusCode.BadRequest, "LEGACY_API_MODE_ENABLED");
