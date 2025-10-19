using Radzen;
using Transwextions.App.Components;
using Transwextions.App.Services;
using Transwextions.App.Services.Interfaces;
using Transwextions.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppDatabase(builder.Environment);


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IUserStateService, UserStateService>();
builder.Services.AddScoped<ITransactionService, TransactionsService>();
builder.Services.AddScoped<IApplicationEventsService, ApplicationEventsService>();

// Don't forget to add an interface for this service
builder.Services.AddScoped<TreasuryReportingRatesService>();


var app = builder.Build();

// Migrate database.
await app.Services.MigrateAndSeedAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();