using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class ProductionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ProductionService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // Work Centers
    public async Task<List<WorkCenter>> GetWorkCentersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.WorkCenters.ToListAsync();
    }

    public async Task SaveWorkCenterAsync(WorkCenter workCenter)
    {
        using var context = _contextFactory.CreateDbContext();
        if (workCenter.Id == 0)
        {
            context.WorkCenters.Add(workCenter);
        }
        else
        {
            context.WorkCenters.Update(workCenter);
        }
        await context.SaveChangesAsync();
    }

    public async Task DeleteWorkCenterAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var item = await context.WorkCenters.FindAsync(id);
        if (item != null)
        {
            context.WorkCenters.Remove(item);
            await context.SaveChangesAsync();
        }
    }

    // Routings
    public async Task<List<Routing>> GetRoutingsForProductAsync(int productId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Routings
            .Include(r => r.Steps)
            .ThenInclude(s => s.WorkCenter)
            .Where(r => r.ProductId == productId)
            .ToListAsync();
    }

    public async Task<Routing?> GetRoutingByIdAsync(int routingId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Routings
            .Include(r => r.Steps)
            .ThenInclude(s => s.WorkCenter)
            .FirstOrDefaultAsync(r => r.Id == routingId);
    }

    public async Task SaveRoutingAsync(Routing routing)
    {
        using var context = _contextFactory.CreateDbContext();
        if (routing.Id == 0)
        {
            context.Routings.Add(routing);
        }
        else
        {
            context.Routings.Update(routing);
        }
        await context.SaveChangesAsync();
    }

    // Routing Steps
    public async Task AddRoutingStepAsync(RoutingStep step)
    {
        using var context = _contextFactory.CreateDbContext();
        context.RoutingSteps.Add(step);
        await context.SaveChangesAsync();
    }

    public async Task DeleteRoutingStepAsync(int stepId)
    {
        using var context = _contextFactory.CreateDbContext();
        var item = await context.RoutingSteps.FindAsync(stepId);
        if (item != null)
        {
            context.RoutingSteps.Remove(item);
            await context.SaveChangesAsync();
        }
    }

    public async Task<decimal> CalculateRoutingCostAsync(int routingId)
    {
        using var context = _contextFactory.CreateDbContext();
        var steps = await context.RoutingSteps
            .Include(s => s.WorkCenter)
            .Where(s => s.RoutingId == routingId)
            .ToListAsync();

        decimal totalCost = 0;
        foreach (var step in steps)
        {
            if (step.WorkCenter != null)
            {
                // Costo = (Setup + RunTime) * (CostoHora / 60)
                // Nota: Setup se prorratea o se cobra por lote? Aquí asumiremos por unidad para simplificar o lote=1
                // En un sistema real, Setup es fijo por lote y RunTime es por unidad.
                // Asumiremos que el cálculo es unitario y el setup se diluye (o es cero en este contexto simple).
                // Vamos a sumar minuros totales por unidad.
                
                var totalMinutes = step.SetupTimeMinutes + step.RunTimeMinutes; 
                var costPerMinute = step.WorkCenter.CostPerHour / 60m;
                totalCost += (decimal)totalMinutes * costPerMinute;
            }
        }
        return totalCost;
    }

    // Work Orders
    public async Task<List<WorkOrder>> GetWorkOrdersAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.WorkOrders
            .Include(wo => wo.Product)
            .OrderByDescending(wo => wo.CreatedDate)
            .ToListAsync();
    }

    public async Task<WorkOrder?> GetWorkOrderByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.WorkOrders
            .Include(wo => wo.Product)
            .Include(wo => wo.Routing)
            .FirstOrDefaultAsync(wo => wo.Id == id);
    }

    public async Task SaveWorkOrderAsync(WorkOrder workOrder)
    {
        using var context = _contextFactory.CreateDbContext();
        
        // Si no tiene número, generar uno automático
        if (string.IsNullOrEmpty(workOrder.OrderNumber))
        {
            workOrder.OrderNumber = $"WO-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
        }
        
        if (workOrder.Id == 0)
        {
            context.WorkOrders.Add(workOrder);
        }
        else
        {
            context.WorkOrders.Update(workOrder);
        }
        await context.SaveChangesAsync();
    }

    public async Task DeleteWorkOrderAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var item = await context.WorkOrders.FindAsync(id);
        if (item != null)
        {
            context.WorkOrders.Remove(item);
            await context.SaveChangesAsync();
        }
    }
}
