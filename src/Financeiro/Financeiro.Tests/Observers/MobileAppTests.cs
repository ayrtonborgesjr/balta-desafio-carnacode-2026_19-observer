using System;
using System.IO;
using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Observers;

namespace Financeiro.Tests.Observers;

[Collection("Console Output Tests")]
public class MobileAppTests
{
    [Fact]
    public void MobileApp_Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var userId = "user123";

        // Act
        var mobileApp = new MobileApp(userId);

        // Assert
        Assert.Equal(userId, mobileApp.UserId);
    }

    [Fact]
    public void Update_ShouldSendPushNotification()
    {
        // Arrange
        var mobileApp = new MobileApp("user456");
        var stock = new Stock("AAPL", 100.00m);
        var oldPrice = 100.00m;

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(105.00m);
        mobileApp.Update(stock, oldPrice);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("App user456", output);
        Assert.Contains("📱", output);
        Assert.Contains("Push", output);
        Assert.Contains("AAPL", output);
    }

    [Theory]
    [InlineData(100.00, 110.00, "user1")]
    [InlineData(50.00, 45.00, "user2")]
    [InlineData(200.00, 200.50, "user3")]
    public void Update_ShouldDisplayCorrectPriceAndChange(decimal oldPrice, decimal newPrice, string userId)
    {
        // Arrange
        var mobileApp = new MobileApp(userId);
        var stock = new Stock("TEST", oldPrice);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(newPrice);
        mobileApp.Update(stock, oldPrice);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains($"App {userId}", output);
        Assert.Contains(newPrice.ToString("N2"), output);
    }

    [Fact]
    public void Update_WithPositiveChange_ShouldShowPositivePercentage()
    {
        // Arrange
        var mobileApp = new MobileApp("investor1");
        var stock = new Stock("GOOGL", 100.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(105.00m);
        mobileApp.Update(stock, 100.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("+", output);
        Assert.Contains("5", output); // percentage
    }

    [Fact]
    public void Update_WithNegativeChange_ShouldShowNegativePercentage()
    {
        // Arrange
        var mobileApp = new MobileApp("investor2");
        var stock = new Stock("MSFT", 200.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(190.00m);
        mobileApp.Update(stock, 200.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("-", output);
        Assert.Contains("5", output); // percentage
    }

    [Fact]
    public void Update_AsObserver_ShouldWorkWithStockSubject()
    {
        // Arrange
        var mobileApp = new MobileApp("mobile_user");
        var stock = new Stock("TSLA", 150.00m);
        stock.Attach(mobileApp);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(160.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("App mobile_user", output);
        Assert.Contains("📱", output);
        Assert.Contains("TSLA", output);
    }

    [Fact]
    public void Update_MultipleApps_ShouldEachReceiveNotification()
    {
        // Arrange
        var app1 = new MobileApp("user1");
        var app2 = new MobileApp("user2");
        var app3 = new MobileApp("user3");
        var stock = new Stock("AMZN", 100.00m);

        stock.Attach(app1);
        stock.Attach(app2);
        stock.Attach(app3);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(105.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains("App user1", output);
        Assert.Contains("App user2", output);
        Assert.Contains("App user3", output);
    }

    [Theory]
    [InlineData("alice@email.com")]
    [InlineData("bob_mobile")]
    [InlineData("12345")]
    public void MobileApp_WithDifferentUserIds_ShouldDisplayCorrectly(string userId)
    {
        // Arrange
        var mobileApp = new MobileApp(userId);
        var stock = new Stock("FB", 250.00m);

        using var consoleOutput = new StringWriter();
        System.Console.SetOut(consoleOutput);

        // Act
        stock.UpdatePrice(255.00m);
        mobileApp.Update(stock, 250.00m);

        // Assert
        var output = consoleOutput.ToString();
        Assert.Contains($"App {userId}", output);
    }
}

