using Microsoft.EntityFrameworkCore;
using Transwextions.Data.Models;

namespace Transwextions.Data;

public class TranswextionsContext : DbContext
{
    public TranswextionsContext(DbContextOptions<TranswextionsContext> options) : base(options) { }

    public DbSet<TransactionModel> Transactions { get; set; }
}