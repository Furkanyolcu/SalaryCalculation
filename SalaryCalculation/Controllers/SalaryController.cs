using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SalaryCalculation.Models;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SalaryCalculation.Controllers
{
    public class SalaryController : Controller
    {
        private readonly SalaryCalculationContext _context;
        private readonly SalaryCalculatorService _calculatorService;
        private readonly ILogger<SalaryController> _logger;

        public SalaryController(SalaryCalculationContext context, SalaryCalculatorService calculatorService, ILogger<SalaryController> logger)
        {
            _context = context;
            _calculatorService = calculatorService;
            _logger = logger;
        }

        public async Task<IActionResult> Calculate()
        {
            _logger.LogInformation("Calculate GET action called");
            
            var viewModel = new SalaryCalculationViewModel
            {
                Year = DateTime.Now.Year,
                StartMonth = 1 // Set to January as default
            };
            
            try
            {
                // Check database connection first
                bool canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation($"Database connection check: {canConnect}");
                
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database. Check your connection string.");
                    ModelState.AddModelError(string.Empty, "Veritabanına bağlanılamıyor. Lütfen yöneticiniz ile iletişime geçin.");
                    
                    // Set fallback values for dropdowns
                    viewModel.YearOptions = new SelectList(new List<int> { 2024, 2023 });
                    viewModel.MonthOptions = GetMonthSelectList();
                    return View(viewModel);
                }
                
                var years = await _context.TaxRates.Select(t => t.Year).ToListAsync();
                _logger.LogInformation($"Found {years.Count} tax years: {string.Join(", ", years)}");
                
                if (years.Count == 0)
                {
                    _logger.LogWarning("No tax years found in the database. Attempting to seed the database...");
                    
                    // If database is empty, try to seed it
                    try
                    {
                        _logger.LogInformation("Attempting to seed the database with tax rates...");
                        await SeedDatabaseIfEmpty();
                        years = await _context.TaxRates.Select(t => t.Year).ToListAsync();
                        _logger.LogInformation($"After seeding, found {years.Count} tax years");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to seed the database with tax rates");
                    }
                }
                
                // Always provide fallback years if database still has no data
                if (years.Count == 0)
                {
                    _logger.LogWarning("Still no tax data available. Using fallback years.");
                    years = new List<int> { 2024, 2023 };
                    ModelState.AddModelError(string.Empty, "Vergi oranları veritabanında bulunamadı. Lütfen yöneticiniz ile iletişime geçin.");
                }
                
                // Set dropdown values
                viewModel.YearOptions = new SelectList(years.OrderByDescending(y => y));
                viewModel.MonthOptions = GetMonthSelectList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Calculate GET action");
                ModelState.AddModelError(string.Empty, $"Bir hata oluştu: {ex.Message}");
                
                // Provide fallback values
                viewModel.YearOptions = new SelectList(new List<int> { 2024, 2023 });
                viewModel.MonthOptions = GetMonthSelectList();
                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(SalaryCalculationViewModel viewModel)
        {
            _logger.LogInformation("Calculate POST action called");
            _logger.LogInformation($"Received data: Salary={viewModel.Salary}, IsSalaryGross={viewModel.IsSalaryGross}, Year={viewModel.Year}, StartMonth={viewModel.StartMonth}");
            
            // Ensure dropdowns are always populated, regardless of validation state
            try
            {
                var years = await _context.TaxRates.Select(t => t.Year).ToListAsync();
                if (years.Count == 0)
                {
                    // Try seeding if no data
                    await SeedDatabaseIfEmpty();
                    years = await _context.TaxRates.Select(t => t.Year).ToListAsync();
                    
                    // Still fallback if needed
                    if (years.Count == 0)
                    {
                        years = new List<int> { 2024, 2023 };
                    }
                }
                viewModel.YearOptions = new SelectList(years.OrderByDescending(y => y), viewModel.Year);
                viewModel.MonthOptions = GetMonthSelectList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating dropdown lists");
                viewModel.YearOptions = new SelectList(new List<int> { 2024, 2023 }, viewModel.Year);
                viewModel.MonthOptions = GetMonthSelectList();
            }
            
            // Validate required fields manually if needed
            if (viewModel.Salary <= 0)
            {
                ModelState.AddModelError("Salary", "Maaş miktarı sıfırdan büyük olmalıdır.");
            }
            
            if (viewModel.Year <= 0)
            {
                _logger.LogWarning("Year value is invalid, defaulting to current year");
                viewModel.Year = DateTime.Now.Year;
            }
            
            if (viewModel.StartMonth <= 0 || viewModel.StartMonth > 12)
            {
                _logger.LogWarning("StartMonth value is invalid, defaulting to current month");
                viewModel.StartMonth = DateTime.Now.Month;
            }
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning($"Model validation failed: {string.Join(", ", errors)}");
                return View(viewModel);
            }
            
            try
            {
                _logger.LogInformation("Model is valid, proceeding with calculation");
                
                // Check if tax rates exist for the selected year
                var taxRates = await _context.TaxRates.AnyAsync(t => t.Year == viewModel.Year);
                
                if (!taxRates)
                {
                    _logger.LogWarning($"Tax rates for year {viewModel.Year} not found. Trying to seed data...");
                    await SeedDatabaseIfEmpty();
                    
                    // Check again after seeding
                    taxRates = await _context.TaxRates.AnyAsync(t => t.Year == viewModel.Year);
                    
                    if (!taxRates)
                    {
                        _logger.LogError($"Tax rates for year {viewModel.Year} still not found after seeding attempt");
                        ModelState.AddModelError(string.Empty, $"{viewModel.Year} yılı için vergi oranları bulunamadı.");
                        
                        // Fetch available years
                        var years = await _context.TaxRates.Select(t => t.Year).ToListAsync();
                        
                        if (years.Count > 0)
                        {
                            _logger.LogInformation($"Setting year to an available year: {years[0]}");
                            viewModel.Year = years[0]; // Use first available year
                        }
                        else
                        {
                            _logger.LogError("No tax years available in the database");
                            ModelState.AddModelError(string.Empty, "Veritabanında kullanılabilir vergi oranı yılı bulunamadı.");
                            return View(viewModel);
                        }
                    }
                }
                
                var calculationResult = await _calculatorService.CalculateSalary(
                    viewModel.Salary,
                    viewModel.IsSalaryGross,
                    viewModel.Year,
                    viewModel.StartMonth,
                    viewModel.ShowEmployerCost);

                _logger.LogInformation("Calculation completed successfully");
                
                // Save the calculation to the database
                try {
                    _context.SalaryCalculations.Add(calculationResult);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Calculation saved to database with ID: {calculationResult.Id}");
                    viewModel.CalculationResult = calculationResult;
                }
                catch (Exception dbEx) {
                    _logger.LogError(dbEx, "Error saving calculation to database");
                    ModelState.AddModelError(string.Empty, "Hesaplama başarılı ancak veritabanına kaydedilemedi. Sonuçlar geçici olarak gösterilmektedir.");
                    viewModel.CalculationResult = calculationResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during calculation");
                ModelState.AddModelError(string.Empty, $"Hesaplama hatası: {ex.Message}");
            }

            return View(viewModel);
        }

        // Export to Excel
        public async Task<IActionResult> ExportToExcel(int id)
        {
            _logger.LogInformation($"ExportToExcel called for ID: {id}");
            
            var calculation = await _context.SalaryCalculations
                .Include(s => s.SalaryDetails)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (calculation == null)
            {
                _logger.LogWarning($"Calculation with ID {id} not found");
                return NotFound();
            }

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Maaş Hesaplaması");
                    
                    // Add headers
                    worksheet.Cells[1, 1].Value = "Ay";
                    worksheet.Cells[1, 2].Value = "Brüt Ücret";
                    worksheet.Cells[1, 3].Value = "SGK İşçi Primi";
                    worksheet.Cells[1, 4].Value = "İşsizlik İşçi Sigortası";
                    worksheet.Cells[1, 5].Value = "Vergi Matrahı";
                    worksheet.Cells[1, 6].Value = "Damga Vergisi";
                    worksheet.Cells[1, 7].Value = "Gelir Vergisi";
                    worksheet.Cells[1, 8].Value = "KGVM";
                    worksheet.Cells[1, 9].Value = "Asgari Ücret Vergi İndirimi";
                    worksheet.Cells[1, 10].Value = "Net Ücret";
                    
                    if (calculation.ShowEmployerCost)
                    {
                        worksheet.Cells[1, 11].Value = "SGK İşveren Primi";
                        worksheet.Cells[1, 12].Value = "İşsizlik İşveren Sigortası";
                        worksheet.Cells[1, 13].Value = "Toplam İşveren Maliyeti";
                    }
                    
                    // Style the header
                    using (var range = worksheet.Cells[1, 1, 1, calculation.ShowEmployerCost ? 13 : 10])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }
                    
                    // Add data
                    int row = 2;
                    foreach (var detail in calculation.SalaryDetails.OrderBy(d => d.Order))
                    {
                        worksheet.Cells[row, 1].Value = detail.MonthName;
                        worksheet.Cells[row, 2].Value = detail.GrossSalary;
                        worksheet.Cells[row, 3].Value = detail.SgkEmployeeAmount;
                        worksheet.Cells[row, 4].Value = detail.UnemploymentEmployeeAmount;
                        worksheet.Cells[row, 5].Value = detail.TaxBase;
                        worksheet.Cells[row, 6].Value = detail.StampTax;
                        worksheet.Cells[row, 7].Value = detail.IncomeTax;
                        worksheet.Cells[row, 8].Value = detail.CumulativeIncomeTax;
                        worksheet.Cells[row, 9].Value = detail.MinimumWageTaxDiscount;
                        worksheet.Cells[row, 10].Value = detail.NetSalary;
                        
                        if (calculation.ShowEmployerCost)
                        {
                            worksheet.Cells[row, 11].Value = detail.SgkEmployerAmount;
                            worksheet.Cells[row, 12].Value = detail.UnemploymentEmployerAmount;
                            worksheet.Cells[row, 13].Value = detail.TotalEmployerCost;
                        }
                        
                        row++;
                    }
                    
                    // Format currency cells
                    using (var range = worksheet.Cells[2, 2, row - 1, calculation.ShowEmployerCost ? 13 : 10])
                    {
                        range.Style.Numberformat.Format = "#,##0.00";
                    }
                    
                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();
                    
                    // Generate Excel file
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;
                    
                    string fileName = $"Maas_Hesaplama_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    _logger.LogInformation($"Successfully generated Excel file: {fileName}");
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel file");
                TempData["ErrorMessage"] = $"Excel dosyası oluşturulurken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Calculate));
            }
        }

        private SelectList GetMonthSelectList()
        {
            var culture = new CultureInfo("tr-TR");
            var monthNames = Enumerable.Range(1, 12)
                .Select(m => new { Id = m, Name = culture.DateTimeFormat.GetMonthName(m) })
                .ToList();
            
            return new SelectList(monthNames, "Id", "Name");
        }
        
        private async Task SeedDatabaseIfEmpty()
        {
            var anyRates = await _context.TaxRates.AnyAsync();
            if (!anyRates)
            {
                _logger.LogInformation("SeedDatabaseIfEmpty: Tax rates table is empty, seeding with default data...");
                
                // Add 2022 tax rates
                _context.TaxRates.Add(new TaxRate
                {
                    Year = 2022,
                    SgkEmployeeRate = 14.0m,
                    SgkEmployerRate = 20.5m,
                    UnemploymentEmployeeRate = 1.0m,
                    UnemploymentEmployerRate = 2.0m,
                    StampTaxRate = 0.759m,
                    
                    IncomeTaxBracket1Rate = 15.0m,
                    IncomeTaxBracket1Limit = 32000m,
                    IncomeTaxBracket2Rate = 20.0m,
                    IncomeTaxBracket2Limit = 70000m,
                    IncomeTaxBracket3Rate = 27.0m,
                    IncomeTaxBracket3Limit = 250000m,
                    IncomeTaxBracket4Rate = 35.0m,
                    IncomeTaxBracket5Rate = 40.0m,
                    
                    MinimumWageAmount = 5004m
                });

                // 2023 tax rates for Turkey
                _context.TaxRates.Add(new TaxRate
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
                
                // 2024 tax rates for Turkey (based on screenshot)
                _context.TaxRates.Add(new TaxRate
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
                
                // 2025 tax rates (based on screenshot)
                _context.TaxRates.Add(new TaxRate
                {
                    Year = 2025,
                    SgkEmployeeRate = 14.0m,
                    SgkEmployerRate = 20.5m,
                    UnemploymentEmployeeRate = 1.0m,
                    UnemploymentEmployerRate = 2.0m,
                    StampTaxRate = 0.759m,
                    
                    IncomeTaxBracket1Rate = 15.0m,
                    IncomeTaxBracket1Limit = 150000m,
                    IncomeTaxBracket2Rate = 20.0m,
                    IncomeTaxBracket2Limit = 300000m,
                    IncomeTaxBracket3Rate = 27.0m,
                    IncomeTaxBracket3Limit = 1000000m,
                    IncomeTaxBracket4Rate = 35.0m,
                    IncomeTaxBracket5Rate = 40.0m,
                    
                    MinimumWageAmount = 20000m // 2025 minimum wage (gross)
                });
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("SeedDatabaseIfEmpty: Successfully seeded the database with tax rates for years 2022-2025");
            }
            else
            {
                _logger.LogInformation("SeedDatabaseIfEmpty: Tax rates table already has data, checking for missing years...");
                
                // Check if all required years exist, add any missing years
                var existingYears = await _context.TaxRates.Select(t => t.Year).ToListAsync();
                var requiredYears = new List<int> { 2022, 2023, 2024, 2025 };
                
                foreach (var year in requiredYears)
                {
                    if (!existingYears.Contains(year))
                    {
                        _logger.LogInformation($"SeedDatabaseIfEmpty: Adding missing tax rates for year {year}");
                        
                        // Create appropriate tax rate for the missing year
                        var taxRate = new TaxRate
                        {
                            Year = year,
                            SgkEmployeeRate = 14.0m,
                            SgkEmployerRate = 20.5m,
                            UnemploymentEmployeeRate = 1.0m,
                            UnemploymentEmployerRate = 2.0m,
                            StampTaxRate = 0.759m,
                            
                            IncomeTaxBracket1Rate = 15.0m,
                            IncomeTaxBracket2Rate = 20.0m,
                            IncomeTaxBracket3Rate = 27.0m,
                            IncomeTaxBracket4Rate = 35.0m,
                            IncomeTaxBracket5Rate = 40.0m
                        };
                        
                        // Set appropriate limits based on the year
                        switch (year)
                        {
                            case 2022:
                                taxRate.IncomeTaxBracket1Limit = 32000m;
                                taxRate.IncomeTaxBracket2Limit = 70000m;
                                taxRate.IncomeTaxBracket3Limit = 250000m;
                                taxRate.MinimumWageAmount = 5004m;
                                taxRate.StampTaxRate = 0.759m;
                                break;
                            case 2023:
                                taxRate.IncomeTaxBracket1Limit = 70000m;
                                taxRate.IncomeTaxBracket2Limit = 150000m;
                                taxRate.IncomeTaxBracket3Limit = 550000m;
                                taxRate.MinimumWageAmount = 10008m;
                                taxRate.StampTaxRate = 0.759m;
                                break;
                            case 2024:
                                taxRate.IncomeTaxBracket1Limit = 110000m;
                                taxRate.IncomeTaxBracket2Limit = 230000m;
                                taxRate.IncomeTaxBracket3Limit = 880000m;
                                taxRate.MinimumWageAmount = 17002m;
                                taxRate.StampTaxRate = 0.759m;
                                break;
                            case 2025:
                                taxRate.IncomeTaxBracket1Limit = 150000m;
                                taxRate.IncomeTaxBracket2Limit = 300000m;
                                taxRate.IncomeTaxBracket3Limit = 1000000m;
                                taxRate.MinimumWageAmount = 20000m;
                                taxRate.StampTaxRate = 0.759m;
                                break;
                        }
                        
                        _context.TaxRates.Add(taxRate);
                    }
                }
                
                // Save changes if any new rates were added
                if (await _context.SaveChangesAsync() > 0)
                {
                    _logger.LogInformation("SeedDatabaseIfEmpty: Successfully added missing tax rates");
                }
            }
        }
    }
} 