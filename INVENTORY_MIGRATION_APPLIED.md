# Inventory & Stock Movement Tables - Migration Applied ✅

## Migration Applied Successfully

**Migration Name:** `AddInventoryAndStockMovement` (20260502140115)

**Status:** ✅ Applied to database

---

## Tables Created

### 1. **Inventories Table**

Tracks current stock levels of products in each warehouse.

```sql
CREATE TABLE Inventories (
    Id INTEGER PRIMARY KEY,
    ProductId INTEGER NOT NULL,
    WarehouseId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    ReorderLevel INTEGER,
    ReorderQuantity INTEGER,
    LastCountedAt TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    UNIQUE(ProductId, WarehouseId),  -- One inventory per product per warehouse
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT,
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE RESTRICT
);

-- Indexes
CREATE UNIQUE INDEX IX_Inventory_Product_Warehouse 
    ON Inventories(ProductId, WarehouseId);
```

**Purpose:** Maintains real-time stock levels
- **Quantity** - Current stock on hand
- **ReorderLevel** - Alert when stock falls below this
- **ReorderQuantity** - Suggested quantity to reorder
- **LastCountedAt** - When inventory was last physically counted

### 2. **StockMovements Table**

Audit trail of all stock transactions (purchases, sales, damage, adjustments).

```sql
CREATE TABLE StockMovements (
    Id INTEGER PRIMARY KEY,
    ProductId INTEGER NOT NULL,
    WarehouseId INTEGER NOT NULL,
    MovementType TEXT NOT NULL,        -- "IN" or "OUT"
    Quantity INTEGER NOT NULL,
    ReferenceNumber TEXT,              -- PO, SO, Invoice number
    Reason TEXT NOT NULL,              -- Purchase, Sales, Damage, etc.
    Notes TEXT,
    UserId INTEGER,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT,
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Indexes for performance
CREATE INDEX IX_StockMovement_ProductId 
    ON StockMovements(ProductId);

CREATE INDEX IX_StockMovement_WarehouseId 
    ON StockMovements(WarehouseId);

CREATE INDEX IX_StockMovement_CreatedAt 
    ON StockMovements(CreatedAt);

CREATE UNIQUE INDEX IX_StockMovement_Product_Warehouse_Date 
    ON StockMovements(ProductId, WarehouseId, CreatedAt);
```

**Purpose:** Complete history of inventory changes
- **MovementType** - "IN" (stock received) or "OUT" (stock issued)
- **Reason** - Purchase, Sales, Damage, Adjustment, Return, etc.
- **ReferenceNumber** - Links to PO, Sales Order, Invoice
- **UserId** - Who made the movement (optional)

---

## Entity Relationships

```
INVENTORY
├─ ProductId ──────> PRODUCT (Restrict Delete)
├─ WarehouseId ────> WAREHOUSE (Restrict Delete)
└─ Unique Constraint: ProductId + WarehouseId (one inventory per location)

STOCK MOVEMENT
├─ ProductId ──────> PRODUCT (Restrict Delete)
├─ WarehouseId ────> WAREHOUSE (Restrict Delete)
├─ UserId ─────────> USER (Set Null on Delete)
└─ Indexes for fast queries by Product, Warehouse, Date
```

---

## Delete Behaviors

| Relationship | Behavior | Reason |
|-------------|----------|--------|
| Inventory → Product | RESTRICT | Can't delete product if inventory exists |
| Inventory → Warehouse | RESTRICT | Can't delete warehouse if inventory exists |
| StockMovement → Product | RESTRICT | Preserves audit trail (never delete movements) |
| StockMovement → Warehouse | RESTRICT | Preserves audit trail |
| StockMovement → User | SET NULL | User deletion doesn't orphan movements |

---

## Usage Examples

### Recording Stock Receipt (Purchase)

