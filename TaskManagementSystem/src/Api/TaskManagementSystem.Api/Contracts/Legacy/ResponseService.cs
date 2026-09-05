namespace TaskManagementSystem.Api.Contracts.Legacy;

public class BaseResponseService
{
    public bool Error { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ResponseService<T> : BaseResponseService
{
    public T? Data { get; set; }
}
