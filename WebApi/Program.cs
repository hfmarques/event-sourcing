using EventStore.Client;
using Microsoft.AspNetCore.Mvc;
using WebApi;
var builder = WebApplication.CreateBuilder(args);

const string connectionString = "esdb://localhost:2113?tls=false";
var settings = EventStoreClientSettings.Create(connectionString);

builder.Services.AddSingleton(settings);
builder.Services.AddTransient<EventStoreRepository>();
builder.Services.AddTransient<AccountService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
if (app.Environment.IsDevelopment())
{
    app.Map("/", () => Results.Redirect("/swagger"));
}

app.MapPost("/openAccount", async (
    [FromServices] AccountService accountService, 
    CancellationToken cancellationToken) =>
{
    var account =  await accountService.OpenAccount(Guid.NewGuid(), cancellationToken);
    return Results.Created($"/{account.Id}", account);
});

app.MapGet("/{id:guid}", async (
    [FromServices] AccountService accountService, 
    Guid id, 
    CancellationToken cancellationToken) =>
{
    var account =  await accountService.GetAccount(id, cancellationToken);
    return Results.Ok(account);
});

app.Run();