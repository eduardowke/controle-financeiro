using ControleFinanceiro.Application.Interfaces;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class AdvisorAgent : IAdvisorAgent
{
    private readonly ChatClient _chatClient;

    public AdvisorAgent(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GerarRecomendacoesAsync(string perfil, string analiseRisco, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "Você é o Agent 3 (AdvisorAgent), um consultor financeiro sênior e estrategista de investimentos. " +
                "Sua tarefa é sintetizar o Perfil do Usuário e o Relatório de Riscos gerados pelos agentes anteriores. " +
                "Gere um plano de ação prático com 3 a 5 recomendações prioritárias para otimizar os gastos e sugerir estratégias adequadas de investimentos/reserva. " +
                "Adote um tom encorajador, profissional e direto."),
            new UserChatMessage(
                $"Perfil Financeiro:\n{perfil}\n\n" +
                $"Avaliação de Riscos:\n{analiseRisco}")
        };

        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return response.Value.Content[0].Text;
    }
}