using Microsoft.EntityFrameworkCore;
using ims_inv.Models;

namespace ims_inv.Data
{
    public class WebAppDbContext : DbContext
    {
        public WebAppDbContext(DbContextOptions<WebAppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitConversion> UnitConversions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================================
            // PRODUCT RELATIONSHIPS
            // ============================================================================

            // Product -> Category (Many-to-One)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)  // Prevent deleting category if products exist
                .IsRequired();

            // Product -> Supplier (Many-to-One)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Product -> Unit (Many-to-One) - BaseUnit
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Unit)
                .WithMany()
                .HasForeignKey(p => p.BaseUnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // ============================================================================
            // UNIT CONVERSION RELATIONSHIPS
            // ============================================================================

            // UnitConversion -> Product (Many-to-One)
            modelBuilder.Entity<UnitConversion>()
                .HasOne(uc => uc.Product)
                .WithMany()
                .HasForeignKey(uc => uc.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // UnitConversion -> FromUnit (Many-to-One)
            modelBuilder.Entity<UnitConversion>()
                .HasOne(uc => uc.FromUnit)
                .WithMany()
                .HasForeignKey(uc => uc.FromUnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UnitConversion -> ToUnit (Many-to-One)
            modelBuilder.Entity<UnitConversion>()
                .HasOne(uc => uc.ToUnit)
                .WithMany()
                .HasForeignKey(uc => uc.ToUnitId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Unique constraint: Can only have one conversion ratio per product between two units
            modelBuilder.Entity<UnitConversion>()
                .HasIndex(uc => new { uc.ProductId, uc.FromUnitId, uc.ToUnitId })
                .IsUnique()
                .HasDatabaseName("IX_UnitConversion_Product_FromTo");

            // ============================================================================
            // CATEGORY RELATIONSHIPS (Self-referencing for hierarchy)
            // ============================================================================

            // Category -> Parent Category (Self-referencing Many-to-One)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // ============================================================================
            // INVENTORY RELATIONSHIPS
            // ============================================================================

            // Inventory -> Product (Many-to-One)
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Unique constraint: Each product can only have one inventory record
            modelBuilder.Entity<Inventory>()
                .HasIndex(i => i.ProductId)
                .IsUnique()
                .HasDatabaseName("IX_Inventory_ProductId");

            // ============================================================================
            // STOCK MOVEMENT RELATIONSHIPS
            // ============================================================================

            // StockMovement -> Product (Many-to-One)
            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Product)
                .WithMany()
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // StockMovement -> Warehouse (Many-to-One)
            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.Warehouse)
                .WithMany()
                .HasForeignKey(sm => sm.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // StockMovement -> User (Many-to-One, Optional)
            modelBuilder.Entity<StockMovement>()
                .HasOne(sm => sm.User)
                .WithMany()
                .HasForeignKey(sm => sm.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Index for efficient queries
            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.ProductId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.WarehouseId);

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => sm.CreatedAt)
                .HasDatabaseName("IX_StockMovement_CreatedAt");

            modelBuilder.Entity<StockMovement>()
                .HasIndex(sm => new { sm.ProductId, sm.WarehouseId, sm.CreatedAt })
                .HasDatabaseName("IX_StockMovement_Product_Warehouse_Date");

            // ============================================================================
            // PRIMARY KEYS & CONSTRAINTS
            // ============================================================================

            // Product constraints
            modelBuilder.Entity<Product>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Product>()
                .Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            // Category constraints
            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Supplier constraints
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(150);

            modelBuilder.Entity<Supplier>()
                .Property(s => s.Email)
                .HasMaxLength(100);

            // Unit constraints
            modelBuilder.Entity<Unit>()
                .Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Unit>()
                .Property(u => u.Symbol)
                .IsRequired()
                .HasMaxLength(10);

            // Inventory constraints
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Quantity)
                .IsRequired();

            modelBuilder.Entity<Inventory>()
                .Property(i => i.ReorderLevel)
                .IsRequired(false);

            modelBuilder.Entity<Inventory>()
                .Property(i => i.ReorderQuantity)
                .IsRequired(false);

            // StockMovement constraints
            modelBuilder.Entity<StockMovement>()
                .Property(sm => sm.MovementType)
                .IsRequired()
                .HasMaxLength(10);  // "IN" or "OUT"

            modelBuilder.Entity<StockMovement>()
                .Property(sm => sm.Quantity)
                .IsRequired();

            modelBuilder.Entity<StockMovement>()
                .Property(sm => sm.Reason)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<StockMovement>()
                .Property(sm => sm.ReferenceNumber)
                .HasMaxLength(50)
                .IsRequired(false);

            modelBuilder.Entity<StockMovement>()
                .Property(sm => sm.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            // ============================================================================
            // INDEXES (for performance)
            // ============================================================================

            // Product indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();  // SKU should be unique

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SupplierId);

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name);

            modelBuilder.Entity<Supplier>()
                .HasIndex(s => s.Name);
        }
    }
}
