using System.Text.Json;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Domain;
using OpenAI.Chat;

namespace ControleFinanceiro.AIServices;

public class ToolRegistryService(
    ITransacaoService transacaoService,
    IContaService contaService) : IToolRegistryService
{
    public IReadOnlyList<ChatTool> ObterFerramentasDisponiveis()
    {
        // 1. Tool para Criar Transação
        var criarTransacaoTool = ChatTool.CreateFunctionTool(
            functionName: "CriarTransacao",
            functionDescription: "Registra uma nova transação financeira (receita ou despesa) na conta do usuário.",
            functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "descricao": {
                        "type": "string",
                        "description": "Descrição simples da transação (ex: Almoço, Salário, Uber)."
                    },
                    "valor": {
                        "type": "number",
                        "description": "Valor monetário positivo da transação (ex: 45.50)."
                    },
                    "tipo": {
                        "type": "integer",
                        "description": "Tipo da transação: 1 para Despesa, 2 para Receita.",
                        "enum": [1, 2]
                    },
                    "contaId": {
                        "type": "integer",
                        "description": "ID da conta bancária onde a transação será lançada (padrão: 1)."
                    },
                    "categoriaId": {
                        "type": "integer",
                        "description": "ID opcional da categoria (ex: 1 para Alimentação, 2 para Transporte)."
                    }
                },
                "required": ["descricao", "valor", "tipo", "contaId"]
            }
            """)
        );

        // 2. Tool para Consultar Contas e Saldo
        var consultarSaldoTool = ChatTool.CreateFunctionTool(
            functionName: "ConsultarContasESaldos",
            functionDescription: "Obtém a lista de contas cadastradas e seus saldos atuais.",
            functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {}
            }
            """)
        );

        return new List<ChatTool> { criarTransacaoTool, consultarSaldoTool };
    }

    public async Task<string> ExecutarFerramentaAsync(
        string nomeFuncao, 
        string argumentosJson, 
        CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(argumentosJson);
        var root = doc.RootElement;

        return nomeFuncao switch
        {
            "CriarTransacao" => await ExecutarCriarTransacaoAsync(root, cancellationToken),
            "ConsultarContasESaldos" => await ExecutarConsultarContasAsync(cancellationToken),
            _ => JsonSerializer.Serialize(new { erro = $"Ferramenta '{nomeFuncao}' não encontrada." })
        };
    }

    private async Task<string> ExecutarCriarTransacaoAsync(JsonElement root, CancellationToken cancellationToken)
    {
        var descricao = root.GetProperty("descricao").GetString() ?? "Sem descrição";
        var valor = root.GetProperty("valor").GetDecimal();
        var tipoInt = root.GetProperty("tipo").GetInt32();
        var contaId = root.GetProperty("contaId").GetInt32();

        // Trata a nulidade atribuindo o ID 1 (categoria padrão/geral) caso a IA não envie
        int categoriaIdFinal = root.TryGetProperty("categoriaId", out var catProp) && catProp.ValueKind == JsonValueKind.Number 
            ? catProp.GetInt32() 
            : 1;

        var dto = new CriarTransacaoDto
        {
            Descricao = descricao,
            Valor = valor,
            Tipo = (TipoTransacao)tipoInt,
            ContaId = contaId,
            CategoriaId = categoriaIdFinal,
            Data = DateTime.UtcNow
        };

        var resultado = await transacaoService.CriarTransacaoAsync(dto);

        return JsonSerializer.Serialize(new
        {
            sucesso = true,
            mensagem = "Transação registrada com sucesso!",
            transacaoId = resultado.Id,
            resultado.Descricao,
            resultado.Valor
        });
    }

    private async Task<string> ExecutarConsultarContasAsync(CancellationToken cancellationToken)
    {
        var contas = await contaService.ObterTodasAsync();
        return JsonSerializer.Serialize(contas);
    }
}