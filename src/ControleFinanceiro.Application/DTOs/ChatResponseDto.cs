namespace ControleFinanceiro.Application.DTOs;

public record ChatResponseDto(
    string Resposta,
    int TotalTransacoesAnalisadas,
    decimal TotalGastoAnalisado,
    DateTime DataProcessamento
);