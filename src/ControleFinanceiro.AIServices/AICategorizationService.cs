using System.Text.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class AICategorizationService(
    IConfiguration configuration,
    IEmbeddingService embeddingService,
    ICategoriaService categoriaService) : IAICategorizationService
{
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
    private readonly string _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
    private const float LimiarMinimoSimilaridade = 0.65f;

    public async Task<ResultadoCategorizacaoDto> CategorizarDescricaoAsync(string descricao, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da transação é obrigatória.", nameof(descricao));

        // -----------------------------------------------------------------------------
        // ETAPA 1: Tentar busca vetorial por similaridade de cosseno (Módulo 3)
        // -----------------------------------------------------------------------------
        try
        {
            var categorias = await categoriaService.ObterTodasAsync();
            
            if (categorias.Any())
            {
                var embeddingDescricao = await embeddingService.GerarEmbeddingAsync(descricao, cancellationToken);
                var nomesCategorias = categorias.Select(c => c.Nome).ToList();
                var embeddingsCategorias = (await embeddingService.GerarEmbeddingsEmLoteAsync(nomesCategorias, cancellationToken)).ToList();

                int melhorCategoriaId = 0;
                string melhorCategoriaNome = string.Empty;
                float maiorSimilaridade = -1.0f;

                for (int i = 0; i < categorias.Count(); i++)
                {
                    var categoria = categorias.ElementAt(i);
                    var embeddingCat = embeddingsCategorias[i];

                    float sim = VectorMath.CosineSimilarity(embeddingDescricao.Vetor.Span, embeddingCat.Vetor.Span);

                    if (sim > maiorSimilaridade)
                    {
                        maiorSimilaridade = sim;
                        melhorCategoriaId = categoria.Id;
                        melhorCategoriaNome = categoria.Nome;
                    }
                }

                // Se a similaridade atingir o limiar de confiança, devolve o resultado vetorial
                if (maiorSimilaridade >= LimiarMinimoSimilaridade)
                {
                    return new ResultadoCategorizacaoDto(
                        melhorCategoriaId,
                        melhorCategoriaNome,
                        maiorSimilaridade,
                        "BuscaVetorial"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AVISO EMBEDDINGS]: Falha na busca vetorial, acionando fallback LLM. Erro: {ex.Message}");
        }

        // -----------------------------------------------------------------------------
        // ETAPA 2: Fallback via Prompt e ChatClient OpenAI (Sua lógica do Módulo 2)
        // -----------------------------------------------------------------------------
        return await CategorizarComLLMFallbackAsync(descricao, cancellationToken);
    }

    private async Task<ResultadoCategorizacaoDto> CategorizarComLLMFallbackAsync(string descricao, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "SUA_CHAVE_OPENAI_AQUI")
        {
            return SimularRespostaFallback(descricao);
        }

        try
        {
            var chatClient = new ChatClient(_model, _apiKey);

            var systemPrompt = @"
Você é um assistente de inteligência financeira estrito.
Sua tarefa é analisar uma descrição em texto livre e identificar a categoria de gasto mais adequada.

Retorne APENAS um objeto JSON no formato:
{
  ""categoriaSugerida"": ""string"",
  ""confianca"": 0.85
}";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage($"Identifique a categoria da transação: \"{descricao}\"")
            };

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var jsonResponse = completion.Content[0].Text;

            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            string categoria = root.TryGetProperty("categoriaSugerida", out var catElem) ? catElem.GetString() ?? "Outros" : "Outros";
            float confianca = root.TryGetProperty("confianca", out var confElem) && confElem.TryGetSingle(out var c) ? c : 0.50f;

            return new ResultadoCategorizacaoDto(
                CategoriaId: null,
                NomeCategoria: categoria,
                ConfiancaIA: confianca,
                OrigemAnalise: "FallbackLLM"
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO IA LLM]: {ex.Message}");
            return SimularRespostaFallback(descricao);
        }
    }

    private static ResultadoCategorizacaoDto SimularRespostaFallback(string texto)
    {
        return new ResultadoCategorizacaoDto(
            CategoriaId: null,
            NomeCategoria: "Outros",
            ConfiancaIA: 0.30f,
            OrigemAnalise: "FallbackSimulado"
        );
    }
}