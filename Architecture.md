# Architecture

## **Modular monolith** using **vertical slices**

Each feature owns its own request, handler, and response. No cross-cutting service layers.

- Orchestrated via **.NET Aspire** (`src/AppHost`).
- Frontend SPAs: `adminweb` and `memberweb` (VITE + React + TypeScript).

## Domain Events

When different modules need cross-communication, we will be publishing domain events to which other modules can subscribe. We will be using the outbox and inbox patterns respectively.

### Outbox pattern

- Instead of the publisher saving something to a database _then_ publishing an event, the publisher will save outbound messages to the database and commit the entire thing in a transaction.
- A background worker will periodically poll the table for outbound messages then send them via the publishing mechanism and mark them as sent.
- Eventually we will consider a clean-up job that will move old messages somewhere else.
- Messages should always be published with a correlation Id that will not change. That way subscribers can make idempotency checks.

### Inbox pattern

- When a subscriber receives a message it will store the message in a pending messages table in its database.
- If the subscriber has received the message before (due to a matching correlation Id) it will silently ignore that message.
- A background service will periodically check for pending inbound messages, process them and mark them as complete.
- Eventually we will consider a clean-up job that will move old inbound messages somewhere else.
