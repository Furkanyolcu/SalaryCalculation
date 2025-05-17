using System.ComponentModel.DataAnnotations;

namespace SalaryCalculation.Models
{
    public class TaxRate
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int Year { get; set; }
        
        [Required]
        public decimal SgkEmployeeRate { get; set; } // SGK İşçi Primi Oranı (%)
        
        [Required]
        public decimal SgkEmployerRate { get; set; } // SGK İşveren Primi Oranı (%)
        
        [Required]
        public decimal UnemploymentEmployeeRate { get; set; } // İşsizlik İşçi Sigortası Oranı (%)
        
        [Required]
        public decimal UnemploymentEmployerRate { get; set; } // İşsizlik İşveren Sigortası Oranı (%)
        
        [Required]
        public decimal StampTaxRate { get; set; } // Damga Vergisi Oranı (%)
        
        // Income tax brackets
        [Required]
        public decimal IncomeTaxBracket1Rate { get; set; } // First bracket rate (%)
        
        [Required]
        public decimal IncomeTaxBracket1Limit { get; set; } // First bracket upper limit
        
        [Required]
        public decimal IncomeTaxBracket2Rate { get; set; } // Second bracket rate (%)
        
        [Required]
        public decimal IncomeTaxBracket2Limit { get; set; } // Second bracket upper limit
        
        [Required]
        public decimal IncomeTaxBracket3Rate { get; set; } // Third bracket rate (%)
        
        [Required]
        public decimal IncomeTaxBracket3Limit { get; set; } // Third bracket upper limit
        
        [Required]
        public decimal IncomeTaxBracket4Rate { get; set; } // Fourth bracket rate (%)
        
        [Required]
        public decimal IncomeTaxBracket5Rate { get; set; } // Fifth bracket rate (%)
        
        [Required]
        public decimal MinimumWageAmount { get; set; } // Minimum wage amount for the year
    }
} 