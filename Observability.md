# Observability

## Prefer span attributes over logs

Every endpoint handler should enrich the current span with attributes that would help a support engineer diagnose an issue without needing to reproduce it. Ask: _what would I wish I knew when investigating this at 2am?_

### Good candidates for span attributes

- Member ID, offer ID, voucher ID, transaction ID
- Decision outcomes (e.g. `offer.eligible = false`, `voucher.status = redeemed`)
- External call results (provider name, status code, latency)
- Retry counts, cache hit/miss

```csharp
Activity.Current?.SetTag("member.id", memberId);
Activity.Current?.SetTag("offer.id", offerId);
Activity.Current?.SetTag("offer.eligible", false);
```

## Logs

Logs are fine when they add supportability value that spans can't capture (e.g. free-text reasoning, batch job progress). Don't add logs just to have them.

## Dependencies

If a dependency (e.g. Postgres) supports observability, then configure it when that dependency is first introduced. Minimise the dependent component's logging. Always consider whether verbose logging will actually help solve problems in production, if not, just stick with spans and metrics. If in doubt, have a conversation with the user.
