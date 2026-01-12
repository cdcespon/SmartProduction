using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class BOMService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public BOMService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<BOMItem>> GetBOMForProductAsync(int productId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.BOMItems
            .Include(bi => bi.ComponentProduct)
            .ThenInclude(cp => cp!.UnitOfMeasure)
            .Where(bi => bi.ParentProductId == productId)
            .ToListAsync();
    }

    public async Task AddBOMItemAsync(BOMItem item)
    {
        using var context = _contextFactory.CreateDbContext();
        context.BOMItems.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBOMItemAsync(BOMItem item)
    {
        using var context = _contextFactory.CreateDbContext();
        context.BOMItems.Update(item);
        await context.SaveChangesAsync();
    }

    public async Task DeleteBOMItemAsync(int itemId)
    {
        using var context = _contextFactory.CreateDbContext();
        var item = await context.BOMItems.FindAsync(itemId);
        if (item != null)
        {
            context.BOMItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Calcula el costo total de un producto basado en su BOM recursivamente.
    /// </summary>
    public async Task<decimal> CalculateRollupCostAsync(int productId)
    {
        using var context = _contextFactory.CreateDbContext();
        var product = await context.Products
            .Include(p => p.ParentInBOMs)
            .ThenInclude(bi => bi.ComponentProduct)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return 0;

        decimal totalCost = 0;

        foreach (var item in product.ParentInBOMs)
        {
            decimal componentCost = 0;
            if (item.ComponentProduct!.IsSubassembly)
            {
                componentCost = await CalculateRollupCostAsync(item.ComponentProductId);
            }
            else
            {
                componentCost = item.ComponentProduct.PurchasePrice > 0 
                    ? item.ComponentProduct.PurchasePrice 
                    : item.ComponentProduct.StandardCost;
            }

            // Aplicar cantidad y merma
            var quantityWithWaste = item.Quantity * (1 + (item.WastePercentage / 100));
            totalCost += componentCost * quantityWithWaste;
        }

        return totalCost;
    }
}
