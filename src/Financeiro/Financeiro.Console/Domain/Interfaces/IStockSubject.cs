namespace Financeiro.Console.Domain.Interfaces;

public interface IStockSubject
{
    void Attach(IStockObserver observer);
    void Detach(IStockObserver observer);
    void Notify(decimal oldPrice);
}