namespace ControleFinanceiro.Application.DTOs;

public record ReciboExtraidoDto(
    string Estabelecimento,
    decimal ValorTotal,
    DateTime DataTransacao,
    string CategoriaSugerida,
    float Confianca,
    string Observacoes
);