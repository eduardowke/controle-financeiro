namespace ControleFinanceiro.Application.DTOs;

public class BancoResponseDto
{
    public string NumBanco { get; set; } = string.Empty;
    public string NomeReduzido { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}