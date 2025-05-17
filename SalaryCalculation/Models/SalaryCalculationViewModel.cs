using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SalaryCalculation.Models
{
    public class SalaryCalculationViewModel
    {
        public SalaryCalculationViewModel()
        {
            // Initialize with empty collections to prevent null reference exceptions
            YearOptions = new SelectList(new List<int>());
            MonthOptions = new SelectList(new List<object>());
        }

        [Display(Name = "Maaş Türü")]
        public bool IsSalaryGross { get; set; } = true; // Default to Brütten Nete

        [Required(ErrorMessage = "Maaş miktarı gereklidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Maaş miktarı sıfırdan büyük olmalıdır.")]
        [Display(Name = "Maaş Miktarı")]
        [DataType(DataType.Currency)]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Yıl seçimi gereklidir.")]
        [Display(Name = "Yıl")]
        public int Year { get; set; } = DateTime.Now.Year;
        
        public SelectList YearOptions { get; set; }

        [Required(ErrorMessage = "Başlangıç ayı seçimi gereklidir.")]
        [Display(Name = "Başlangıç Ayı")]
        public int StartMonth { get; set; } = 1; // Default to January (month 1)
        
        public SelectList MonthOptions { get; set; }

        [Display(Name = "İşveren maliyeti listelenen tabloda gösterilsin.")]
        public bool ShowEmployerCost { get; set; }

        [Display(Name = "Hesaplanan Maaş Detayları")]
        public SalaryCalculationRecord? CalculationResult { get; set; }

        // Property for currency format
        [Display(Name = "Para Birimi")]
        public string CurrencySymbol { get; set; } = "TL";
    }
} 