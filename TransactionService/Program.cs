using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Refit;
using Steeltoe.Common.Http.Discovery;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;
using TransactionService.Abstractions;
using TransactionService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery(options => options.UseConsul());

builder.Services.AddHttpClient("BankA", client => client.BaseAddress = new Uri("http://bank-a"))
    .AddServiceDiscovery()
    .AddResilienceHandler("BankAHttpClient", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(5));

        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromSeconds(1)
        });

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(10),
            FailureRatio = 0.75,
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(5)
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(1));
    });

builder.Services.AddHttpClient("BankB", client => client.BaseAddress = new Uri("http://bank-b"))
    .AddServiceDiscovery()
    .AddResilienceHandler("BankBHttpClient", pipeline =>
    {
        pipeline.AddTimeout(TimeSpan.FromSeconds(5));

        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromSeconds(1)
        });

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(10),
            FailureRatio = 0.75,
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(5)
        });

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

app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();;

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:8080");

