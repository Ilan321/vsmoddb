namespace VsModDb.Models.Legacy;

public abstract class BaseLegacyResponse
{
    /// <summary>
    /// Gets the status code of the request.
    /// </summary>
    public string StatusCode { get; init; }
    public bool IsSuccess => StatusCode == "200";
}
