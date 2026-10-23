using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using OpenAI.Embeddings;

namespace ControleFinanceiro.AIServices;

public class EmbeddingService(string apiKey) : IEmbeddingService
{
    private const string ModelName = "text-embedding-3-small";
    private readonly EmbeddingClient _embeddingClient = new(ModelName, apiKey);

    public async Task<EmbeddingResponseDto> GerarEmbeddingAsync(string texto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("O texto para geração de embedding não pode ser vazio.", nameof(texto));

        OpenAIEmbedding embedding = await _embeddingClient.GenerateEmbeddingAsync(texto, cancellationToken: cancellationToken);

        return new EmbeddingResponseDto(
            TextoOriginal: texto,
            Vetor: embedding.ToFloats(),
            ConsumoTokens: 0
        );
    }

    public async Task<IEnumerable<EmbeddingResponseDto>> GerarEmbeddingsEmLoteAsync(IEnumerable<string> textos, CancellationToken cancellationToken = default)
    {
        var listaTextos = textos.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        
        if (listaTextos.Count == 0)
            return Enumerable.Empty<EmbeddingResponseDto>();

        OpenAIEmbeddingCollection embeddings = await _embeddingClient.GenerateEmbeddingsAsync(listaTextos, cancellationToken: cancellationToken);

        var resultados = new List<EmbeddingResponseDto>();
        for (int i = 0; i < embeddings.Count; i++)
        {
            resultados.Add(new EmbeddingResponseDto(
                TextoOriginal: listaTextos[i],
                Vetor: embeddings[i].ToFloats(),
                ConsumoTokens: 0
            ));
        }

        return resultados;
    }
}