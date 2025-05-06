using Microsoft.EntityFrameworkCore;
using TransactionService.Abstractions;
using TransactionService.Data;
using TransactionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<BankAApiService>();
builder.Services.AddScoped<BankBApiService>();
builder.Services.AddScoped<Func<string, IBankApiService>>(provider => key =>
{
    return key switch
    {
        "BankA" => provider.GetRequiredService<BankAApiService>(),
        "BankB" => provider.GetRequiredService<BankBApiService>(),
        _ => throw new KeyNotFoundException()
    };
});

builder.Services.AddHttpClient<BankAApiService>("BankAApi", client => client.BaseAddress = new Uri("https://localhost:7188/api/"));
builder.Services.AddHttpClient<BankBApiService>("BankBApi", client => client.BaseAddress = new Uri("https://localhost:7094/api/"));

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();