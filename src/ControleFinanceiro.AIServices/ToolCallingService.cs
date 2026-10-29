using System.Text.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class ToolCallingService(
    IToolRegistryService toolRegistryService,
    IConfiguration configuration) : IToolCallingService
{
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
    private readonly string _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

    public async Task<ChatResponseDto> ProcessarMensagemComAcaoAsync(
        ChatRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Mensagem))
            throw new ArgumentException("A mensagem não pode estar vazia.", nameof(request));

        // Fallback para modo sem chave de API configurada
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "SUA_CHAVE_OPENAI_AQUI")
        {
            return new ChatResponseDto(
                Resposta: "[MODO DEMO/FALLBACK] O serviço de Tool Calling requer uma chave da OpenAI configurada para executar ações dinâmicas.",
                TotalTransacoesAnalisadas: 0,
                TotalGastoAnalisado: 0m,
                DataProcessamento: DateTime.UtcNow
            );
        }

        try
        {
            var chatClient = new ChatClient(_model, _apiKey);
            var tools = toolRegistryService.ObterFerramentasDisponiveis();

            var options = new ChatCompletionOptions();
            foreach (var tool in tools)
            {
                options.Tools.Add(tool);
            }

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"Você é um assistente financeiro inteligente e autônomo com capacidade de executar ações.
Quando o usuário solicitar o registro de uma despesa/receita ou consulta de saldos, utilize as ferramentas disponíveis (Tool Calling).
Se uma ferramenta for executada com sucesso, confirme a ação para o usuário de forma amigável e direta em Português do Brasil."),
                new UserChatMessage(request.Mensagem)
            };

            // 1. Primeira chamada para o LLM com as ferramentas anexadas
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);

            // Loop de orquestração do Tool Calling
            while (completion.FinishReason == ChatFinishReason.ToolCalls)
            {
                // Adiciona a resposta da IA (com o pedido de execução de tool) ao histórico de mensagens
                messages.Add(new AssistantChatMessage(completion));

                foreach (var toolCall in completion.ToolCalls)
                {
                    // Executa o método correspondente no C#
                    string resultadoTool = await toolRegistryService.ExecutarFerramentaAsync(
                        toolCall.FunctionName, 
                        toolCall.FunctionArguments.ToString(), 
                        cancellationToken);

                    // Adiciona a resposta da ferramenta C# de volta ao histórico
                    messages.Add(new ToolChatMessage(toolCall.Id, resultadoTool));
                }

                // 2. Envia o resultado da execução da ferramenta de volta para a IA finalizar a resposta
                completion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
            }

            return new ChatResponseDto(
                Resposta: completion.Content[0].Text,
                TotalTransacoesAnalisadas: 0,
                TotalGastoAnalisado: 0m,
                DataProcessamento: DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO TOOL CALLING]: {ex.Message}");
            return new ChatResponseDto(
                Resposta: "Ocorreu um erro ao processar sua solicitação de ação financeira. Tente novamente.",
                TotalTransacoesAnalisadas: 0,
                TotalGastoAnalisado: 0m,
                DataProcessamento: DateTime.UtcNow
            );
        }
    }
}