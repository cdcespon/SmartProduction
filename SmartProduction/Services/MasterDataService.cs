using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class MasterDataService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MasterDataService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<UnitOfMeasure>> GetUnitsOfMeasureAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.UnitsOfMeasure.ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Categories.ToListAsync();
    }
}
