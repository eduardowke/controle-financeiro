namespace ControleFinanceiro.Domain;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool IsReceita { get; set; }

    // Relacionamento com Transações
    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}