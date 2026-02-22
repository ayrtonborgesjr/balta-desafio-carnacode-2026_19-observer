using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Domain.Interfaces;

namespace Financeiro.Console.Observers;

public class MobileApp : IStockObserver
{
    public string UserId { get; }

    public MobileApp(string userId)
    {
        UserId = userId;
    }

    public void Update(Stock stock, decimal oldPrice)
    {
        decimal changePercent = ((stock.Price - oldPrice) / oldPrice) * 100;

        System.Console.WriteLine($"  → [App {UserId}] 📱 Push: {stock.Symbol} R$ {stock.Price:N2} ({changePercent:+0.00;-0.00}%)");
    }
}