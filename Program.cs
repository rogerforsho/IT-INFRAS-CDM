using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CDM.InventorySystem.Data;
using CDM.InventorySystem.Models;
using CDM.InventorySystem.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<InventoryDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Add services to DI container
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IItemService, ItemService>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Controllers for API endpoints
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Use session
app.UseSession();

app.MapRazorPages();
app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<InventoryDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Ensure database exists and apply migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Applying pending migrations...");
            context.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}

app.Run();

// Add-Migration InitialCreate -Context InventoryDbContext
// Update-Database -Context InventoryDbContext
