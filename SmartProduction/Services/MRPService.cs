using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class MRPService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MRPService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task RunMRPAsync()
    {
        using var context = _contextFactory.CreateDbContext();

        // 1. Limpiar requerimientos anteriores (Simulación completa)
        var existingReqs = await context.MaterialRequirements.ToListAsync();
        context.MaterialRequirements.RemoveRange(existingReqs);
        await context.SaveChangesAsync();

        // 2. Obtener Datos
        var inventory = await context.InventoryItems.ToListAsync();
        var workOrders = await context.WorkOrders
            .Where(wo => wo.Status != WorkOrderStatus.Completed && wo.Status != WorkOrderStatus.Cancelled)
            .ToListAsync();
        
        // Cargar BOMs completos (aplanados o cargarlos bajo demanda, para eficiencia cargamos todo y filtramos en memoria)
        var boms = await context.BOMItems.ToListAsync(); 

        // Diccionario de Inventario Virtual (Snapshot para simulación)
        var virtualInventory = inventory.ToDictionary(i => i.ProductId, i => i.QuantityOnHand);

        // Cola de Demandas (Producto, Cantidad, Fecha, Referencia)
        // Usamos una lista para poder agregar dinámicamente
        var demands = new List<DemandItem>();

        // 3. Cargar Demanda Independiente (Work Orders)
        foreach (var wo in workOrders)
        {
            // Solo procesar si queda pendiente. Asumimos que la cantidad de la WO es lo que falta por entregar.
            // En un sistema real, chequearíamos la cantidad ya producida.
            demands.Add(new DemandItem 
            { 
                ProductId = wo.ProductId, 
                Quantity = wo.Quantity, 
                RequiredDate = wo.StartDate ?? DateTime.Today,
                Reference = $"WO: {wo.OrderNumber}",
                SourceOrderId = wo.Id
            });
        }

        // 4. Procesar Demandas (Iterativo para manejar niveles)
        // Procesamos por fecha para priorizar urgencias en el consumo de stock
        // Nota: Al agregar nuevos items a la lista durante la iteración, usaremos un índice o cola.
        
        int currentIndex = 0;
        
        while (currentIndex < demands.Count)
        {
            var demand = demands[currentIndex];
            currentIndex++;

            // Verificar Stock Disponible
            decimal stockAvailable = 0;
            if (virtualInventory.ContainsKey(demand.ProductId))
            {
                stockAvailable = virtualInventory[demand.ProductId];
            }

            // Cálculo Neto
            // Si hay stock, lo consumimos.
            decimal quantityToTakeFromStock = Math.Min(stockAvailable, demand.Quantity);
            decimal netRequirement = demand.Quantity - quantityToTakeFromStock;

            // Actualizar Inventario Virtual
            if (virtualInventory.ContainsKey(demand.ProductId))
            {
                virtualInventory[demand.ProductId] -= quantityToTakeFromStock;
            }

            // Si hay necesidad neta (Net Requirement > 0), generamos Orden Sugerida (Planeada)
            if (netRequirement > 0)
            {
                // Determinar si es comprado o fabricado
                var productBoms = boms.Where(b => b.ParentProductId == demand.ProductId).ToList();
                bool isManufactured = productBoms.Any();

                var reqType = isManufactured ? RequirementType.Production : RequirementType.Purchase;

                // Crear Registro de Requerimiento (Sugerencia)
                var newReq = new MaterialRequirement
                {
                    ProductId = demand.ProductId,
                    RequiredQuantity = netRequirement,
                    RequiredDate = demand.RequiredDate, // Idealmente restar leadtime aquí
                    Type = reqType,
                    Reference = $"Ref: {demand.Reference}",
                    SourceWorkOrderId = demand.SourceOrderId,
                    IsProcessed = false
                };
                context.MaterialRequirements.Add(newReq);

                // Si es fabricado, EXPLOTAR BOM (Generar demanda dependiente)
                if (isManufactured)
                {
                    foreach (var bomItem in productBoms)
                    {
                        // Cantidad Requerida = (NetPadre * CantidadHijo) * (1 + Merma)
                        decimal childQty = (netRequirement * bomItem.Quantity) * (1 + (bomItem.WastePercentage / 100m));
                        
                        // Agregar a la cola de demandas
                        demands.Add(new DemandItem
                        {
                            ProductId = bomItem.ComponentProductId,
                            Quantity = childQty,
                            RequiredDate = demand.RequiredDate, // Simplificación: Misma fecha. Ideal: Restar LeadTime fabricación.
                            Reference = $"Componente de {demand.Reference}",
                            SourceOrderId = demand.SourceOrderId
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    // Helper class local
    private class DemandItem
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime RequiredDate { get; set; }
        public string Reference { get; set; } = string.Empty;
        public int? SourceOrderId { get; set; }
    }
    
    public async Task<List<MaterialRequirement>> GetRequirementsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.MaterialRequirements
            .Include(r => r.Product)
            .ThenInclude(p => p.UnitOfMeasure)
            .OrderBy(r => r.RequiredDate)
            .ToListAsync();
    }
}
