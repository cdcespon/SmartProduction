using SmartProduction.Components;
using Radzen;
using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<MasterDataService>();
builder.Services.AddScoped<BOMService>();
builder.Services.AddScoped<ProductionService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<MRPService>();
builder.Services.AddScoped<PredictionService>();
builder.Services.AddScoped<SmartAssistantService>();
builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