```csharp
// Create inventory if it doesn't exist
var inventory = new Inventory
{
    ProductId = 1,
    WarehouseId = 1,
    Quantity = 100,
    ReorderLevel = 20,
    ReorderQuantity = 50
};
_dbContext.Inventories.Add(inventory);

// Record the stock movement
var movement = new StockMovement
{
    ProductId = 1,
    WarehouseId = 1,
    MovementType = "IN",           // Stock received
    Quantity = 100,
    Reason = "Purchase",
    ReferenceNumber = "PO-2024-001",
    Notes = "Received from supplier ABC Corp",
    UserId = currentUserId,
    CreatedAt = DateTime.Now
};
_dbContext.StockMovements.Add(movement);
await _dbContext.SaveChangesAsync();

// Update inventory
inventory.Quantity += 100;
inventory.UpdatedAt = DateTime.Now;
_dbContext.Inventories.Update(inventory);
await _dbContext.SaveChangesAsync();
```

### Recording Stock Issue (Sales)

```csharp
var movement = new StockMovement
{
    ProductId = 1,
    WarehouseId = 1,
    MovementType = "OUT",          // Stock issued
    Quantity = 25,
    Reason = "Sales",
    ReferenceNumber = "SO-2024-500",
    Notes = "Order shipped to customer",
    UserId = currentUserId,
    CreatedAt = DateTime.Now
};
_dbContext.StockMovements.Add(movement);
await _dbContext.SaveChangesAsync();

// Update inventory
inventory.Quantity -= 25;
inventory.UpdatedAt = DateTime.Now;
_dbContext.Inventories.Update(inventory);
await _dbContext.SaveChangesAsync();
```

### Querying Inventory

```csharp
// Get inventory for a product across all warehouses
var productInventory = await _dbContext.Inventories
    .Include(i => i.Product)
    .Include(i => i.Warehouse)
    .Where(i => i.ProductId == 1)
    .ToListAsync();

// Find low stock items
var lowStockItems = await _dbContext.Inventories
    .Include(i => i.Product)
    .Where(i => i.Quantity <= i.ReorderLevel)
    .ToListAsync();

// Get total stock across all warehouses
var totalStock = await _dbContext.Inventories
    .Where(i => i.ProductId == 1)
    .SumAsync(i => i.Quantity);
```

### Querying Stock Movement History

```csharp
// Get all movements for a product in a warehouse
var movements = await _dbContext.StockMovements
    .Where(sm => sm.ProductId == 1 && sm.WarehouseId == 1)
    .OrderByDescending(sm => sm.CreatedAt)
    .ToListAsync();

// Get movements by date range
var recentMovements = await _dbContext.StockMovements
    .Where(sm => sm.CreatedAt >= DateTime.Now.AddDays(-30))
    .Include(sm => sm.Product)
    .Include(sm => sm.User)
    .ToListAsync();

// Get all inbound movements (purchases)
var inboundStock = await _dbContext.StockMovements
    .Where(sm => sm.MovementType == "IN")
    .GroupBy(sm => sm.ProductId)
    .Select(g => new { ProductId = g.Key, TotalReceived = g.Sum(x => x.Quantity) })
    .ToListAsync();
```

---

## Migration Script

Generated migration file:
- **Up:** Creates Inventories and StockMovements tables with all constraints
- **Down:** Drops both tables if you need to rollback

The migration also recreated the Product table with proper foreign key relationships to Category, Supplier, and Unit.

---

## Models Created

### `Inventory.cs`
- Tracks current stock per product per warehouse
- Includes reorder levels and last count date
- ViewModels for API responses

### `StockMovement.cs`
- Audit log of all stock transactions
- Links to Product, Warehouse, and User
- Includes CreateStockMovementViewModel

---

## Next Steps (Optional)

1. **Create API Controllers** for managing inventory
2. **Implement Low Stock Alerts** - Query items below reorder level
3. **Generate Stock Reports** - Daily/monthly movement summaries
4. **Add Stock Reconciliation** - Inventory count adjustments
5. **Calculate ABC Analysis** - Identify fast/slow moving items

---

## Key Constraints & Validations

✅ **Unique Inventory Records** - Only one inventory per product per warehouse  
✅ **Referential Integrity** - Cannot delete products/warehouses with inventory  
✅ **Audit Trail** - Complete history of all stock movements  
✅ **User Tracking** - Know who recorded each movement  
✅ **Performance Indexes** - Fast queries on product, warehouse, date  
✅ **Soft Deletes Safe** - UserId can be null when user is deleted

---

## Database File

Your SQLite database has been updated with:
- ✅ Inventories table
- ✅ StockMovements table
- ✅ All foreign keys and indexes
- ✅ Constraints and relationships
