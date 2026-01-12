using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class SmartAssistantService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    
    public SmartAssistantService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<string> ProcessQueryAsync(string userQuery)
    {
        using var context = _contextFactory.CreateDbContext();
        userQuery = userQuery.ToLower().Trim();

        // 1. Intent: Consultar Stock ("cuanto tengo de X", "stock de Y")
        if (Regex.IsMatch(userQuery, @"(cuánto|cuanto|stock|inventario).*(de|del) (.*)"))
        {
            var match = Regex.Match(userQuery, @"(cuánto|cuanto|stock|inventario).*(de|del) (.*)");
            string productName = match.Groups[3].Value.Trim().TrimEnd('?');
            
            var item = await context.InventoryItems
                .Include(i => i.Product)
                .ThenInclude(p => p.UnitOfMeasure)
                .FirstOrDefaultAsync(i => EF.Functions.Like(i.Product.Name, $"%{productName}%"));

            if (item != null)
            {
                return $"El stock actual de **{item.Product.Name}** es de **{item.QuantityOnHand:N2} {item.Product.UnitOfMeasure.Abbreviation}**. (Stock Seguridad: {item.SafetyStock:N2})";
            }
            return $"Lo siento, no encontré ningún producto que coincida con '{productName}'.";
        }

        // 2. Intent: Estado de Órdenes ("órdenes atrasadas", "estado de orden X")
        if (userQuery.Contains("atrasada") || userQuery.Contains("retrasada"))
        {
            var delayedOrders = await context.WorkOrders
                .Include(wo => wo.Product)
                .Where(wo => wo.Status != WorkOrderStatus.Completed && wo.Status != WorkOrderStatus.Cancelled && wo.DueDate < DateTime.Today)
                .ToListAsync();

            if (delayedOrders.Any())
            {
                var msg = "⚠️ **Atención:** Hay órdenes atrasadas:\n";
                foreach (var order in delayedOrders)
                {
                    msg += $"- **{order.OrderNumber}** ({order.Product.Name}): Vencía el {order.DueDate:d}\n";
                }
                return msg;
            }
            return "✅ No hay órdenes atrasadas. ¡Todo marcha bien!";
        }

        if (userQuery.Contains("estado de orden") || userQuery.Contains("buscar orden"))
        {
            // Intentar extraer numero
            var words = userQuery.Split(' ');
            var orderNum = words.Last(); // Asumimos que el numero es la ultima palabra
            
            var order = await context.WorkOrders
                .Include(wo => wo.Product)
                .FirstOrDefaultAsync(wo => EF.Functions.Like(wo.OrderNumber, $"%{orderNum}%"));

            if (order != null)
            {
                return $"La orden **{order.OrderNumber}** para {order.Product.Name} está en estado **{order.Status}**. Fecha entrega: {order.DueDate:d}";
            }
        }
        
        // 3. Intent: Sugerencias de Compra (MRP)
        if (userQuery.Contains("comprar") || userQuery.Contains("falta"))
        {
            var reqs = await context.MaterialRequirements
                .Include(r => r.Product)
                .ThenInclude(p => p.UnitOfMeasure)
                .Where(r => r.Type == RequirementType.Purchase && !r.IsProcessed)
                .ToListAsync();
                
            if (reqs.Any())
            {
                var msg = "🛒 **Sugerencias de Compra (según MRP):**\n";
                foreach (var req in reqs.Take(5)) // Top 5
                {
                    msg += $"- {req.RequiredQuantity:N0} {req.Product.UnitOfMeasure.Abbreviation} de **{req.Product.Name}** (Para: {req.Reference})\n";
                }
                if (reqs.Count > 5) msg += $"... y {reqs.Count - 5} más.";
                return msg;
            }
            return "Según el MRP, no hay urgencias de compra pendientes.";
        }

        // Default Help
        return @" **Soy tu Copilot de Producción.**
Puedo ayudarte con consultas como:
- *¿Cuánto stock tengo de Resistencia?*
- *¿Qué órdenes están atrasadas?*
- *¿Qué debo comprar?*";
    }
}
