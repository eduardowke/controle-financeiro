namespace ControleFinanceiro.Domain;

public class Transacao
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public DateTime Data { get; set; }
    public bool IsPago { get; set; }

    // Chaves Estrangeiras (Relacionamentos)
    public int ContaId { get; set; }
    public Conta Conta { get; set; } = null!;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    // Novas propriedades para suporte à IA e rastreabilidade
    public double? ConfiancaIA { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}