# Persistence

## Entity Framework

### Entity Design

Aggregate roots (see [domain driven design](./ddd.md)) are never stored directly into the database. They should be separate classes.

Each class should be able to accept or return it's corresponding aggregate root and the aggregate root's value type members.

```csharp

/* Domain object */
internal record Account(Guid Id, string Name, ...);

/* Database entity */
internal class AccountEntity
{
    public static AccountEntity CreateNew(Account acc)
    {
        ...
    }

    public AccountEntity ToAccountEntity(Account acc)
    {
        ...
    }
}

```

### Location

They belong in the same module as their domain counterparts but in a \aggregate root name\data\ structure similar to how we set up domain objects. For example

- Account
  - Data
    - AccountEntity.cs
    - IAccountRepository.cs
    - AccountRepository.cs

### Db contexts

As contexts will work with different aggregate roots they should live in a top level contexts folder in their respective module.
