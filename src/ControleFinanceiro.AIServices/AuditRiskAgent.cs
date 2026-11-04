using ControleFinanceiro.Application.Interfaces;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class AuditRiskAgent : IAuditRiskAgent
{
    private readonly ChatClient _chatClient;

    public AuditRiskAgent(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> AuditarRiscosAsync(string contextoTransacoes, string perfilClassificado, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "Você é o Agent 2 (AuditRiskAgent), auditor especialista em risco financeiro e detecção de anomalias. " +
                "Sua tarefa é analisar o histórico de transações em conjunto com o Perfil Financeiro identificado pelo Classificador. " +
                "Identifique riscos de endividamento, concentração excessiva em categorias não essenciais, ausência de reserva de emergência ou desvios de orçamento. " +
                "Forneça um relatório sucinto de riscos em tópicos claros."),
            new UserChatMessage(
                $"Perfil Identificado:\n{perfilClassificado}\n\n" +
                $"Histórico de Transações:\n{contextoTransacoes}")
        };

        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return response.Value.Content[0].Text;
    }
}