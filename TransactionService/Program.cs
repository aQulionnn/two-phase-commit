using Microsoft.EntityFrameworkCore;
using Refit;
using TransactionService.Abstractions;
using TransactionService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<Func<string, IBankApiService>>(provider => key =>
{
    return key switch
    {
        "BankA" => RestService.For<IBankApiService>("https://localhost:7188/api"),
        "BankB" => RestService.For<IBankApiService>("https://localhost:7094/api"),
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();