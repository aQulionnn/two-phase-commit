using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var consul = builder.AddContainer("consul", "hashicorp/consul")
    .WithContainerName("consul")
    .WithHttpEndpoint(port: 8500, targetPort: 8500);

var bankA = builder.AddProject<BankAService>("bank-a")
    .WaitFor(consul);

var bankB = builder.AddProject<BankBService>("bank-b")
    .WaitFor(consul);

builder.AddProject<TransactionService>("transactions")
    .WaitFor(consul)
    .WithReference(bankA)
    .WithReference(bankB);

builder.Build().Run();

