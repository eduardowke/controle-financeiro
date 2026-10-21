using System.Text.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class AICategorizationService : IAICategorizationService
{
    private readonly string _apiKey;
    private readonly string _model;

    public AICategorizationService(IConfiguration configuration)
    {
        _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
    }

    public async Task<AnaliseGastoResponseDto> CategorizarGastoAsync(AnaliseGastoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "SUA_CHAVE_OPENAI_AQUI")
        {
            return SimularRespostaFallback(dto.TextoLivre);
        }

        try
        {
            // Instanciação direta para a API pública oficial da OpenAI (sem Azure)
            var chatClient = new ChatClient(_model, _apiKey);

            var systemPrompt = @"
Você é um assistente de inteligência financeira estrito.
Sua tarefa é analisar descrições em texto livre de gastos ou receitas e extrair os dados em formato JSON estrito.

Regras de Negócio:
1. 'tipoTransacao': Retorne 1 para Receita e 2 para Despesa.
2. 'categoriaSugerida': Identifique uma categoria simples (ex: 'Alimentação', 'Transporte', 'Moradia', 'Lazer', 'Saúde', 'Salário', 'Educação').
3. 'valor': Extraia apenas o valor numérico em decimal (ex: 45.90). Se nenhum valor for informado, retorne 0.
4. 'confianca': Forneça um valor entre 0.0 e 1.0 indicando a certeza da análise.

Retorne APENAS um objeto JSON válido no formato:
{
  ""descricaoSugerida"": ""string"",
  ""valor"": 0.0,
  ""categoriaSugerida"": ""string"",
  ""tipoTransacao"": 2,
  ""confianca"": 0.95
}";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage($"Analise a seguinte transação: \"{dto.TextoLivre}\"")
            };

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
            var jsonResponse = completion.Content[0].Text;

            var resultado = JsonSerializer.Deserialize<AnaliseGastoResponseDto>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return resultado ?? SimularRespostaFallback(dto.TextoLivre);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO IA]: {ex.Message}");
            return SimularRespostaFallback(dto.TextoLivre);
        }
    }

    private static AnaliseGastoResponseDto SimularRespostaFallback(string texto)
    {
        return new AnaliseGastoResponseDto
        {
            DescricaoSugerida = string.IsNullOrWhiteSpace(texto) ? "Transação não identificada" : texto,
            Valor = 0.0m,
            CategoriaSugerida = "Outros",
            TipoTransacao = 2,
            Confianca = 0.50
        };
    }
}