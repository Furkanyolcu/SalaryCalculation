using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalaryCalculation.Models
{
    public class SalaryDetail
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int Month { get; set; } // 1 for January, 2 for February, etc.
        
        [Required]
        public string MonthName { get; set; } = string.Empty;
        
        [Required]
        public int Order { get; set; } // Order in which the month appears in the calculation
        
        [Required]
        public decimal GrossSalary { get; set; }
        
        [Required]
        public decimal SgkEmployeeAmount { get; set; } // SGK İşçi Primi
        
        [Required]
        public decimal UnemploymentEmployeeAmount { get; set; } // İşsizlik İşçi Sigortası
        
        [Required]
        public decimal TaxBase { get; set; } // Vergi Matrahı
        
        [Required]
        public decimal StampTax { get; set; } // Damga Vergisi
        
        [Required]
        public decimal IncomeTax { get; set; } // Gelir Vergisi
        
        [Required]
        public decimal CumulativeIncomeTax { get; set; } // KGVM - Kümülatif Vergi
        
        [Required]
        public decimal MinimumWageTaxDiscount { get; set; } // Asgari Ücret Gelir Vergisi İndirimi
        
        [Required]
        public decimal NetSalary { get; set; } // Net Ücret
        
        // For employer costs (optional display)
        [Required]
        public decimal SgkEmployerAmount { get; set; } // SGK İşveren Primi
        
        [Required]
        public decimal UnemploymentEmployerAmount { get; set; } // İşsizlik İşveren Sigortası
        
        [Required]
        public decimal TotalEmployerCost { get; set; } // Toplam İşveren Maliyeti
        
        // Foreign key
        public int SalaryCalculationId { get; set; }
        
        [ForeignKey("SalaryCalculationId")]
        public virtual SalaryCalculationRecord SalaryCalculation { get; set; } = null!;
    }
} 