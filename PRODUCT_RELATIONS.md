# Product Entity Relationships Configuration

## Overview

The `Product` entity has relationships with three main entities:
- **Category** - Product belongs to one category
- **Supplier** - Product is supplied by one supplier  
- **Unit** - Product uses one base unit of measurement

## Relationship Diagram

```
┌─────────────────────────────────────────────────────┐
│                 PRODUCT                             │
├─────────────────────────────────────────────────────┤
│ Id (PK)                                             │
│ Name                                                │
│ SKU (Unique)                                        │
│ CategoryId (FK) ──────────> CATEGORY                │
│ SupplierId (FK) ──────────> SUPPLIER                │
│ BaseUnitId (FK) ──────────> UNIT                    │
│ CreatedAt                                           │
└─────────────────────────────────────────────────────┘

Additional:
┌─────────────────────────────────────────────────────┐
│ CATEGORY (Self-Referencing Hierarchy)               │
├─────────────────────────────────────────────────────┤
│ Id (PK)                                             │
│ Name                                                │
│ ParentId (FK) ──────────> CATEGORY (Parent)        │
│ Children ──────────> List<Category>                 │
│ CreatedAt                                           │
└─────────────────────────────────────────────────────┘
```

## OnModelCreating Configuration

### 1. Product → Category (Many-to-One)

```csharp
modelBuilder.Entity<Product>()
    .HasOne(p => p.Category)           // One product has one category
    .WithMany()                         // One category has many products
    .HasForeignKey(p => p.CategoryId)  // Foreign key column
    .OnDelete(DeleteBehavior.Restrict) // Cannot delete category with products
    .IsRequired();                      // CategoryId is required (not nullable)
```

**Behavior:**
- Each product must have a category
- Cannot delete a category if it has products (prevents orphan data)
- Foreign key: `CategoryId` in Products table points to `Category.Id`

### 2. Product → Supplier (Many-to-One)

```csharp
modelBuilder.Entity<Product>()
    .HasOne(p => p.Supplier)
    .WithMany()
    .HasForeignKey(p => p.SupplierId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

**Behavior:**
- Each product must have a supplier
- Cannot delete a supplier if they supply products
- Foreign key: `SupplierId` points to `Supplier.Id`

### 3. Product → Unit (Many-to-One)

```csharp
modelBuilder.Entity<Product>()
    .HasOne(p => p.Unit)
    .WithMany()
    .HasForeignKey(p => p.BaseUnitId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired();
```

**Behavior:**
- Each product has a base unit (kg, liter, pieces, etc.)
- Cannot delete a unit type if products use it
- Foreign key: `BaseUnitId` points to `Unit.Id`

### 4. Category → Category (Self-Referencing)

```csharp
modelBuilder.Entity<Category>()
    .HasOne(c => c.Parent)
    .WithMany(c => c.Children)
    .HasForeignKey(c => c.ParentId)
    .OnDelete(DeleteBehavior.Cascade)
    .IsRequired(false);
```

**Behavior:**
- Creates a hierarchy tree structure (parent-child categories)
- `OnDelete(DeleteBehavior.Cascade)` - Deletes all child categories when parent is deleted
- `IsRequired(false)` - Top-level categories don't need a parent

## Generated Database Schema

```sql
-- Products Table
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    SKU NVARCHAR(50) NOT NULL UNIQUE,
    CategoryId INT NOT NULL,
    SupplierId INT NOT NULL,
    BaseUnitId INT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    FOREIGN KEY (BaseUnitId) REFERENCES Units(Id)
);

-- Categories Table
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    ParentId INT NULL,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (ParentId) REFERENCES Categories(Id)
);

-- Suppliers Table
CREATE TABLE Suppliers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(150) NOT NULL,
    ContactPerson NVARCHAR(MAX),
    Address NVARCHAR(MAX),
    Phone NVARCHAR(MAX),
    Email NVARCHAR(100),
    IsActive BIT NOT NULL
);

-- Units Table
CREATE TABLE Units (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Symbol NVARCHAR(10) NOT NULL,
    Type NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME NOT NULL
);
```

## Usage Examples

### Creating a Product

```csharp
// Get existing references
var category = await _dbContext.Categories.FindAsync(1);
var supplier = await _dbContext.Suppliers.FindAsync(1);
var unit = await _dbContext.Units.FindAsync(1);

// Create product
var product = new Product
{
    Name = "Organic Coffee Beans",
    SKU = "COFFEE-001",
    CategoryId = category.Id,
    SupplierId = supplier.Id,
    BaseUnitId = unit.Id,
    CreatedAt = DateTime.Now
};

_dbContext.Products.Add(product);
await _dbContext.SaveChangesAsync();
```

### Loading with Related Data

```csharp
// Eager loading - Load product with all related entities
var product = await _dbContext.Products
    .Include(p => p.Category)
    .Include(p => p.Supplier)
    .Include(p => p.Unit)
    .FirstOrDefaultAsync(p => p.Id == 1);

// Use the loaded data
Console.WriteLine($"Product: {product.Name}");
Console.WriteLine($"Category: {product.Category.Name}");
Console.WriteLine($"Supplier: {product.Supplier.Name}");
Console.WriteLine($"Unit: {product.Unit.Symbol}");
```

### Querying with Relationships

```csharp
// Get all products in a category
var categoryProducts = await _dbContext.Products
    .Where(p => p.Category.Name == "Electronics")
    .ToListAsync();

// Get all products from a specific supplier
var supplierProducts = await _dbContext.Products
    .Where(p => p.Supplier.Name == "ABC Corp")
    .Include(p => p.Category)
    .ToListAsync();

// Get products by unit type
var kilogramProducts = await _dbContext.Products
    .Where(p => p.Unit.Symbol == "kg")
    .ToListAsync();
```

### Deleting with Constraints

```csharp
// This will FAIL if category has products (DeleteBehavior.Restrict)
var category = await _dbContext.Categories.FindAsync(1);
_dbContext.Categories.Remove(category);
await _dbContext.SaveChangesAsync();  // ❌ Throws exception: "The DELETE statement conflicted..."

// Correct approach: Remove products first
var productsToRemove = await _dbContext.Products
    .Where(p => p.CategoryId == 1)
    .ToListAsync();

_dbContext.Products.RemoveRange(productsToRemove);
_dbContext.Categories.Remove(category);
await _dbContext.SaveChangesAsync();  // ✅ Success
```

## Delete Behaviors Reference

| Behavior | Effect |
|----------|--------|
| `Cascade` | Delete related entities automatically |
| `Restrict` | Prevent deletion if related data exists |
| `SetNull` | Set FK to null (only if nullable) |
| `NoAction` | Leave as-is (database enforces) |

## Indexes for Performance

Added indexes on:
- `Product.SKU` - Unique index (fast lookups)
- `Product.CategoryId` - Foreign key (fast filtering)
- `Product.SupplierId` - Foreign key (fast filtering)
- `Category.Name` - For name searches
- `Supplier.Name` - For name searches

These improve query performance when filtering by these columns.

## Notes

- **Transactional Integrity**: OnDelete behaviors prevent orphan records
- **Cascading Delete**: Category deletion cascades to children (hierarchical)
- **Restrict Behavior**: Product relationships use Restrict to maintain referential integrity
- **Indexes**: Added for commonly queried columns
- **Uniqueness**: SKU is unique per product (prevents duplicates)
