# Endpoint Design

## Discovery

Each module should expose a method the containing API Project can reference

```csharp
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/account/ping", PingEndpoint.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        return app;
    }
```

## Location

Endpoints should live in the [aggregate root name]\endpoints folder. For example

- Account
  - Endpoints
    - Create.cs
    - Modify.cs
    - ...

## Contracts

Each module should have a corresponding [ModuleName].Contracts. Each request should have a public class Request class and a public response class (if applicable).

```csharp
public record RegisterPersonRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string FirstName,
    [property: Required] string LastName
);

public record RegisterPersonResponse(
    Guid RegistrationId,
    string InvitationLink
)
```

Use data annotations liberally as we have turned on minimal API validation. That can catch many 400 errors before they ever hit the endpoint. Null checking in the endpoint handler should be considered a code smell, if it can be handled by the type validation. You should still verify through integration tests!

## Responsibilities

The endpoints are orchestrators only, if they are performing data manipulations against domain objects, that is a smell. The domain objects should do it themselves in an extension method. If they are sending emails for example, constructing the email is a smell that should be delegated to the email service.
