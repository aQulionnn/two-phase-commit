using Microsoft.EntityFrameworkCore;
using TransactionService.Data;
using TransactionService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<BankAApiService>(provider =>
{
    var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("BankAApi");
    return new BankAApiService(client);
});

builder.Services.AddScoped<BankBApiService>(provider =>
{
    var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var client = clientFactory.CreateClient("BankBApi");
    return new BankBApiService(client);
});

builder.Services.AddHttpClient("BankAApi", client => client.BaseAddress = new Uri("https://localhost:7188/api"));
builder.Services.AddHttpClient("BankBApi", client => client.BaseAddress = new Uri("https://localhost:7094/api"));

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