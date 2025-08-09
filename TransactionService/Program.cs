using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TransactionService.Abstractions;
using TransactionService.Configurations;
using TransactionService.Data;
using TransactionService.DelegatingHandlers;
using TransactionService.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = $"{retryAfter.TotalSeconds}";

            var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                context.HttpContext,
                StatusCodes.Status429TooManyRequests,
                "Too Many Requests",
                $"Too many requests. Try again after {retryAfter.TotalSeconds} seconds");

            await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
        }
    };

    options.AddPolicy("", httpContext =>
    {
        string? userId = httpContext.User.FindFirstValue("userId");

        if (!string.IsNullOrEmpty(userId))
        {
            return RateLimitPartition.GetTokenBucketLimiter(
                userId,
                _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 5,
                    TokensPerPeriod = 2,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                });
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

builder.Services.AddTickerQ(options =>
{
    options.SetExceptionHandler<BackgroundJobExceptionHandler>();
    options.AddOperationalStore<TransactionDbContext>(efOptions =>
    {
        efOptions.UseModelCustomizerForMigrations();
        efOptions.CancelMissedTickersOnApplicationRestart();
    });

    options.AddDashboard("/tickerq");
    options.AddDashboardBasicAuth();
});

builder.Services.AddOptions<BankApiSettings>()
    .BindConfiguration(BankApiSettings.SectionName);

builder.Services.AddTransient<LoggingHandler>();
builder.Services.AddTransient<BankApiAuthHandler>();

builder.Services.AddServiceDiscovery(options => options.UseConsul());

HttpRetryStrategyOptions GetSharedRetryOptions() => new()
{
    MaxRetryAttempts = 3,
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,
    Delay = TimeSpan.FromSeconds(1)
};

HttpCircuitBreakerStrategyOptions GetSharedCircuitBreakerOptions() => new()
{
    SamplingDuration = TimeSpan.FromSeconds(10),
    FailureRatio = 0.75,
    MinimumThroughput = 5,
    BreakDuration = TimeSpan.FromSeconds(5)
};

builder.Services.AddHttpClient("BankA", client => client.BaseAddress = new Uri("http://bank-a"))
    .AddServiceDiscovery()
    .AddHttpMessageHandler<LoggingHandler>()
    .AddHttpMessageHandler<BankApiAuthHandler>()
    .AddResilienceHandler("BankAHttpClient", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(5));

        pipeline.AddRetry(GetSharedRetryOptions());

        pipeline.AddCircuitBreaker(GetSharedCircuitBreakerOptions());

        pipeline.AddTimeout(TimeSpan.FromSeconds(1));
    });

builder.Services.AddHttpClient("BankB", client => client.BaseAddress = new Uri("http://bank-b"))
    .AddServiceDiscovery()
    .AddHttpMessageHandler<LoggingHandler>()
    .AddHttpMessageHandler<BankApiAuthHandler>()
    .AddResilienceHandler("BankBHttpClient", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(5));

        pipeline.AddRetry(GetSharedRetryOptions());

        pipeline.AddCircuitBreaker(GetSharedCircuitBreakerOptions());

        pipeline.AddTimeout(TimeSpan.FromSeconds(1));
    });

builder.Services.AddScoped<Func<string, IBankApiService>>(provider => key =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    
    return key switch
    {
        "BankA" => RestService.For<IBankApiService>(httpClientFactory.CreateClient("BankA")),
        "BankB" => RestService.For<IBankApiService>(httpClientFactory.CreateClient("BankB")),
        _ => throw new KeyNotFoundException()
    };
});

builder.Services.AddDbContext<TransactionDbContext>(options => options.UseInMemoryDatabase("Database"));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{   
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run("http://0.0.0.0:8080");

