using System;
using System.IO;
using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Observers;

namespace Financeiro.Tests.Observers;

[Collection("Console Output Tests")]
public class InvestorTests
{
    [Fact]
    public void Investor_Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var name = "John Doe";
        var threshold = 5.0m;

        // Act
        var investor = new Investor(name, threshold);

        // Assert
        Assert.Equal(name, investor.Name);
        Assert.Equal(threshold, investor.AlertThreshold);
    }

    [Fact]
    public void Update_ShouldReceiveStockUpdate()
    {
        // Arrange
        var investor = new Investor("John Doe", 5.0m);
        var stock = new Stock("AAPL", 110.00m); // Stock already at new price
        var oldPrice = 100.00m;

        // Redirect console output to capture it
        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        investor.Update(stock, oldPrice);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Investidor John Doe", output);
        Assert.Contains("AAPL", output);
    }

    [Theory]
    [InlineData(100.00, 106.00, 5.0, true)]  // 6% change, threshold 5% -> alert
    [InlineData(100.00, 95.00, 5.0, true)]  // -5% change, threshold 5% -> alert (exactly at threshold)
    [InlineData(100.00, 94.00, 5.0, true)]   // -6% change, threshold 5% -> alert
    [InlineData(100.00, 104.00, 5.0, false)] // 4% change, threshold 5% -> no alert
    [InlineData(100.00, 110.00, 10.0, true)] // 10% change, threshold 10% -> alert (exactly at threshold)
    [InlineData(100.00, 111.00, 10.0, true)]  // 11% change, threshold 10% -> alert
    public void Update_WithThreshold_ShouldAlertWhenExceeded(decimal oldPrice, decimal newPrice, decimal threshold, bool shouldAlert)
    {
        // Arrange
        var investor = new Investor("Test Investor", threshold);
        var stock = new Stock("TEST", newPrice); // Stock already at new price

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        investor.Update(stock, oldPrice);

        // Assert
        var output = consoleOutput.ToString();
        if (shouldAlert)
        {
            Assert.Contains("ALERTA!", output);
        }
        else
        {
            Assert.DoesNotContain("ALERTA!", output);
        }
    }

    [Fact]
    public void Update_WithPositiveChange_AboveThreshold_ShouldShowAlert()
    {
        // Arrange
        var investor = new Investor("Jane Smith", 3.0m);
        var stock = new Stock("GOOGL", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(105.00m);
        investor.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Investidor Jane Smith", output);
        Assert.Contains("ALERTA!", output);
        Assert.Contains("3", output); // threshold value
    }

    [Fact]
    public void Update_WithNegativeChange_AboveThreshold_ShouldShowAlert()
    {
        // Arrange
        var investor = new Investor("Bob Wilson", 4.0m);
        var stock = new Stock("MSFT", 200.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(191.00m);
        investor.Update(stock, 200.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Investidor Bob Wilson", output);
        Assert.Contains("ALERTA!", output);
    }

    [Fact]
    public void Update_WithChangeBelowThreshold_ShouldNotShowAlert()
    {
        // Arrange
        var investor = new Investor("Alice Brown", 10.0m);
        var stock = new Stock("TSLA", 150.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - 2.67% change (below 10% threshold)
        stock.UpdatePrice(154.00m);
        investor.Update(stock, 150.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Investidor Alice Brown", output);
        Assert.DoesNotContain("ALERTA!", output);
    }

    [Fact]
    public void Update_AsObserver_ShouldWorkWithStockSubject()
    {
        // Arrange
        var investor = new Investor("Charlie Davis", 2.0m);
        var stock = new Stock("NFLX", 300.00m);
        stock.Attach(investor);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(310.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Investidor Charlie Davis", output);
        Assert.Contains("ALERTA!", output); // 3.33% change > 2% threshold
    }
}

