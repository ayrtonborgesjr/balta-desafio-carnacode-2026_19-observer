using System;
using System.IO;
using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Observers;

namespace Financeiro.Tests.Observers;

[Collection("Console Output Tests")]
public class TradingBotTests
{
    [Fact]
    public void TradingBot_Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var botName = "AlphaBot";
        var buyThreshold = 5.0m;
        var sellThreshold = 5.0m;

        // Act
        var bot = new TradingBot(botName, buyThreshold, sellThreshold);

        // Assert
        Assert.Equal(botName, bot.BotName);
        Assert.Equal(buyThreshold, bot.BuyThreshold);
        Assert.Equal(sellThreshold, bot.SellThreshold);
    }

    [Fact]
    public void Update_ShouldAnalyzeStock()
    {
        // Arrange
        var bot = new TradingBot("TestBot", 5.0m, 5.0m);
        var stock = new Stock("AAPL", 100.00m);
        var oldPrice = 100.00m;

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(102.00m);
        bot.Update(stock, oldPrice);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Bot TestBot", output);
        Assert.Contains("🤖", output);
        Assert.Contains("Analisando", output);
        Assert.Contains("AAPL", output);
    }

    [Fact]
    public void Update_WhenPriceDropsBelowBuyThreshold_ShouldBuy()
    {
        // Arrange
        var bot = new TradingBot("BuyBot", 5.0m, 5.0m);
        var stock = new Stock("AAPL", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - price drops 6% (below -5%)
        stock.UpdatePrice(94.00m);
        bot.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Bot BuyBot", output);
        Assert.Contains("COMPRANDO", output);
        Assert.Contains("💰", output);
    }

    [Fact]
    public void Update_WhenPriceRisesAboveSellThreshold_ShouldSell()
    {
        // Arrange
        var bot = new TradingBot("SellBot", 5.0m, 5.0m);
        var stock = new Stock("GOOGL", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - price rises 6% (above 5%)
        stock.UpdatePrice(106.00m);
        bot.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Bot SellBot", output);
        Assert.Contains("VENDENDO", output);
        Assert.Contains("💸", output);
    }

    [Theory]
    [InlineData(100.00, 94.00, 5.0, 5.0, true, false)]  // -6% -> Buy
    [InlineData(100.00, 106.00, 5.0, 5.0, false, true)] // +6% -> Sell
    [InlineData(100.00, 95.00, 5.0, 5.0, true, false)] // -5% -> Buy (exactly at threshold)
    [InlineData(100.00, 105.00, 5.0, 5.0, false, true)] // +5% -> Sell (exactly at threshold)
    [InlineData(100.00, 103.00, 5.0, 5.0, false, false)] // +3% -> No action
    [InlineData(100.00, 97.00, 5.0, 5.0, false, false)]  // -3% -> No action
    public void Update_WithDifferentThresholds_ShouldTriggerCorrectAction(
        decimal oldPrice, 
        decimal newPrice, 
        decimal buyThreshold, 
        decimal sellThreshold,
        bool shouldBuy,
        bool shouldSell)
    {
        // Arrange
        var bot = new TradingBot("ActionBot", buyThreshold, sellThreshold);
        var stock = new Stock("TEST", oldPrice);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(newPrice);
        bot.Update(stock, oldPrice);

        // Assert
        var output = consoleOutput.ToString();
        if (shouldBuy)
        {
            Assert.Contains("COMPRANDO", output);
            Assert.DoesNotContain("VENDENDO", output);
        }
        else if (shouldSell)
        {
            Assert.Contains("VENDENDO", output);
            Assert.DoesNotContain("COMPRANDO", output);
        }
        else
        {
            Assert.DoesNotContain("COMPRANDO", output);
            Assert.DoesNotContain("VENDENDO", output);
        }
    }

    [Fact]
    public void Update_WithDifferentBuyAndSellThresholds_ShouldWorkCorrectly()
    {
        // Arrange - asymmetric thresholds
        var bot = new TradingBot("AsymmetricBot", 3.0m, 7.0m);
        var stock = new Stock("MSFT", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - price drops 4% (below -3% buy threshold)
        stock.UpdatePrice(96.00m);
        bot.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("COMPRANDO", output);
    }

    [Fact]
    public void Update_WithHighSellThreshold_ShouldNotSellOnSmallIncrease()
    {
        // Arrange
        var bot = new TradingBot("ConservativeBot", 5.0m, 10.0m);
        var stock = new Stock("TSLA", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - price rises 7% (below 10% sell threshold)
        stock.UpdatePrice(107.00m);
        bot.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.DoesNotContain("VENDENDO", output);
        Assert.DoesNotContain("COMPRANDO", output);
        Assert.Contains("Analisando", output);
    }

    [Fact]
    public void Update_AsObserver_ShouldWorkWithStockSubject()
    {
        // Arrange
        var bot = new TradingBot("ObserverBot", 4.0m, 4.0m);
        var stock = new Stock("NFLX", 200.00m);
        stock.Attach(bot);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(190.00m); // -5% change

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Bot ObserverBot", output);
        Assert.Contains("COMPRANDO", output);
    }

    [Fact]
    public void Update_MultipleBots_ShouldEachAnalyzeIndependently()
    {
        // Arrange
        var aggressiveBot = new TradingBot("AggressiveBot", 2.0m, 2.0m);
        var conservativeBot = new TradingBot("ConservativeBot", 10.0m, 10.0m);
        var stock = new Stock("AMZN", 100.00m);

        stock.Attach(aggressiveBot);
        stock.Attach(conservativeBot);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - price drops 5%
        stock.UpdatePrice(95.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("Bot AggressiveBot", output);
        Assert.Contains("Bot ConservativeBot", output);
        // AggressiveBot should buy (5% > 2%), ConservativeBot should not (5% < 10%)
    }

    [Theory]
    [InlineData("AlphaBot")]
    [InlineData("BetaTrader")]
    [InlineData("ML_Bot_v2")]
    public void TradingBot_WithDifferentNames_ShouldDisplayCorrectly(string botName)
    {
        // Arrange
        var bot = new TradingBot(botName, 5.0m, 5.0m);
        var stock = new Stock("FB", 255.00m); // Stock already at new price

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        bot.Update(stock, 250.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains($"Bot {botName}", output);
    }

    [Fact]
    public void Update_ExactlyAtBuyThreshold_ShouldBuy()
    {
        // Arrange
        var bot = new TradingBot("ThresholdBot", 5.0m, 5.0m);
        var stock = new Stock("TEST", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - exactly -5%
        stock.UpdatePrice(95.00m);
        bot.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("COMPRANDO", output);
        Assert.Contains("Analisando", output);
    }

    [Fact]
    public void Update_ExactlyAtSellThreshold_ShouldSell()
    {
        // Arrange
        var bot = new TradingBot("ThresholdBot", 5.0m, 5.0m);
        var stock = new Stock("TEST", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act - exactly +5%
        stock.UpdatePrice(105.00m);
        bot.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("VENDENDO", output);
        Assert.Contains("Analisando", output);
    }
}



