# fizz-ng

A B2C application where members access cashback offers and purchase gift card vouchers.

## Architecture

- **Modular monolith** using **vertical slices**. Each feature owns its own request, handler, and response. No cross-cutting service layers.
- Orchestrated via **.NET Aspire** (`src/AppHost`).
- Frontend SPAs: `adminweb` and `memberweb` (VITE + React + TypeScript).

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
