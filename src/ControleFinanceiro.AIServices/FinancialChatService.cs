using System.Text;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class FinancialChatService(
    ITransacaoRepository transacaoRepository,
    IConfiguration configuration) : IFinancialChatService
{
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
    private readonly string _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

    public async Task<ChatResponseDto> ResponderConsultaAsync(
        ChatRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Mensagem))
            throw new ArgumentException("A mensagem da consulta não pode estar vazia.", nameof(request));

        // 1. RETRIEVAL: Buscar transações no banco de dados conforme o filtro informado
        var transacoes = (await transacaoRepository.ObterPorPeriodoEContaAsync(
            request.ContaId, 
            request.DataInicio, 
            request.DataFim, 
            cancellationToken)).ToList();

        // 2. CONTEXT BUILDING: Formatar as transações como contexto estruturado para o LLM
        var contextBuilder = new StringBuilder();
        decimal totalGasto = 0m;

        if (transacoes.Count != 0)
        {
            contextBuilder.AppendLine("HISTÓRICO FINANCEIRO DO USUÁRIO:");
            foreach (var t in transacoes)
            {
                var nomeCategoria = t.Categoria?.Nome ?? "Sem Categoria";
                var nomeConta = t.Conta?.Nome ?? "Conta Padrão";
                var tipo = t.Tipo.ToString();
                
                contextBuilder.AppendLine($"- [{t.Data:dd/MM/yyyy}] {t.Descricao} | Tipo: {tipo} | Valor: R$ {t.Valor:N2} | Categoria: {nomeCategoria} | Conta: {nomeConta}");

                if (t.Tipo == Domain.TipoTransacao.Despesa)
                    totalGasto += t.Valor;
            }
        }
        else
        {
            contextBuilder.AppendLine("Nenhuma transação encontrada para o período/filtro informado.");
        }

        // Fallback caso a chave de API não esteja configurada
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "SUA_CHAVE_OPENAI_AQUI")
        {
            return new ChatResponseDto(
                Resposta: $"[MODO DEMO/FALLBACK] Analisei {transacoes.Count} transações. Total de despesas: R$ {totalGasto:N2}. Para respostas inteligentes do GPT-4o, configure sua chave da OpenAI.",
                TotalTransacoesAnalisadas: transacoes.Count,
                TotalGastoAnalisado: totalGasto,
                DataProcessamento: DateTime.UtcNow
            );
        }

        try
        {
            // 3. PROMPT GENERATION: Construir os prompts com injeção de contexto RAG
            var systemPrompt = $@"
Você é um assistente virtual especialista em finanças pessoais.
Sua missão é responder à dúvida do usuário com base EXCLUSIVAMENTE nos dados financeiros fornecidos no contexto abaixo.

Instruções:
1. Seja claro, empático, direto e profissional.
2. Forneça resumos numéricos quando solicitado (ex: totais, médias, maiores gastos).
3. Se o contexto não contiver informações suficientes para responder com precisão, informe isso honestamente ao usuário.
4. Responda em Português do Brasil.
5. Formate valores monetários em Reais (R$).

{contextBuilder}";

            var chatClient = new ChatClient(_model, _apiKey);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(request.Mensagem)
            };

            // 4. GENERATION: Enviar a requisição para o GPT
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            string respostaIa = completion.Content[0].Text;

            return new ChatResponseDto(
                Resposta: respostaIa,
                TotalTransacoesAnalisadas: transacoes.Count,
                TotalGastoAnalisado: totalGasto,
                DataProcessamento: DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO RAG CHAT]: {ex.Message}");
            return new ChatResponseDto(
                Resposta: "Ocorreu um erro ao consultar o assistente financeiro. Tente novamente em instantes.",
                TotalTransacoesAnalisadas: transacoes.Count,
                TotalGastoAnalisado: totalGasto,
                DataProcessamento: DateTime.UtcNow
            );
        }
    }
}