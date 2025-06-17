namespace IdempotentApiDemo.API.Models.Responses;

public class IdempotentResponse
{
    public int StatusCode { get; set; }
    public object? Value { get; set; }
}