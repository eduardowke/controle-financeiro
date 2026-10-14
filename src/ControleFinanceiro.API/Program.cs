// using ControleFinanceiro.AIServices;

// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "API do Controle Financeiro Rodando!");

// // Endpoint para executar o laboratório no console
// app.MapGet("/api/lab/tokens", () =>
// {
//     TokenizationLab.ExecutarDemonstracao();
//     return Results.Ok("Demonstração executada com sucesso! Verifique o console do terminal.");
// });

// app.Run();
using ControleFinanceiro.AIServices;

// Executa a demonstração diretamente no terminal sem aguardar requisições HTTP
TokenizationLab.ExecutarDemonstracao();