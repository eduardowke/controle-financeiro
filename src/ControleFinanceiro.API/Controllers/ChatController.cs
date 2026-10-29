using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(
    IFinancialChatService chatService,
    IToolCallingService toolCallingService) : ControllerBase
{
    /// <summary>
    /// Envia uma pergunta ao Assistente Financeiro RAG com base no histórico do usuário.
    /// </summary>
    [HttpPost("perguntar")]
    public async Task<ActionResult<ChatResponseDto>> Perguntar(
        [FromBody] ChatRequestDto dto,
        CancellationToken cancellationToken)
    {
        var resposta = await chatService.ResponderConsultaAsync(dto, cancellationToken);
        return Ok(resposta);
    }

    /// <summary>
    /// Envia comandos em linguagem natural para que o Agente IA execute ações reais no sistema (Tool Calling).
    /// Exemplo: "Cadastre uma despesa de R$ 45,50 em almoço na conta 1".
    /// </summary>
    [HttpPost("acao")]
    public async Task<ActionResult<ChatResponseDto>> ExecutarAcao(
        [FromBody] ChatRequestDto dto,
        CancellationToken cancellationToken)
    {
        var resposta = await toolCallingService.ProcessarMensagemComAcaoAsync(dto, cancellationToken);
        return Ok(resposta);
    }
}