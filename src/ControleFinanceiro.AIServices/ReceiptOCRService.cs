using System.Text.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class ReceiptOCRService(IConfiguration configuration) : IReceiptOCRService
{
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
    private readonly string _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

    public async Task<ReciboExtraidoDto> ProcessarReciboAsync(
        Stream imagemStream, 
        string contentType, 
        CancellationToken cancellationToken = default)
    {
        if (imagemStream == null || imagemStream.Length == 0)
            throw new ArgumentException("O arquivo de imagem é inválido ou está vazio.", nameof(imagemStream));

        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "SUA_CHAVE_OPENAI_AQUI")
        {
            return SimularRespostaFallback();
        }

        try
        {
            // 1. Converter o Stream da imagem em BinaryData para o SDK da OpenAI
            using var memoryStream = new MemoryStream();
            await imagemStream.CopyToAsync(memoryStream, cancellationToken);
            byte[] imagemBytes = memoryStream.ToArray();
            var imageBinaryData = BinaryData.FromBytes(imagemBytes);

            // 2. Instanciar o cliente de Chat
            var chatClient = new ChatClient(_model, _apiKey);

            var systemPrompt = @"
Você é um sistema especialista em visão computacional e OCR de recibos fiscais.
Sua tarefa é analisar a imagem do comprovante e extrair os dados em formato JSON estrito.

Regras de Extração:
1. 'estabelecimento': Nome do comércio/loja/prestador de serviço. Se não identificar, retorne 'Desconhecido'.
2. 'valorTotal': Valor final pago em formato decimal (ex: 129.90).
3. 'dataTransacao': Data do recibo no formato ISO 'YYYY-MM-DD'. Se não encontrar, use a data atual.
4. 'categoriaSugerida': Identifique uma categoria adequada (ex: 'Alimentação', 'Mercado', 'Combustível', 'Saúde', 'Lazer').
5. 'confianca': Pontuação de 0.0 a 1.0 indicando a clareza da leitura da imagem.
6. 'observacoes': Qualquer informação relevante adicional ou aviso sobre ilegibilidade.

Retorne APENAS um objeto JSON no formato:
{
  ""estabelecimento"": ""string"",
  ""valorTotal"": 0.0,
  ""dataTransacao"": ""YYYY-MM-DD"",
  ""categoriaSugerida"": ""string"",
  ""confianca"": 0.95,
  ""observacoes"": ""string""
}";

            // 3. Criar mensagem multimodal contendo texto e imagem
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Analise o seguinte recibo fiscal e extraia os dados conforme instruído:"),
                    ChatMessageContentPart.CreateImagePart(imageBinaryData, contentType)
                )
            };

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
            };

            // 4. Chamada de API OpenAI Vision
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var jsonResponse = completion.Content[0].Text;

            // 5. Parse e Mapeamento
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            string estabelecimento = root.TryGetProperty("estabelecimento", out var est) ? est.GetString() ?? "Desconhecido" : "Desconhecido";
            decimal valorTotal = root.TryGetProperty("valorTotal", out var val) && val.TryGetDecimal(out var v) ? v : 0.0m;
            
            DateTime dataTransacao = DateTime.UtcNow;
            if (root.TryGetProperty("dataTransacao", out var dt) && DateTime.TryParse(dt.GetString(), out var dataParsed))
            {
                dataTransacao = DateTime.SpecifyKind(dataParsed, DateTimeKind.Utc);
            }

            string categoria = root.TryGetProperty("categoriaSugerida", out var cat) ? cat.GetString() ?? "Outros" : "Outros";
            float confianca = root.TryGetProperty("confianca", out var conf) && conf.TryGetSingle(out var c) ? c : 0.5f;
            string observacoes = root.TryGetProperty("observacoes", out var obs) ? obs.GetString() ?? string.Empty : string.Empty;

            return new ReciboExtraidoDto(
                Estabelecimento: estabelecimento,
                ValorTotal: valorTotal,
                DataTransacao: dataTransacao,
                CategoriaSugerida: categoria,
                Confianca: confianca,
                Observacoes: observacoes
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO OCR VISION]: {ex.Message}");
            return SimularRespostaFallback();
        }
    }

    private static ReciboExtraidoDto SimularRespostaFallback()
    {
        return new ReciboExtraidoDto(
            Estabelecimento: "Mercado Exemplo (Fallback)",
            ValorTotal: 50.00m,
            DataTransacao: DateTime.UtcNow,
            CategoriaSugerida: "Alimentação",
            Confianca: 0.30f,
            Observacoes: "Não foi possível conectar à API de visão. Dados gerados via fallback."
        );
    }
}