using Financeiro.Console.Domain.Interfaces;

namespace Financeiro.Console.Domain.Entities;

public class Stock : IStockSubject
{
    private readonly List<IStockObserver> _observers = new();

    public string Symbol { get; }
    public decimal Price { get; private set; }
    public DateTime LastUpdate { get; private set; }

    public Stock(string symbol, decimal initialPrice)
    {
        Symbol = symbol;
        Price = initialPrice;
        LastUpdate = DateTime.Now;
    }

    public void Attach(IStockObserver observer)
    {
        _observers.Add(observer);
    }

    public void Detach(IStockObserver observer)
    {
        _observers.Remove(observer);
    }

    public void Notify(decimal oldPrice)
    {
        foreach (var observer in _observers)
        {
            observer.Update(this, oldPrice);
        }
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (Price == newPrice)
            return;

        var oldPrice = Price;
        Price = newPrice;
        LastUpdate = DateTime.Now;

        decimal changePercent = ((newPrice - oldPrice) / oldPrice) * 100;

        System.Console.WriteLine($"\n[{Symbol}] Preço atualizado: R$ {oldPrice:N2} → R$ {newPrice:N2} ({changePercent:+0.00;-0.00}%)");

        Notify(oldPrice);
    }
}