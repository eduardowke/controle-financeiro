using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;                                                             

public interface IFinancialChatService
{
    Task<ChatResponseDto> ResponderConsultaAsync(
        ChatRequestDto request, 
        CancellationToken cancellationToken = default);
}