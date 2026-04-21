# Domain Driven Design

## Location

The owning module should have a \aggregate root name\domain\ folder. For example of an Account type we would have

- Account
  - Domain
    - Account.cs
    - TransactionHistory.cs
    - Transaction.cs
    - ...

## Design

### Type

Each aggregate root should be designed as an immutable c# record type. For example

```csharp
internal record Account(Guid Id, string Name, ImmutableList<Transaction> Transactions);
```

### Construction

Each aggregate root should have an accompanying static factory class.

```csharp
internal static class AccountFactory
{
    public static Result<Account> Create(...)
}
```

### Operations

Each aggregate root should have an accompanying static Extensions class.

```csharp
internal static class AccountExtensions
{
    public static Result<Account> Deposit(this Account account, Transaction transaction)
    {
        ...
    }
}
```

### Value types

Group related data that changes together into immutable value types, rather than pollute the aggregate type with a large number of fields. Value types should live in the same folder as their aggregate root owners.

```csharp
internal record Modifications(DateTime ModifiedAt, string ModifiedBy);
```

Do use the discriminated union pattern where it makes sense.

```csharp
internal abstract record AccountStatus;

internal record AccountActive : AccountStatus;

internal record AccountInactive : AccountStatus;

internal static class AccountStatusFactory
{
    public static AccountStatus Active() => new AccountActive();

    public static AccountStatus Inactive() => new AccountInactive();
}
```

### Result type

If an operation can possibly fail it should be wrapped in a result type.

```csharp
public abstract record Result<T>;

public record Ok<T>(T Value) : Result<T>;

public record Error<T>(string[] Errors) : Result<T>;
```

### Visibility

Aggregate roots and their value types should be internal by default as they only belong to their module. We can allow test projects to have visibility though but they should never be public.

### Persistence

We never directly store aggregate roots in the database. See [persistence](./persistence.md)

## Sharing

This is a modular monolith and we don't expect many types to be shared between modules, if any. Even if we have 2 modules with an identical value type, we won't share them. The only exception would be for the Result type, which when required can go into a 'shared' class library that can be used by any module.

## Primitive obsession

Avoid simple primitive types if they can be better expressed in a type. When multiple fields are used together, or used to compute a value, they are natural candidates to be combined into a type.
