using Financeiro.Console.Domain.Entities;

namespace Financeiro.Console.Domain.Interfaces;

public interface IStockObserver
{
    void Update(Stock stock, decimal oldPrice);
}