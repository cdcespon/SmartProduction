using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class SupplierService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SupplierService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Suppliers.ToListAsync();
    }

    public async Task SaveSupplierAsync(Supplier supplier)
    {
        using var context = _contextFactory.CreateDbContext();
        if (supplier.Id == 0)
            context.Suppliers.Add(supplier);
        else
            context.Suppliers.Update(supplier);
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteSupplierAsync(int supplierId)
    {
        using var context = _contextFactory.CreateDbContext();
        var supplier = await context.Suppliers.FindAsync(supplierId);
        if (supplier != null)
        {
            context.Suppliers.Remove(supplier);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<SupplierProduct>> GetSupplierProductsAsync(int supplierId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.SupplierProducts
            .Include(sp => sp.Product)
            .Where(sp => sp.SupplierId == supplierId)
            .ToListAsync();
    }

    public async Task AddProductLinkAsync(SupplierProduct supplierProduct)
    {
        using var context = _contextFactory.CreateDbContext();
        context.SupplierProducts.Add(supplierProduct);
        await context.SaveChangesAsync();
    }

    public async Task RemoveProductLinkAsync(int supplierId, int productId)
    {
        using var context = _contextFactory.CreateDbContext();
        var link = await context.SupplierProducts
            .FirstOrDefaultAsync(sp => sp.SupplierId == supplierId && sp.ProductId == productId);
        if (link != null)
        {
            context.SupplierProducts.Remove(link);
            await context.SaveChangesAsync();
        }
    }
}
