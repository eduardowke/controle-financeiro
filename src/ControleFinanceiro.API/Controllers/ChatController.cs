using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(IFinancialChatService chatService) : ControllerBase
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
}