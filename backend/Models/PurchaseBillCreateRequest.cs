using System.ComponentModel.DataAnnotations;

namespace Applican.Api.Models;

public class PurchaseBillCreateRequest
{
    [Required]
    [MaxLength(50)]
    public string BillNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string SupplierName { get; set; } = string.Empty;

    [Required]
    public DateTime BillDate { get; set; }

    [Range(0.01, 999999999)]
    public decimal Amount { get; set; }

    [MaxLength(300)]
    public string? Remarks { get; set; }
}
