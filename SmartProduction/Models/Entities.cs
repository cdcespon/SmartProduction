using System.ComponentModel.DataAnnotations;

namespace SmartProduction.Models;

public class UnitOfMeasure
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class Product
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty; // Código Interno
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsSubassembly { get; set; }
    
    // Para Costeo
    public decimal StandardCost { get; set; }
    public decimal CurrentCost { get; set; }
    public decimal PurchasePrice { get; set; } // Nuevo campo
    
    // Relaciones
    public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    public ICollection<BOMItem> ParentInBOMs { get; set; } = new List<BOMItem>();
    public ICollection<BOMItem> ComponentsInBOMs { get; set; } = new List<BOMItem>();
}

public class WorkCenter
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public decimal CostPerHour { get; set; }
    public decimal CapacityPerDay { get; set; } = 8; // Horas disponibles por día estandar
    public bool IsActive { get; set; } = true;
}

public class Routing
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public bool IsActive { get; set; } = true;
    
    public ICollection<RoutingStep> Steps { get; set; } = new List<RoutingStep>();
}

public class RoutingStep
{
    public int Id { get; set; }
    public int RoutingId { get; set; }
    public Routing? Routing { get; set; }
    
    public int Sequence { get; set; } // 10, 20, 30
    [Required]
    public string OperationName { get; set; } = string.Empty;
    
    public int WorkCenterId { get; set; }
    public WorkCenter? WorkCenter { get; set; }
    
    public double SetupTimeMinutes { get; set; }
    public double RunTimeMinutes { get; set; } // Por unidad
}

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    
    public ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
}

public class SupplierProduct
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    
    public string SupplierSKU { get; set; } = string.Empty; // Código del Proveedor
    public decimal PurchasePrice { get; set; }
    public int LeadTimeDays { get; set; }
}

public class BOMItem
{
    public int Id { get; set; }
    public int ParentProductId { get; set; }
    public Product? ParentProduct { get; set; }
    public int ComponentProductId { get; set; }
    public Product? ComponentProduct { get; set; }
    public decimal Quantity { get; set; }
    public decimal WastePercentage { get; set; }
}

public enum WorkOrderStatus
{
    Created,
    Released,
    Started,
    Completed,
    Cancelled
}

public class WorkOrder
{
    public int Id { get; set; }
    [Required]
    public string OrderNumber { get; set; } = string.Empty; // Ej: WO-2023-001
    
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal Quantity { get; set; }
    
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Created;
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? StartDate { get; set; } // Fecha inicio planificada
    public DateTime? DueDate { get; set; }   // Fecha fin requerida
    public DateTime? ActualCompletionDate { get; set; }
    
    public int? RoutingId { get; set; }
    public Routing? Routing { get; set; }
    
    public string? Notes { get; set; }
}

public class InventoryItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal QuantityOnHand { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal SafetyStock { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public enum RequirementType
{
    Purchase, // Comprar (Materia Prima)
    Production // Fabricar (Subensamble/Prod Terminado)
}

public class MaterialRequirement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public decimal RequiredQuantity { get; set; }
    public DateTime RequiredDate { get; set; }
    
    public RequirementType Type { get; set; }
    
    public string Reference { get; set; } = string.Empty; // Ej: "WO-2023-001 (Componente)"
    public bool IsProcessed { get; set; } // Si ya se convirtió en Orden
    
    public int? SourceWorkOrderId { get; set; } // Orden que generó la necesidad
}

public class SalesHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public DateTime Date { get; set; }
    public decimal QuantitySold { get; set; }
}
