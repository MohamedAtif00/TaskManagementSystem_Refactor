namespace TaskManagementSystem.BuildingBlocks.Domain;

public readonly struct Result<T> : IResult, IEquatable<Result<T>>
{
    private readonly bool _hasValue;
    private readonly T? _value;
    private readonly ResultError? _error;

    private Result(T value)
    {
        _hasValue = true;
        _value = value;
        _error = null;
    }

    private Result(ResultError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        _hasValue = false;
        _value = default;
        _error = error;
    }

    public bool IsSuccess => _hasValue;

    public bool IsFailure => !IsSuccess;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot access Value when result is a failure.");

    public ResultError Error =>
        _error ?? throw new InvalidOperationException(
            IsSuccess
                ? "Cannot access Error when result is successful."
                : "Result is uninitialized.");

    public static Result<T> Ok(T value) => new(value);

    public static Result<T> Fail(ResultError error) => new(error);

    public static Result<T> Fail(string code, string message) =>
        Fail(new ResultError(code, message));

    public static implicit operator Result<T>(ResultError error) => Fail(error);

    public bool TryGetValue(out T value)
    {
        if (IsSuccess)
        {
            value = _value!;
            return true;
        }

        value = default!;
        return false;
    }

    public bool TryGetError(out ResultError error)
    {
        if (_error is not null)
        {
            error = _error;
            return true;
        }

        error = default!;
        return false;
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Ok(mapper(Value)) : Result<TOut>.Fail(_error!);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder) =>
        IsSuccess ? binder(Value) : Result<TOut>.Fail(_error!);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ResultError, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(_error!);

    public bool Equals(Result<T> other) =>
        IsSuccess == other.IsSuccess &&
        EqualityComparer<T>.Default.Equals(_value, other._value) &&
        EqualityComparer<ResultError?>.Default.Equals(_error, other._error);

    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_hasValue, _value, _error);

    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

    public static bool operator !=(Result<T> left, Result<T> right) => !left.Equals(right);
}

public static class Result
{
    public static Result<NoValue> Ok() => Result<NoValue>.Ok(NoValue.Value);

    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    public static Result<T> Fail<T>(ResultError error) => Result<T>.Fail(error);

    public static Result<NoValue> Fail(ResultError error) => Result<NoValue>.Fail(error);
}
