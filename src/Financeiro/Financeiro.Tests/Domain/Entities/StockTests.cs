using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Domain.Interfaces;

namespace Financeiro.Tests.Domain.Entities;

[Collection("Console Output Tests")]
public class StockTests
{
    [Fact]
    public void Stock_Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var symbol = "AAPL";
        var initialPrice = 150.50m;

        // Act
        var stock = new Stock(symbol, initialPrice);

        // Assert
        Assert.Equal(symbol, stock.Symbol);
        Assert.Equal(initialPrice, stock.Price);
        Assert.True((DateTime.Now - stock.LastUpdate).TotalSeconds < 1);
    }

    [Fact]
    public void Attach_ShouldAddObserver()
    {
        // Arrange
        var stock = new Stock("AAPL", 150.50m);
        var observer = new TestObserver();

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.Attach(observer);
        stock.UpdatePrice(155.00m);

        // Assert
        Assert.True(observer.WasNotified);
    }

    [Fact]
    public void Detach_ShouldRemoveObserver()
    {
        // Arrange
        var stock = new Stock("AAPL", 150.50m);
        var observer = new TestObserver();
        stock.Attach(observer);

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.Detach(observer);
        observer.WasNotified = false;
        stock.UpdatePrice(155.00m);

        // Assert
        Assert.False(observer.WasNotified);
    }

    [Fact]
    public void UpdatePrice_ShouldUpdatePriceAndNotifyObservers()
    {
        // Arrange
        var stock = new Stock("AAPL", 150.50m);
        var observer = new TestObserver();
        stock.Attach(observer);
        var newPrice = 155.00m;

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(newPrice);

        // Assert
        Assert.Equal(newPrice, stock.Price);
        Assert.True(observer.WasNotified);
        Assert.Equal(150.50m, observer.ReceivedOldPrice);
        Assert.Equal(stock, observer.ReceivedStock);
    }

    [Fact]
    public void UpdatePrice_WithSamePrice_ShouldNotNotifyObservers()
    {
        // Arrange
        var stock = new Stock("AAPL", 150.50m);
        var observer = new TestObserver();
        stock.Attach(observer);

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(150.50m);

        // Assert
        Assert.False(observer.WasNotified);
    }

    [Fact]
    public void UpdatePrice_ShouldUpdateLastUpdateTime()
    {
        // Arrange
        var stock = new Stock("AAPL", 150.50m);
        var initialTime = stock.LastUpdate;
        Thread.Sleep(10);

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(155.00m);

        // Assert
        Assert.True(stock.LastUpdate > initialTime);
    }

    [Fact]
    public void Notify_ShouldNotifyAllAttachedObservers()
    {
        // Arrange
        var stock = new Stock("AAPL", 150.50m);
        var observer1 = new TestObserver();
        var observer2 = new TestObserver();
        var observer3 = new TestObserver();

        stock.Attach(observer1);
        stock.Attach(observer2);
        stock.Attach(observer3);

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(155.00m);

        // Assert
        Assert.True(observer1.WasNotified);
        Assert.True(observer2.WasNotified);
        Assert.True(observer3.WasNotified);
    }

    [Fact]
    public void UpdatePrice_WithMultipleUpdates_ShouldUseCorrectOldPrice()
    {
        // Arrange
        var stock = new Stock("AAPL", 100.00m);
        var observer = new TestObserver();
        stock.Attach(observer);

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act & Assert
        stock.UpdatePrice(110.00m);
        Assert.Equal(100.00m, observer.ReceivedOldPrice);

        stock.UpdatePrice(105.00m);
        Assert.Equal(110.00m, observer.ReceivedOldPrice);

        stock.UpdatePrice(120.00m);
        Assert.Equal(105.00m, observer.ReceivedOldPrice);
    }

    [Theory]
    [InlineData(100.00, 110.00)]
    [InlineData(50.50, 75.25)]
    [InlineData(200.00, 190.00)]
    public void UpdatePrice_WithDifferentPrices_ShouldUpdateCorrectly(decimal initial, decimal newPrice)
    {
        // Arrange
        var stock = new Stock("TEST", initial);
        var observer = new TestObserver();
        stock.Attach(observer);

        using var consoleOutput = new System.IO.StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(newPrice);

        // Assert
        Assert.Equal(newPrice, stock.Price);
        Assert.Equal(initial, observer.ReceivedOldPrice);
        Assert.True(observer.WasNotified);
    }

    // Helper class for testing
    private class TestObserver : IStockObserver
    {
        public bool WasNotified { get; set; }
        public Stock? ReceivedStock { get; set; }
        public decimal ReceivedOldPrice { get; set; }

        public void Update(Stock stock, decimal oldPrice)
        {
            WasNotified = true;
            ReceivedStock = stock;
            ReceivedOldPrice = oldPrice;
        }
    }
}

