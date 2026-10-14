namespace ControleFinanceiro.Domain;

public class Conta
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty; // Ex: Banco do Brasil, Carteira, Nubank
    public decimal SaldoInicial { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}