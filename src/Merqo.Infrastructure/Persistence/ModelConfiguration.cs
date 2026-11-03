using Merqo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Explicit relational configuration for the business schema.
/// Design rules encoded here:
///  • every foreign key is RESTRICT — a historical document can never be orphaned or cascade-deleted;
///  • enums are stored as readable strings so hand-written reports and forensics stay legible;
///  • every lookup column that the UI searches has an index;
///  • document numbers and idempotency keys are unique at the database level, not just in code.
/// </summary>
public static class ModelConfiguration
{
    public static void ConfigureBusinessModel(ModelBuilder modelBuilder)
    {
        ConfigureParties(modelBuilder);
        ConfigureSales(modelBuilder);
        ConfigurePurchases(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureAccounting(modelBuilder);
        ConfigureSecurity(modelBuilder);
        ConfigureLicensing(modelBuilder);

        // Uniform referential policy: nothing cascades. Deletion of history is a product-level decision,
        // and the application archives or voids instead of deleting.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }

    private static void ConfigureParties(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Address).HasMaxLength(500);
            e.HasIndex(x => x.Phone);
            e.HasIndex(x => x.Name);
            e.HasQueryFilter(x => !x.IsArchived);
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.ToTable("suppliers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.HasIndex(x => x.Name);
            e.HasQueryFilter(x => !x.IsArchived);
        });

        modelBuilder.Entity<CustomerLedgerEntry>(e =>
        {
            e.ToTable("customer_ledger_entries");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Customer).WithMany(c => c.Ledger).HasForeignKey(x => x.CustomerId);
            e.HasIndex(x => new { x.CustomerId, x.EntryDateUtc });
            e.HasIndex(x => x.ReferenceId);
        });

