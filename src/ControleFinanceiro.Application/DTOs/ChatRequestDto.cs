namespace ControleFinanceiro.Application.DTOs;

public class ChatRequestDto
{
    public required string Mensagem { get; set; }
    public int? ContaId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}