using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BonBon.McpServers.Clients.SeedWork;

public record ApiSuccessResult<T> : ApiResult<T>
{
    [JsonConstructor]
    public ApiSuccessResult(T? data) : base(HttpErrorCode.CodeSuccess, data, "Xử lý thành công.")
    {
        
    }
    
    public ApiSuccessResult(T? data, string? message) : base(HttpErrorCode.CodeSuccess, data, message)
    {
        
    }
    
    
    public ApiSuccessResult(int code, T? data, string? message) : base(code, data, message)
    {
        
    }
}

public record ApiErrorResult<T>(string? message) : ApiResult<T>(HttpErrorCode.CodeError, message)
{
    public ApiErrorResult(string? message, T data) : this(message)
    {
        Data = data;
    }
}

public record ApiPagingResultSuccess<T> : ApiSuccessResult<ResultPaging<T>>
{
    public ApiPagingResultSuccess(ResultPaging<T>? data) : base(data)
    {
    }

    public ApiPagingResultSuccess(ResultPaging<T>? data, string? message) : base(data, message)
    {
    }
}

public record ResultPaging<T>
{
    [JsonPropertyName("PageInfo")]
    public PageInfoDto PageInfo { get; init; } = new PageInfoDto();
    [JsonPropertyName("Items")]
    public IEnumerable<T> Items { get; init; } = new List<T>();
}

public record ApiErrorMessage
{
    public ApiErrorMessage(string title, string message)
    {
        Title = title;
        Message = message;
    }
    
    [JsonPropertyName("title")]
    public string Title { get; init; }
    
    [JsonPropertyName("message")]
    public string Message { get; init; }
}