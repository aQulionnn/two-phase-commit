using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<TransactionService>("transactions");
builder.AddProject<BankAService>("bank-a");
builder.AddProject<BankBService>("bank-b");

builder.Build().Run();