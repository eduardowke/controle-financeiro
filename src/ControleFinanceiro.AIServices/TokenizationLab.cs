using Microsoft.ML.Tokenizers;

namespace ControleFinanceiro.AIServices;

public class TokenizationLab
{
    public static void ExecutarDemonstracao()
    {
        Console.WriteLine("\n====================================================");
        Console.WriteLine("=== LABORATÓRIO DE TOKENS E EMBEDDINGS (MÓDULO 1) ===");
        Console.WriteLine("====================================================\n");

        try
        {
            // Instanciando o tokenizador do Tiktoken
            var tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");

            string descricaoTransacao = "Pagamento da fatura do cartão de crédito Nubank R$ 450,00";

            var tokens = tokenizer.EncodeToTokens(descricaoTransacao, out _);

            Console.WriteLine($"Texto original: \"{descricaoTransacao}\"");
            Console.WriteLine($"Total de Tokens gerados: {tokens.Count}\n");

            Console.WriteLine("--- Detalhamento dos Tokens ---");
            foreach (var token in tokens)
            {
                Console.WriteLine($"ID: {token.Id,-8} | Texto: \"{token.Value}\"");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO AO EXECUTAR TOKENIZADOR]: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Detalhe: {ex.InnerException.Message}");
            }
        }

        Console.WriteLine("\n====================================================\n");
    }
}