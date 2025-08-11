namespace GamificationEngine.Shared;

public readonly struct Result<TSuccess, TError>
{
    public bool IsSuccess { get; }
    public TSuccess? Value { get; }
    public TError? Error { get; }

    private Result(bool isSuccess, TSuccess? value, TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TSuccess, TError> Success(TSuccess value)
        => new Result<TSuccess, TError>(true, value, default);

    public static Result<TSuccess, TError> Failure(TError error)
        => new Result<TSuccess, TError>(false, default, error);
}

public static class Result
{
    public static Result<TSuccess, TError> Success<TSuccess, TError>(TSuccess value)
        => Result<TSuccess, TError>.Success(value);

    public static Result<TSuccess, TError> Failure<TSuccess, TError>(TError error)
        => Result<TSuccess, TError>.Failure(error);
}