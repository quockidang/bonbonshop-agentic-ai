using System.Text.Json.Serialization;

namespace BonBon.McpServers.Clients.SeedWork;

public record ApiResult<T>
{
    [JsonConstructor]
    public ApiResult(int errorCode, string? errorDescription)
    {
        ErrorDescription = errorDescription;
        ErrorCode = errorCode;
    }

    public ApiResult(int errorCode, T? data, string? errorDescription)
    {
        Data = data;
        ErrorDescription = errorDescription;
        ErrorCode = errorCode;
    }

    [JsonPropertyName("ErrorCode")]
    public int ErrorCode { get; set; }
    
    [JsonPropertyName("ErrorDescription")]
    public string? ErrorDescription { get; set; }
    
    [JsonPropertyName("Data")]
    public T? Data { get; set;}
}


public class HttpErrorCode
{
    public const int CodeError = 1;

    public const int CodeSuccess = 0;
}