using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class InventoryService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public InventoryService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<InventoryItem>> GetInventoryItemsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        var items = await context.InventoryItems
            .Include(i => i.Product)
            .ThenInclude(p => p.UnitOfMeasure) // Include UoM for display
            .ToListAsync();
            
        // Si hay productos que no están en inventario, deberíamos agregarlos con stock 0 virtualmente o crearlos
        // Para este MVP, vamos a asegurarnos de que todos los productos existan en la tabla inventory
        var allProducts = await context.Products.ToListAsync();
        var missingProducts = allProducts.Where(p => !items.Any(i => i.ProductId == p.Id)).ToList();
        
        if (missingProducts.Any())
        {
            foreach (var p in missingProducts)
            {
                var newItem = new InventoryItem 
                { 
                    ProductId = p.Id, 
                    QuantityOnHand = 0, 
                    SafetyStock = 0 
                };
                context.InventoryItems.Add(newItem);
            }
            await context.SaveChangesAsync();
            
            // Recargar lista actualizada
            return await context.InventoryItems
                .Include(i => i.Product)
                .ThenInclude(p => p.UnitOfMeasure)
                .ToListAsync();
        }

        return items;
    }

    public async Task UpdateStockAsync(int inventoryItemId, decimal newQuantity, decimal newSafetyStock)
    {
        using var context = _contextFactory.CreateDbContext();
        var item = await context.InventoryItems.FindAsync(inventoryItemId);
        if (item != null)
        {
            item.QuantityOnHand = newQuantity;
            item.SafetyStock = newSafetyStock;
            item.LastUpdated = DateTime.Now;
            context.InventoryItems.Update(item);
            await context.SaveChangesAsync();
        }
    }
}
