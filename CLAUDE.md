# fizz-ng

A B2C application where members access cashback offers and purchase gift card vouchers.

## Architecture

See [Architecture](./Architecture.md)

## Observability

See [Observability](./Observability.md)

## Conventions

- .NET 10, C# with nullable and implicit using-s enabled.
- Vertical slices: one folder per feature, co-locate request/handler/response/validation.
- Frontend: functional React components, TypeScript strict mode.

### Domain Driven Design

- [Domain Model](./ddd.md)

### Endpoint Design

- [Endpoints](./endpoints.md)

### Persistence

- [Persistence](./persistence.md)
