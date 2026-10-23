namespace ControleFinanceiro.Application.DTOs;

/// <summary>
/// DTO de resposta contendo o vetor numérico gerado e métricas de uso de tokens.
/// </summary>
public record EmbeddingResponseDto(
    string TextoOriginal,
    ReadOnlyMemory<float> Vetor,
    int ConsumoTokens
);