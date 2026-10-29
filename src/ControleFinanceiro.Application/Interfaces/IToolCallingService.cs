using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface IToolCallingService
{
    Task<ChatResponseDto> ProcessarMensagemComAcaoAsync(
        ChatRequestDto request, 
        CancellationToken cancellationToken = default);
}