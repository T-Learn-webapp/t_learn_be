namespace TLearn.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; }
    public bool IsUnauthorized { get; set; }
    
    
    private Result(bool isSuccess, T? data, string? error, bool isUnauthorized = false)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        IsUnauthorized = isUnauthorized;
    }

    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, null);
    }
    
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error );
    }
    
    public static Result<T> Unauthorized(string error = "Unauthorized")
    {
        return new Result<T>(false, default, error, true);
    }

}