        modelBuilder.Entity<SupplierLedgerEntry>(e =>
        {
            e.ToTable("supplier_ledger_entries");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Supplier).WithMany(s => s.Ledger).HasForeignKey(x => x.SupplierId);
            e.HasIndex(x => new { x.SupplierId, x.EntryDateUtc });
            e.HasIndex(x => x.ReferenceId);
        });

        modelBuilder.Entity<CustomerPayment>(e =>
        {
            e.ToTable("customer_payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.ReceiptNumber).IsRequired().HasMaxLength(60);
            e.Property(x => x.MethodNameSnapshot).IsRequired().HasMaxLength(80);
            e.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(80);
            e.HasIndex(x => x.ReceiptNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => new { x.CustomerId, x.PaidAtUtc });
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
        });

        modelBuilder.Entity<SupplierPayment>(e =>
        {
            e.ToTable("supplier_payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.VoucherNumber).IsRequired().HasMaxLength(60);
            e.Property(x => x.MethodNameSnapshot).IsRequired().HasMaxLength(80);
            e.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(80);
            e.HasIndex(x => x.VoucherNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => new { x.SupplierId, x.PaidAtUtc });
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);
        });

        modelBuilder.Entity<PaymentMethod>(e =>
        {
            e.ToTable("payment_methods");
            e.HasKey(x => x.Id);
            e.Property(x => x.NameBn).IsRequired().HasMaxLength(80);
            e.Property(x => x.CashAccountCode).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.NameBn).IsUnique();
        });
    }

    private static void ConfigureSales(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sale>(e =>
        {
            e.ToTable("sales");
            e.HasKey(x => x.Id);
            e.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(60);
            e.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(80);
            e.Property(x => x.CustomerNameSnapshot).HasMaxLength(200);
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => x.SaleDateUtc);
            e.HasIndex(x => new { x.CustomerId, x.SaleDateUtc });
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
        });

        modelBuilder.Entity<SaleItem>(e =>
        {
            e.ToTable("sale_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(300);
            e.HasOne(x => x.Sale).WithMany(s => s.Items).HasForeignKey(x => x.SaleId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasIndex(x => x.SaleId);
            e.HasIndex(x => x.ProductId);
        });

        modelBuilder.Entity<SalePayment>(e =>
        {
            e.ToTable("sale_payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.MethodNameSnapshot).IsRequired().HasMaxLength(80);
            e.HasOne(x => x.Sale).WithMany(s => s.Payments).HasForeignKey(x => x.SaleId);
            e.HasIndex(x => x.SaleId);
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
        });

        modelBuilder.Entity<SaleReturn>(e =>
        {
            e.ToTable("sale_returns");
            e.HasKey(x => x.Id);
            e.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(60);
            e.HasIndex(x => x.ReturnNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasOne(x => x.Sale).WithMany(s => s.Returns).HasForeignKey(x => x.SaleId);
            e.HasIndex(x => x.ReturnDateUtc);
        });

        modelBuilder.Entity<SaleReturnItem>(e =>
        {
            e.ToTable("sale_return_items");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.SaleReturn).WithMany(r => r.Items).HasForeignKey(x => x.SaleReturnId);
            e.HasOne(x => x.SaleItem).WithMany().HasForeignKey(x => x.SaleItemId);
            e.HasIndex(x => x.SaleReturnId);
            e.HasIndex(x => x.SaleItemId);
        });
    }

    private static void ConfigurePurchases(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Purchase>(e =>
        {
            e.ToTable("purchases");
            e.HasKey(x => x.Id);
            e.Property(x => x.PurchaseNumber).IsRequired().HasMaxLength(60);
            e.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(80);
            e.HasIndex(x => x.PurchaseNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => x.PurchaseDateUtc);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId);
        });

        modelBuilder.Entity<PurchaseItem>(e =>
        {
            e.ToTable("purchase_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(300);
            e.HasOne(x => x.Purchase).WithMany(p => p.Items).HasForeignKey(x => x.PurchaseId);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasIndex(x => x.PurchaseId);
            e.HasIndex(x => x.ProductId);
        });

        modelBuilder.Entity<PurchasePayment>(e =>
        {
            e.ToTable("purchase_payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.MethodNameSnapshot).IsRequired().HasMaxLength(80);
            e.HasOne(x => x.Purchase).WithMany(p => p.Payments).HasForeignKey(x => x.PurchaseId);
            e.HasIndex(x => x.PurchaseId);
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
        });

        modelBuilder.Entity<PurchaseReturn>(e =>
        {
            e.ToTable("purchase_returns");
            e.HasKey(x => x.Id);
            e.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(60);
            e.HasIndex(x => x.ReturnNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasOne(x => x.Purchase).WithMany().HasForeignKey(x => x.PurchaseId);
        });

        modelBuilder.Entity<PurchaseReturnItem>(e =>
        {
            e.ToTable("purchase_return_items");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.PurchaseReturn).WithMany(r => r.Items).HasForeignKey(x => x.PurchaseReturnId);
            e.HasOne(x => x.PurchaseItem).WithMany().HasForeignKey(x => x.PurchaseItemId);
            e.HasIndex(x => x.PurchaseReturnId);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockBalance>(e =>
        {
            e.ToTable("stock_balances");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProductId).IsUnique();
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.Property(x => x.Version).IsConcurrencyToken();
        });

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.ToTable("stock_movements");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasIndex(x => new { x.ProductId, x.MovedAtUtc });
            e.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
            e.HasIndex(x => x.MovedAtUtc);
        });

        modelBuilder.Entity<StockAdjustment>(e =>
        {
            e.ToTable("stock_adjustments");
            e.HasKey(x => x.Id);
            e.Property(x => x.AdjustmentNumber).IsRequired().HasMaxLength(60);
            e.Property(x => x.Reason).IsRequired().HasMaxLength(500);
            e.HasIndex(x => x.AdjustmentNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => x.AdjustmentDateUtc);
        });

        modelBuilder.Entity<StockAdjustmentItem>(e =>
        {
            e.ToTable("stock_adjustment_items");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.StockAdjustment).WithMany(a => a.Items).HasForeignKey(x => x.StockAdjustmentId);
            e.HasIndex(x => x.StockAdjustmentId);
        });
    }

    private static void ConfigureAccounting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(e =>
        {
            e.ToTable("chart_of_accounts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).IsRequired().HasMaxLength(20);
            e.Property(x => x.NameBn).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<JournalEntry>(e =>
        {
            e.ToTable("journal_entries");
            e.HasKey(x => x.Id);
            e.Property(x => x.EntryNumber).IsRequired().HasMaxLength(60);
            e.Property(x => x.Memo).HasMaxLength(500);
            e.HasIndex(x => x.EntryNumber).IsUnique();
            e.HasIndex(x => x.EntryDateUtc);
            e.HasIndex(x => new { x.SourceType, x.SourceId });
        });

        modelBuilder.Entity<JournalEntryLine>(e =>
        {
            e.ToTable("journal_entry_lines");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.JournalEntry).WithMany(j => j.Lines).HasForeignKey(x => x.JournalEntryId);
            e.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId);
            e.Property(x => x.AccountCodeSnapshot).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.JournalEntryId);
            e.HasIndex(x => x.AccountId);
        });

        modelBuilder.Entity<ExpenseCategory>(e =>
        {
            e.ToTable("expense_categories");
            e.HasKey(x => x.Id);
            e.Property(x => x.NameBn).IsRequired().HasMaxLength(120);
            e.Property(x => x.AccountCode).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.NameBn).IsUnique();
            e.HasQueryFilter(x => !x.IsArchived);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.ToTable("expenses");
            e.HasKey(x => x.Id);
            e.Property(x => x.ExpenseNumber).IsRequired().HasMaxLength(60);
            e.HasIndex(x => x.ExpenseNumber).IsUnique();
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => x.ExpenseDateUtc);
            e.HasOne(x => x.ExpenseCategory).WithMany().HasForeignKey(x => x.ExpenseCategoryId);
        });
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(60).UseCollation("NOCASE");
            e.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(400);
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(80);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.ToTable("role_permissions");
            e.HasKey(x => x.Id);
            e.Property(x => x.PermissionCode).IsRequired().HasMaxLength(80);
            e.HasOne(x => x.Role).WithMany(r => r.Permissions).HasForeignKey(x => x.RoleId);
            e.HasIndex(x => new { x.RoleId, x.PermissionCode }).IsUnique();
        });

        modelBuilder.Entity<UserRole>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(u => u.Roles).HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Action).IsRequired().HasMaxLength(80);
            e.Property(x => x.EntityType).HasMaxLength(80);
            e.Property(x => x.EntityId).HasMaxLength(80);
            e.Property(x => x.Username).HasMaxLength(60);
            e.Property(x => x.PreviousHash).HasMaxLength(64);
            e.Property(x => x.RowHash).HasMaxLength(64);
            e.HasIndex(x => x.OccurredAtUtc);
            e.HasIndex(x => x.Ordinal).IsUnique();
            e.HasIndex(x => new { x.EntityType, x.EntityId });
        });

        modelBuilder.Entity<LoginAttempt>(e =>
        {
            e.ToTable("login_attempts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(60);
            e.HasIndex(x => x.OccurredAtUtc);
        });

        modelBuilder.Entity<DocumentSequence>(e =>
        {
            e.ToTable("document_sequences");
            e.HasKey(x => x.Id);
            e.Property(x => x.SequenceKey).IsRequired().HasMaxLength(40);
            e.HasIndex(x => new { x.SequenceKey, x.Year, x.Month }).IsUnique();
        });

        modelBuilder.Entity<BackupHistory>(e =>
        {
            e.ToTable("backup_history");
            e.HasKey(x => x.Id);
            e.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
            e.Property(x => x.Sha256).HasMaxLength(64);
            e.HasIndex(x => x.CreatedAtUtc);
        });
    }

    private static void ConfigureLicensing(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LicenseRecord>(e =>
        {
            e.ToTable("license_records");
            e.HasKey(x => x.Id);
            e.Property(x => x.LicenseId).IsRequired().HasMaxLength(80);
            e.Property(x => x.BusinessIdentity).IsRequired().HasMaxLength(64);
            e.HasIndex(x => x.LicenseId).IsUnique();
        });

        modelBuilder.Entity<ActivationRecord>(e =>
        {
            e.ToTable("activation_records");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ActivatedAtUtc);
        });

        modelBuilder.Entity<MachineBinding>(e =>
        {
            e.ToTable("machine_bindings");
            e.HasKey(x => x.Id);
            e.Property(x => x.FingerprintHash).IsRequired().HasMaxLength(64);
            e.HasIndex(x => x.FingerprintHash);
        });

        modelBuilder.Entity<LicenseRecoveryRecord>(e =>
        {
            e.ToTable("license_recovery_records");
            e.HasKey(x => x.Id);
            e.Property(x => x.OriginalLicenseId).IsRequired().HasMaxLength(80);
            e.Property(x => x.ReplacementLicenseId).IsRequired().HasMaxLength(80);
        });
    }
}
