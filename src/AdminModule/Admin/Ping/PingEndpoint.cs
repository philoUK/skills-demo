using Microsoft.AspNetCore.Http;

namespace AdminModule.Admin.Ping;

internal static class PingEndpoint
{
    internal static IResult Handle() => Results.Ok("pong");
}
