using OpenAI.Chat;

namespace ControleFinanceiro.Application.Interfaces;

public interface IToolRegistryService
{
    /// <summary>
    /// Retorna a lista de ferramentas (ChatTools) configuradas para a OpenAI.
    /// </summary>
    IReadOnlyList<ChatTool> ObterFerramentasDisponiveis();

    /// <summary>
    /// Executa a função C# correspondente ao nome enviado pela OpenAI.
    /// </summary>
    Task<string> ExecutarFerramentaAsync(
        string nomeFuncao, 
        string argumentosJson, 
        CancellationToken cancellationToken = default);
}