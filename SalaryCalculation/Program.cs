using Microsoft.EntityFrameworkCore;
using SalaryCalculation.Models;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Configure EPPlus to use non-commercial license
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add DbContext
builder.Services.AddDbContext<SalaryCalculationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Salary Calculator Service
builder.Services.AddScoped<SalaryCalculatorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Salary}/{action=Calculate}/{id?}");

// Seed the database with default tax rates
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<SalaryCalculationContext>();
        
        // Ensure database is created
        logger.LogInformation("Ensuring database is created...");
        bool dbCreated = context.Database.EnsureCreated();
        logger.LogInformation($"Database created: {dbCreated}");
        
        // Check if we can connect to the database
        bool canConnect = context.Database.CanConnect();
        logger.LogInformation($"Can connect to database: {canConnect}");
        
        if (!canConnect)
        {
            logger.LogError("Cannot connect to database. Check your connection string.");
        }
        else
        {
            // Migrate database if needed
            try
            {
                logger.LogInformation("Applying database migrations...");
                context.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
            
            // Seed data
            logger.LogInformation("Calling SeedData.Initialize...");
            SeedData.Initialize(context, logger);
            logger.LogInformation("SeedData.Initialize completed");
            
            // Verify data was seeded
            var taxRateCount = context.TaxRates.Count();
            logger.LogInformation($"Database contains {taxRateCount} tax rates after initialization");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

app.Run();
