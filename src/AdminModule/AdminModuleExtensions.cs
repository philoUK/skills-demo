using System.Security.Claims;
using System.Text.Json;
using AdminModule.Admin.Ping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdminModule;

public static class AdminModuleExtensions
{
    public static IServiceCollection AddAdminModule(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Keycloak:Authority"];
                options.Audience = configuration["Keycloak:Audience"];
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("AdminModule.Auth");
                        logger.LogWarning("JWT authentication failed: {Error}", ctx.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var realmAccess = ctx.Principal.FindFirst("realm_access");
                            if (realmAccess is not null)
                            {
                                var roles = JsonDocument.Parse(realmAccess.Value)
                                    .RootElement.GetProperty("roles");
                                foreach (var role in roles.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (roleName is not null)
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/ping", PingEndpoint.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        return app;
    }
}
