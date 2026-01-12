using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class ProductService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ProductService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Products
            .Include(p => p.UnitOfMeasure)
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task SaveProductAsync(Product product)
    {
        using var context = _contextFactory.CreateDbContext();
        if (product.Id == 0)
            context.Products.Add(product);
        else
            context.Products.Update(product);
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(int productId)
    {
        using var context = _contextFactory.CreateDbContext();
        var product = await context.Products.FindAsync(productId);
        if (product != null)
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
    }
}
