namespace ControleFinanceiro.Domain;

public class Conta
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty; // Ex: "Itaú - Eduardo"
    public string NumBanco { get; set; } = "999";
    public Banco Banco { get; set; } = null!;

    public TipoConta TipoConta { get; set; } = TipoConta.ContaCorrente;
    public string Titular { get; set; } = string.Empty;
    public string? Agencia { get; set; }
    public string? NumConta { get; set; }

    public decimal SaldoInicial { get; set; }
    public bool Ativa { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}