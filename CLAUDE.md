# fizz-ng

A B2C application where members access cashback offers and purchase gift card vouchers.

## Architecture

- **Modular monolith** using **vertical slices**. Each feature owns its own request, handler, and response. No cross-cutting service layers.
- Orchestrated via **.NET Aspire** (`src/AppHost`).
- Frontend SPAs: `adminweb` and `memberweb` (Vite + React + TypeScript).

## Observability

**Prefer span attributes over logs.** Every endpoint handler should enrich the current span with attributes that would help a support engineer diagnose an issue without needing to reproduce it. Ask: _what would I wish I knew when investigating this at 2am?_

Good candidates for span attributes:

- Member ID, offer ID, voucher ID, transaction ID
- Decision outcomes (e.g. `offer.eligible = false`, `voucher.status = redeemed`)
- External call results (provider name, status code, latency)
- Retry counts, cache hit/miss

```csharp
Activity.Current?.SetTag("member.id", memberId);
Activity.Current?.SetTag("offer.id", offerId);
Activity.Current?.SetTag("offer.eligible", false);
```

**Logs** are fine when they add supportability value that spans can't capture (e.g. free-text reasoning, batch job progress). Don't add logs just to have them.

## Conventions

- .NET 10, C# with nullable and implicit usings enabled.
- Vertical slices: one folder per feature, co-locate request/handler/response/validation.
- Frontend: functional React components, TypeScript strict mode.

### Domain Driven Design

- [Domain Model](./ddd.md)

### Endpoint Design

- [Endpoints](./endpoints.md)

### Persistence

- [Persistence](./persistence.md)

## To be added

- Testing approach
- CSS / component library
- Database and external service patterns
- Auth approach
