using System.Text;
using ContractManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Api.Persistence;

/// <summary>EF Core model of the Contract aggregate: snake_case tables, numeric(18,2) money, text enums.</summary>
public sealed class ContractDbContext(DbContextOptions<ContractDbContext> options) : DbContext(options)
{
    /// <summary>The contracts table.</summary>
    public DbSet<Contract> Contracts => Set<Contract>();
    /// <summary>The payment streams table.</summary>
    public DbSet<PaymentStream> PaymentStreams => Set<PaymentStream>();
    /// <summary>The payment schedule rows table.</summary>
    public DbSet<PaymentScheduleEntry> PaymentScheduleEntries => Set<PaymentScheduleEntry>();
    /// <summary>The expense schedule rows table.</summary>
    public DbSet<ExpenseScheduleEntry> ExpenseScheduleEntries => Set<ExpenseScheduleEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(e =>
        {
            e.ToTable("contracts");
            e.HasKey(x => x.ContractId);
            e.Property(x => x.ContractId).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ExpenseMethod).HasConversion<string>().HasMaxLength(20);
            e.HasMany(x => x.PaymentStreams).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.PaymentEntries).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.ExpenseEntries).WithOne().HasForeignKey(x => x.ContractId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.StartDate, x.Name, x.ContractId })
                .IsDescending(true, false, false)
                .HasDatabaseName("ix_contracts_start_date_name_id");
        });

        modelBuilder.Entity<PaymentStream>(e =>
        {
            e.ToTable("payment_streams");
            e.HasKey(x => x.StreamId);
            e.Property(x => x.StreamId).ValueGeneratedNever();
            e.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            e.Property(x => x.Frequency).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.ContractId).HasDatabaseName("ix_payment_streams_contract_id");
        });

        modelBuilder.Entity<PaymentScheduleEntry>(e =>
        {
            e.ToTable("payment_schedule_entries");
            e.HasKey(x => x.PaymentEntryId);
            e.Property(x => x.PaymentEntryId).ValueGeneratedNever();
            e.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            e.HasOne<PaymentStream>().WithMany().HasForeignKey(x => x.StreamId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ContractId, x.DueDate }).HasDatabaseName("ix_payment_schedule_entries_contract_id_due_date");
        });

        modelBuilder.Entity<ExpenseScheduleEntry>(e =>
        {
            e.ToTable("expense_schedule_entries");
            e.HasKey(x => x.ExpenseEntryId);
            e.Property(x => x.ExpenseEntryId).ValueGeneratedNever();
            e.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            e.HasIndex(x => new { x.ContractId, x.PeriodStart }).HasDatabaseName("ix_expense_schedule_entries_contract_id_period_start");
        });

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
