using Financeiro.Console.Domain.Entities;
using Financeiro.Console.Observers;

Console.WriteLine("=== Sistema de Monitoramento com Observer Pattern ===");

var petr4 = new Stock("PETR4", 35.50m);

var investor1 = new Investor("João Silva", 3.0m);
var investor2 = new Investor("Maria Santos", 5.0m);
var mobileApp = new MobileApp("user123");
var tradingBot = new TradingBot("AlgoTrader", 2.0m, 2.5m);

// Subscrição dinâmica
petr4.Attach(investor1);
petr4.Attach(investor2);
petr4.Attach(mobileApp);
petr4.Attach(tradingBot);

Console.WriteLine("\n=== Movimentações do Mercado ===");

petr4.UpdatePrice(36.20m);
Thread.Sleep(500);

petr4.UpdatePrice(37.50m);
Thread.Sleep(500);

// Removendo um observador dinamicamente
Console.WriteLine("\n--- Maria decidiu parar de acompanhar ---");
petr4.Detach(investor2);

petr4.UpdatePrice(35.00m);