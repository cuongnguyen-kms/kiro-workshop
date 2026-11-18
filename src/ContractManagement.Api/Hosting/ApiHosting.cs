using System.Diagnostics;
using System.Text.Json.Serialization;
using ContractManagement.Api.Endpoints;
using ContractManagement.Api.Persistence;
using ContractManagement.Api.Services;
using ContractManagement.Scheduling;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContractManagement.Api.Hosting;

/// <summary>Service registration and middleware of the API (security, errors, CORS, health, startup checks).</summary>
public static class ApiHosting
{
    /// <summary>Name of the one CORS policy.</summary>
    public const string CorsPolicy = "frontend";

    /// <summary>Largest request body the API accepts (NFR6.3).</summary>
    public const int MaxBodyBytes = 256 * 1024;

    /// <summary>Registers every service. Configuration is read lazily so test hosts can override it.</summary>
    public static void AddContractApi(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = MaxBodyBytes);

        if (!builder.Environment.IsDevelopment())
        {
            builder.Logging.AddJsonConsole();
        }

        builder.Services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        // A request that cannot be read (wrong JSON type, bad JSON) goes through the same Problem Details handler.
        builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);

        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();
        builder.Services.AddHttpLogging(o =>
            o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath
                | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration);

        builder.Services.AddDbContextFactory<ContractDbContext>((sp, options) =>
            options.UseNpgsql(DatabaseSettings.BuildConnectionString(sp.GetRequiredService<IConfiguration>())));
        builder.Services.AddSingleton<IScheduleGenerator, ScheduleGenerator>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IContractRepository, ContractRepository>();
        builder.Services.AddScoped<ContractService>();

        builder.Services.AddCors();
        builder.Services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            var origin = configuration["Cors:AllowedOrigin"];
            options.AddPolicy(CorsPolicy, policy =>
            {
                if (!string.IsNullOrWhiteSpace(origin))
                {
                    policy.WithOrigins(origin.TrimEnd('/'));
                }
                policy.WithMethods("GET", "POST").WithHeaders("Content-Type");
            });
        });
    }

    /// <summary>Adds the middleware and maps the endpoints.</summary>
    public static void UseContractApi(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp => errorApp.Run(HandleExceptionAsync));

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.CacheControl = "no-store";
                return Task.CompletedTask;
            });
            await next();
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseHttpLogging();
        app.UseCors(CorsPolicy);

        app.MapContractEndpoints();
        app.MapGet("/health", HealthAsync).ExcludeFromDescription();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
    }

    /// <summary>Applies migrations in Development (creating the database when missing) and only checks the connection elsewhere (BR6.2, NFR4.5).</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        string target;
        try
        {
            target = DatabaseSettings.Describe(DatabaseSettings.BuildConnectionString(app.Configuration));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogCritical("{Message}", ex.Message);
            throw;
        }

        try
        {
            await using var db = await app.Services.GetRequiredService<IDbContextFactory<ContractDbContext>>().CreateDbContextAsync();
            if (app.Environment.IsDevelopment())
            {
                await db.Database.MigrateAsync();
            }
            else if (!await db.Database.CanConnectAsync())
            {
                throw new InvalidOperationException("not reachable");
            }
        }
        catch (Exception ex)
        {
            // The message names host and database only, never the password (BR6.2).
            logger.LogCritical("Cannot start: the database {Target} is not ready ({Reason}).", target, ex.GetType().Name);
            throw new InvalidOperationException($"Cannot start: the database {target} is not ready. See the log.");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ErrorHandler");

        if (exception is BadHttpRequestException bad)
        {
            // The client sent something unreadable or too large; say so without detail (NFR6.3).
            context.Response.StatusCode = bad.StatusCode;
            await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = bad.StatusCode,
                Title = bad.StatusCode == StatusCodes.Status413PayloadTooLarge ? "The request is too large." : "The request could not be read.",
                Extensions = { ["traceId"] = traceId },
            }, options: null, contentType: "application/problem+json");
            return;
        }

        logger.LogError(exception, "Unhandled error, trace {TraceId}", traceId);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Something went wrong.",
            Extensions = { ["traceId"] = traceId },
        }, options: null, contentType: "application/problem+json");
    }

    private static async Task<IResult> HealthAsync(IDbContextFactory<ContractDbContext> factory)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try
        {
            await using var db = await factory.CreateDbContextAsync(timeout.Token);
            await db.Database.ExecuteSqlRawAsync("SELECT 1", timeout.Token);
            return Results.Ok(new { status = "ok" });
        }
        catch (Exception)
        {
            return Results.Json(new { status = "unavailable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
