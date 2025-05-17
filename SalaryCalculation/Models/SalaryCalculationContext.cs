using Microsoft.EntityFrameworkCore;

namespace SalaryCalculation.Models
{
    public class SalaryCalculationContext : DbContext
    {
        public SalaryCalculationContext(DbContextOptions<SalaryCalculationContext> options) : base(options)
        {
        }

        public DbSet<SalaryCalculationRecord> SalaryCalculations { get; set; }
        public DbSet<SalaryDetail> SalaryDetails { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }
    }
} 