using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Domain.Interfaces;

namespace Financeiro.Console.Observers;

public class TradingBot : IStockObserver
{
    public string BotName { get; }
    public decimal BuyThreshold { get; }
    public decimal SellThreshold { get; }

    public TradingBot(string botName, decimal buyThreshold, decimal sellThreshold)
    {
        BotName = botName;
        BuyThreshold = buyThreshold;
        SellThreshold = sellThreshold;
    }

    public void Update(Stock stock, decimal oldPrice)
    {
        decimal changePercent = ((stock.Price - oldPrice) / oldPrice) * 100;

        System.Console.WriteLine($"  → [Bot {BotName}] 🤖 Analisando {stock.Symbol}...");

        if (changePercent <= -BuyThreshold)
        {
            System.Console.WriteLine($"  → [Bot {BotName}] 💰 COMPRANDO por R$ {stock.Price:N2}");
        }
        else if (changePercent >= SellThreshold)
        {
            System.Console.WriteLine($"  → [Bot {BotName}] 💸 VENDENDO por R$ {stock.Price:N2}");
        }
    }
}