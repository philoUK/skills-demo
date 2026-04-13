namespace AdminModule;

internal abstract record Result<T>;

internal record Ok<T>(T Value) : Result<T>;

internal record Error<T>(string[] Errors) : Result<T>;
