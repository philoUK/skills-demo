using System.Security.Claims;
using System.Text.Json;
using AdminModule.Admin.Ping;
using AdminModule.Administrator.Data;
using AdminModule.Administrator.Domain;
using AdminModule.Administrator.Endpoints;
using AdminModule.Contexts;
using AdminModule.Email;
using AdminModule.Keycloak;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdminModule;

public static class AdminModuleExtensions
{
    public static IHostApplicationBuilder AddAdminModule(this IHostApplicationBuilder builder)
    {
        SetUpOptions(builder);
        SetUpAuthentication(builder);
        SetUpCors(builder);
        SetUpPersistence(builder);

        return builder;
    }

    private static void SetUpOptions(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Keycloak"));
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
        builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection("Api"));
        builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection("Frontend"));
    }

    private static void SetUpCors(IHostApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (builder.Environment.IsDevelopment())
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });
    }

    private static void SetUpPersistence(IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AdminDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("admindb"))
        );

        builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
        builder.Services.AddScoped<IEmailService, SmtpEmailService>();
        builder.Services.AddHostedService<DatabaseMigrationService>();
        builder.Services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>();
    }

    private static void SetUpAuthentication(IHostApplicationBuilder builder)
    {
        var keycloak = builder.Configuration.GetSection("Keycloak").Get<KeycloakOptions>();

        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloak?.Authority;
                options.Audience = keycloak?.Audience;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.MapInboundClaims = false;
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        var logger = ctx
                            .HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("AdminModule.Auth");
                        logger.LogWarning(
                            "JWT authentication failed: {Error}",
                            ctx.Exception.Message
                        );
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async ctx =>
                    {
                        if (ctx.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var realmAccess = ctx.Principal.FindFirst("realm_access");
                            if (realmAccess is not null)
                            {
                                var roles = JsonDocument
                                    .Parse(realmAccess.Value)
                                    .RootElement.GetProperty("roles");
                                foreach (var role in roles.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (roleName is not null)
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }

                            var username = ctx.Principal.FindFirst("preferred_username")?.Value;

                            if (username is null)
                            {
                                ctx.Fail("No preferred_username claim found in token.");
                                return;
                            }

                            var repository =
                                ctx.HttpContext.RequestServices.GetRequiredService<IAdministratorRepository>();
                            var administrator = await repository.GetByEmailAsync(
                                username,
                                ctx.HttpContext.RequestAborted
                            );

                            if (administrator is null || !administrator.IsActive())
                                ctx.Fail("Administrator is not active.");
                        }
                    },
                };
            });

        builder.Services.AddAuthorization();
    }

    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/ping", PingEndpoint.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        app.MapGet("/admin/administrators", List.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        app.MapPost("/admin/administrators/{id}/deactivate", Deactivate.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        app.MapPost("/admin/administrators/{id}/reactivate", Reactivate.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        app.MapPost("/admin/administrators/invite", Invite.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        app.MapPost("/admin/administrators/{id}/resend-invitation", ResendInvitation.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        app.MapDelete("/admin/administrators/{id}/invitation", CancelInvitation.Handle)
            .RequireAuthorization(policy => policy.RequireRole("administrator"));

        app.MapGet("/admin/register/{token}", Register.Handle);

        return app;
    }
}
