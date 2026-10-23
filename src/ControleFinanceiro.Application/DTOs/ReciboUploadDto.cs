namespace ControleFinanceiro.Application.DTOs;

public class ReciboUploadDto
{
    public required Stream ConteudoArquivo { get; set; }
    public required string NomeArquivo { get; set; }
    public required string ContentType { get; set; }
    public int ContaId { get; set; }
    public int CategoriaIdPadrao { get; set; }
}