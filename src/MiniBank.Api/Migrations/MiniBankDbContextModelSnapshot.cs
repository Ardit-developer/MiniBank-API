using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiniBank.Api.Data;

#nullable disable

namespace MiniBank.Api.Migrations;

[DbContext(typeof(MiniBankDbContext))]
partial class MiniBankDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.20");

        modelBuilder.Entity("MiniBank.Api.Models.Account", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<string>("AccountNumber").IsRequired().HasMaxLength(30).HasColumnType("nvarchar(30)");
            b.Property<decimal>("Balance").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            b.Property<DateTime>("CreatedAt").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property<int>("CustomerId").HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("AccountNumber").IsUnique();
            b.HasIndex("CustomerId");
            b.ToTable("Accounts", t => t.HasCheckConstraint("CK_Accounts_Balance_NonNegative", "[Balance] >= 0"));
        });

        modelBuilder.Entity("MiniBank.Api.Models.BankTransaction", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<decimal>("Amount").HasPrecision(18, 2).HasColumnType("decimal(18,2)");
            b.Property<int>("AccountId").HasColumnType("int");
            b.Property<DateTime>("CreatedAt").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property<string>("Type").IsRequired().HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.HasKey("Id");
            b.HasIndex("AccountId");
            b.ToTable("Transactions", t => t.HasCheckConstraint("CK_Transactions_Amount_Positive", "[Amount] > 0"));
        });

        modelBuilder.Entity("MiniBank.Api.Models.Customer", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<DateTime>("CreatedAt").ValueGeneratedOnAdd().HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property<string>("Email").IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
            b.Property<string>("FirstName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("LastName").IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.ToTable("Customers");
        });

        modelBuilder.Entity("MiniBank.Api.Models.Account", b =>
        {
            b.HasOne("MiniBank.Api.Models.Customer", "Customer")
                .WithMany("Accounts")
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("Customer");
        });

        modelBuilder.Entity("MiniBank.Api.Models.BankTransaction", b =>
        {
            b.HasOne("MiniBank.Api.Models.Account", "Account")
                .WithMany("Transactions")
                .HasForeignKey("AccountId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Account");
        });

        modelBuilder.Entity("MiniBank.Api.Models.Customer", b =>
        {
            b.Navigation("Accounts");
        });
    }
}
