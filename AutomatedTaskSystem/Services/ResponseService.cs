namespace AutomatedTaskSystem.Services.ResponseService;

public class ResponseService<T> : BaseResponseService
{
	public T? Data { get; set; }
}
public class BaseResponseService
{
	public bool Error { get; set; } = false;
	public string Message { get; set; } = string.Empty;
}