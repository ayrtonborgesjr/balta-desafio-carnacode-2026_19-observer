using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Domain.Interfaces;

namespace Financeiro.Console.Observers;

public class Investor : IStockObserver
{
    public string Name { get; }
    public decimal AlertThreshold { get; }

    public Investor(string name, decimal alertThreshold)
    {
        Name = name;
        AlertThreshold = alertThreshold;
    }

    public void Update(Stock stock, decimal oldPrice)
    {
        decimal changePercent = ((stock.Price - oldPrice) / oldPrice) * 100;

        System.Console.WriteLine($"  → [Investidor {Name}] Notificado sobre {stock.Symbol}");

        if (Math.Abs(changePercent) >= AlertThreshold)
        {
            System.Console.WriteLine($"  → [Investidor {Name}] ⚠️ ALERTA! Mudança de {changePercent:+0.00;-0.00}% excedeu {AlertThreshold}%");
        }
    }
}