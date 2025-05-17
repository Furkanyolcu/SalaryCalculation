using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SalaryCalculation.Models
{
    public static class SeedData
    {
        public static void Initialize(SalaryCalculationContext context, ILogger logger = null)
        {
            if (logger != null)
            {
                logger.LogInformation("SeedData Initialize method called");
            }
            
            try
            {
                // Try to fetch data first to see if we can connect
                var testQuery = context.Database.CanConnect();
                if (logger != null)
                {
                    logger.LogInformation($"Can connect to database: {testQuery}");
                }
                
                if (!testQuery)
                {
                    if (logger != null)
                    {
                        logger.LogError("Cannot connect to database. Check your connection string.");
                    }
                    return;
                }
                
                // Look for existing tax rates
                var anyRates = context.TaxRates.Any();
                if (logger != null)
                {
                    logger.LogInformation($"TaxRates table has data: {anyRates}");
                }
                
                if (anyRates)
                {
                    if (logger != null)
                    {
                        logger.LogInformation("Database has been already seeded, skipping");
                        var years = context.TaxRates.Select(t => t.Year).ToList();
                        logger.LogInformation($"Years in database: {string.Join(", ", years)}");
                    }
                    return; // Database has been seeded
                }

                if (logger != null)
                {
                    logger.LogInformation("Seeding database with tax rates");
                }
                
                // 2022 tax rates for Turkey
                context.TaxRates.Add(new TaxRate
                {
                    Year = 2022,
                    SgkEmployeeRate = 14.0m, // %14
                    SgkEmployerRate = 20.5m, // %20.5
                    UnemploymentEmployeeRate = 1.0m, // %1
                    UnemploymentEmployerRate = 2.0m, // %2
                    StampTaxRate = 0.759m, // %0.759
                    
                    // 2022 Income Tax Brackets
                    IncomeTaxBracket1Rate = 15.0m, // %15
                    IncomeTaxBracket1Limit = 32000m,
                    IncomeTaxBracket2Rate = 20.0m, // %20
                    IncomeTaxBracket2Limit = 70000m,
                    IncomeTaxBracket3Rate = 27.0m, // %27
                    IncomeTaxBracket3Limit = 250000m,
                    IncomeTaxBracket4Rate = 35.0m, // %35
                    IncomeTaxBracket5Rate = 40.0m, // %40
                    
                    MinimumWageAmount = 5004m // 2022 minimum wage (gross)
                });
                
                // 2023 tax rates for Turkey
                context.TaxRates.Add(new TaxRate
                {
                    Year = 2023,
                    SgkEmployeeRate = 14.0m, // %14
                    SgkEmployerRate = 20.5m, // %20.5
                    UnemploymentEmployeeRate = 1.0m, // %1
                    UnemploymentEmployerRate = 2.0m, // %2
                    StampTaxRate = 0.759m, // %0.759
                    
                    // 2023 Income Tax Brackets
                    IncomeTaxBracket1Rate = 15.0m, // %15
                    IncomeTaxBracket1Limit = 70000m,
                    IncomeTaxBracket2Rate = 20.0m, // %20
                    IncomeTaxBracket2Limit = 150000m,
                    IncomeTaxBracket3Rate = 27.0m, // %27
                    IncomeTaxBracket3Limit = 550000m,
                    IncomeTaxBracket4Rate = 35.0m, // %35
                    IncomeTaxBracket5Rate = 40.0m, // %40
                    
                    MinimumWageAmount = 10008m // 2023 minimum wage (gross)
                });
                
                // 2024 tax rates for Turkey
                context.TaxRates.Add(new TaxRate
                {
                    Year = 2024,
                    SgkEmployeeRate = 14.0m, // %14
                    SgkEmployerRate = 20.5m, // %20.5
                    UnemploymentEmployeeRate = 1.0m, // %1
                    UnemploymentEmployerRate = 2.0m, // %2
                    StampTaxRate = 0.759m, // %0.759
                    
                    // 2024 Income Tax Brackets
                    IncomeTaxBracket1Rate = 15.0m, // %15
                    IncomeTaxBracket1Limit = 110000m,
                    IncomeTaxBracket2Rate = 20.0m, // %20
                    IncomeTaxBracket2Limit = 230000m,
                    IncomeTaxBracket3Rate = 27.0m, // %27
                    IncomeTaxBracket3Limit = 880000m,
                    IncomeTaxBracket4Rate = 35.0m, // %35
                    IncomeTaxBracket5Rate = 40.0m, // %40
                    
                    MinimumWageAmount = 17002m // 2024 minimum wage (gross)
                });
                
                // 2025 tax rates for Turkey
                context.TaxRates.Add(new TaxRate
                {
                    Year = 2025,
                    SgkEmployeeRate = 14.0m, // %14
                    SgkEmployerRate = 20.5m, // %20.5
                    UnemploymentEmployeeRate = 1.0m, // %1
                    UnemploymentEmployerRate = 2.0m, // %2
                    StampTaxRate = 0.759m, // %0.759
                    
                    // 2025 Income Tax Brackets
                    IncomeTaxBracket1Rate = 15.0m, // %15
                    IncomeTaxBracket1Limit = 150000m,
                    IncomeTaxBracket2Rate = 20.0m, // %20
                    IncomeTaxBracket2Limit = 300000m,
                    IncomeTaxBracket3Rate = 27.0m, // %27
                    IncomeTaxBracket3Limit = 1000000m,
                    IncomeTaxBracket4Rate = 35.0m, // %35
                    IncomeTaxBracket5Rate = 40.0m, // %40
                    
                    MinimumWageAmount = 20000m // 2025 minimum wage (gross)
                });

                var saveResult = context.SaveChanges();
                if (logger != null)
                {
                    logger.LogInformation($"Database successfully seeded with tax rates. SaveChanges result: {saveResult}");
                    
                    // Verify seeding
                    var verifyYears = context.TaxRates.Select(t => t.Year).ToList();
                    logger.LogInformation($"Verified years after seeding: {string.Join(", ", verifyYears)}");
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError(ex, "Error in SeedData.Initialize");
                }
            }
        }
    }
} 