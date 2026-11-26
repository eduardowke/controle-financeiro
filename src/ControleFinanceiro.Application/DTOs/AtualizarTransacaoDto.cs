using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.DTOs;

public class AtualizarTransacaoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public DateTime Data { get; set; }
    public DateTime? DataCompensacao { get; set; }
    public bool IsPago { get; set; }
    public int CategoriaId { get; set; }
}