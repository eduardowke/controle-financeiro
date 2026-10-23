using ControleFinanceiro.Application.DTOs;

namespace ControleFinanceiro.Application.Interfaces;

public interface IReceiptOCRService
{
    Task<ReciboExtraidoDto> ProcessarReciboAsync(
        Stream imagemStream, 
        string contentType, 
        CancellationToken cancellationToken = default);
}