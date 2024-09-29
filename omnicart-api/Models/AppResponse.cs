namespace omnicart_api.Models
{
    public class AppResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public required string Message { get; set; }
        public string? Error { get; set; }
        public int? ErrorCode { get; set; }
        public object? ErrorData { get; set; }
    }
}
