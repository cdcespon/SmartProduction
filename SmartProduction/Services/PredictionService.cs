using Microsoft.EntityFrameworkCore;
using SmartProduction.Data;
using SmartProduction.Models;

namespace SmartProduction.Services;

public class PredictionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PredictionService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<PredictionResult> PredictDemandAsync(int productId, int monthsToPredict = 3)
    {
        using var context = _contextFactory.CreateDbContext();
        
        // 1. Obtener Historial
        var history = await context.SalesHistory
            .Where(sh => sh.ProductId == productId)
            .OrderBy(sh => sh.Date)
            .ToListAsync();
            
        var salesData = history
            .Select(h => new SaleDataPoint { Date = h.Date, Quantity = (double)h.QuantitySold })
            .ToList();

        if (salesData.Count < 2)
        {
            return new PredictionResult 
            { 
                ProductId = productId, 
                Status = "Insufficient Data",
                Predictions = new List<PredictionDataPoint>() 
            };
        }

        // 2. Regresión Lineal Simple (Mínimos Cuadrados)
        // X = índice del mes (0, 1, 2...), Y = Quantity
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        int n = salesData.Count;

        for (int i = 0; i < n; i++)
        {
            double x = i;
            double y = salesData[i].Quantity;
            
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        // Pendiente (m) y Ordenada al origen (b) -> y = mx + b
        double m = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        double b = (sumY - m * sumX) / n;

        // 3. Proyectar Futuro
        var predictions = new List<PredictionDataPoint>();
        var lastDate = salesData.Last().Date;

        for (int i = 1; i <= monthsToPredict; i++)
        {
            int futureX = n - 1 + i; // Si ultimo indice fue n-1, siguiente es n, etc.
            double predictedQ = m * futureX + b;

            // Evitar predicciones negativas
            if (predictedQ < 0) predictedQ = 0;

            predictions.Add(new PredictionDataPoint
            {
                Date = lastDate.AddMonths(i),
                Quantity = (decimal)predictedQ
            });
        }

        return new PredictionResult
        {
            ProductId = productId,
            Status = "Success",
            Trend = m > 0 ? "Ascendente" : (m < 0 ? "Descendente" : "Estable"),
            History = salesData,
            Predictions = predictions
        };
    }
}

public class SaleDataPoint
{
    public DateTime Date { get; set; }
    public double Quantity { get; set; }
}

public class PredictionDataPoint
{
    public DateTime Date { get; set; }
    public decimal Quantity { get; set; }
}

public class PredictionResult
{
    public int ProductId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public List<SaleDataPoint> History { get; set; } = new();
    public List<PredictionDataPoint> Predictions { get; set; } = new();
}
