using ControleFinanceiro.Application.Interfaces;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class ClassifierAgent : IClassifierAgent
{
    private readonly ChatClient _chatClient;

    public ClassifierAgent(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> ClassificarPerfilAsync(string contextoTransacoes, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "Você é o Agent 1 (ClassifierAgent), especialista em análise comportamental financeira. " +
                "Sua única tarefa é analisar o histórico de transações fornecido e determinar o perfil financeiro do usuário. " +
                "Mencione o padrão de consumo, a relação entre receitas e despesas, e classifique o perfil (ex: Conservador, Moderado, Propenso ao Impulso, etc.). " +
                "Seja objetivo, claro e forneça uma resposta analítica em até 2 parágrafos."),
            new UserChatMessage($"Histórico de Transações:\n{contextoTransacoes}")
        };

        var response = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return response.Value.Content[0].Text;
    }
}