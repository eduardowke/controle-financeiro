using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface IEmbeddingService
{
    /// <summary>
    /// Gera o vetor numérico (embedding) a partir de um texto descritivo.
    /// </summary>
    Task<EmbeddingResponseDto> GerarEmbeddingAsync(string texto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gera embeddings em lote para otimizar chamadas de API ao processar múltiplas descrições.
    /// </summary>
    Task<IEnumerable<EmbeddingResponseDto>> GerarEmbeddingsEmLoteAsync(IEnumerable<string> textos, CancellationToken cancellationToken = default);
}