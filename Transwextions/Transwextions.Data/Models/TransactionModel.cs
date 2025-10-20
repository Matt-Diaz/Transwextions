using System.ComponentModel.DataAnnotations;

namespace Transwextions.Data.Models;

public class TransactionModel
{
    public int Id { get; set; }

    [StringLength(50, MinimumLength = 1)]
    [Required]
    public string Description { get; set; } = string.Empty;

    public ulong AmountTotalCents { get; set; }
    public DateTime TransactionDateUtc { get; set; }
    public Guid? UniqueIdentifier { get; set; }
    public bool IsDeleted { get; set; }
}