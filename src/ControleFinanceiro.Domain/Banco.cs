namespace ControleFinanceiro.Domain;

public class Banco
{
    public string NumBanco { get; set; } = string.Empty; // Chave primária: "001", "341", "260", etc.
    public string NomeReduzido { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    public ICollection<Conta> Contas { get; set; } = new List<Conta>();
}