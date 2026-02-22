using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Companies")]
public class Company
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    [StringLength(50)]
    public string Status { get; set; }   // ✅ FIXED

    [NotMapped]
    public int GroupId { get; set; } = 1;

    [NotMapped]
    public string WeekStrDay { get; set; }

    [NotMapped]
    public string CompanyName { get; set; }

    [NotMapped]
    public string Address { get; set; }

    [NotMapped]
    public string Phone { get; set; }

    [NotMapped]
    public string Fax { get; set; }

    [StringLength(150)]
    public string Email { get; set; }

    [NotMapped]
    public string IPAddress { get; set; }

    [NotMapped]
    public string ContactPerson { get; set; }

    [NotMapped]
    public DateTime FinancialYearStart { get; set; }

    [NotMapped]
    public DateTime FinancialYearTo { get; set; }

    [NotMapped]
    public string BaseCurrencyInWord { get; set; }

    [NotMapped]
    public string BaseCurrencyInSymbol { get; set; }

    [NotMapped]
    public int ReportingLevelNo { get; set; }

    [NotMapped]
    public string DateTimeFormat { get; set; }

    [NotMapped]
    public string VoucherFormat { get; set; }

    [NotMapped]
    public string CompanyInitial { get; set; }

    [NotMapped]
    public string CompanyLogo { get; set; }

    [NotMapped]
    public DateTime? AccountingBookStartFrom { get; set; }

    [NotMapped]
    public int? BaseCurrencyID { get; set; }

    [NotMapped]
    public string Slogan { get; set; }

    [NotMapped]
    public string ReportSortingOrder { get; set; }
}
