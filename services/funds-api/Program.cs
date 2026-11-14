using Microsoft.EntityFrameworkCore;
using FundsApi.Data; 
using FundsApi.Hubs; // Προσθήκη για το SignalR
using FundsApi.Filters;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. ΡΥΘΜΙΣΕΙΣ ΥΠΗΡΕΣΙΩΝ (Πριν το builder.Build())
// ============================================================

builder.Services.AddOpenApi();
builder.Services.AddControllers();

// --- ΠΡΟΣΘΗΚΕΣ ΕΔΩ ---
builder.Services.AddHttpClient(); // Διορθώνει το σφάλμα του IHttpClientFactory
builder.Services.AddSignalR();    // Διορθώνει το σφάλμα του IHubContext
// ---------------------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WalletDbContext>(options => 
    options.UseSqlServer(connectionString));
// Register the IdempotencyFilter in the DI container
builder.Services.AddScoped<IdempotencyFilter>(); 
var app = builder.Build();

// ============================================================
// 3. ΡΥΘΜΙΣΗ ΤΟΥ PIPELINE (Μετά το builder.Build())
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


//app.UseHttpsRedirection();

// --- ΠΡΟΣΘΗΚΗ ΕΔΩ ---
app.MapHub<FundsHub>("/fundsHub"); // Δημιουργεί το endpoint για το SignalR
// -------------------

app.MapControllers(); 

app.Run();