using OpenAI.Chat;

public class AgenteFinanceiro
{
    private readonly ChatClient _chatClient;
    private readonly List<ChatTool> _ferramentas;

    public AgenteFinanceiro(string apiKey)
    {
        _chatClient = new ChatClient("gpt-4o", apiKey);
        
        // 1. Definimos a lista de ferramentas que o C# oferece para a IA
        _ferramentas = new List<ChatTool>
        {
            ChatTool.CreateFunctionTool(
                functionName: "ConsultarSaldo",
                functionDescription: "Retorna o saldo atual do usuário no banco."
            )
        };
    }

    public async Task<string> ProcessarMensagemAsync(string mensagemUsuario)
    {
        // Guardamos o histórico da conversa
        var historico = new List<ChatMessage>
        {
            new UserChatMessage(mensagemUsuario)
        };

        // LOOP DE REAÇÃO (Inicia o ciclo de decisão)
        while (true)
        {
            // Envia o histórico + as ferramentas disponíveis para o LLM
            ChatCompletionOptions opcoes = new() { Tools = { _ferramentas[0] } };
            ChatCompletion resposta = await _chatClient.CompleteChatAsync(historico, opcoes);

            // CENÁRIO A: O LLM decidiu que precisa Chamar uma Ferramenta (Tool)
            if (resposta.FinishReason == ChatFinishReason.ToolCalls)
            {
                // Adiciona a intenção da IA ao histórico
                historico.Add(new AssistantChatMessage(resposta));

                foreach (var toolCall in resposta.ToolCalls)
                {
                    // 2. O C# executa a função local de acordo com o nome pedido pela IA
                    string resultadoExecucao = ExecutarFerramentaLocal(toolCall.FunctionName);

                    // 3. O C# devolve o resultado da ferramenta para o histórico da IA
                    historico.Add(new ToolChatMessage(toolCall.Id, resultadoExecucao));
                }

                // O loop WHILE repete! Envia o resultado de volta para a IA analisar.
            }
            // CENÁRIO B: O LLM concluiu o raciocínio e deu a resposta final em texto
            else
            {
                return resposta.Content[0].Text; // Sai do loop!
            }
        }
    }

    // Método C# tradicional com a regra de negócio
    private string ExecutarFerramentaLocal(string nomeFuncao)
    {
        if (nomeFuncao == "ConsultarSaldo")
        {
            // Simulação de busca no Banco de Dados via Entity Framework
            decimal saldoAtual = 150.00m; 
            return $"{{\"saldo\": {saldoAtual}}}";
        }

        return "{\"erro\": \"Ferramenta desconhecida\"}";
    }
}