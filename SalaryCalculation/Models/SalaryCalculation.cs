using System.ComponentModel.DataAnnotations;

namespace SalaryCalculation.Models
{
    public class SalaryCalculationRecord
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public decimal GrossSalary { get; set; }
        
        [Required]
        public bool IsSalaryGross { get; set; } // Brütten Nete or Netten Brüte
        
        [Required]
        public int Year { get; set; }
        
        [Required]
        public int StartMonth { get; set; } // 1 for January, 2 for February, etc.
        
        public bool ShowEmployerCost { get; set; }
        
        public DateTime CalculationDate { get; set; }
        
        // Navigation property
        public virtual ICollection<SalaryDetail> SalaryDetails { get; set; } = new List<SalaryDetail>();
    }
} 