using ControleFinanceiro.Domain;

namespace ControleFinanceiro.Application.DTOs;

public class CriarTransacaoDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public int ContaId { get; set; }
    public int CategoriaId { get; set; }
}

public class TransacaoResponseDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public DateTime Data { get; set; }
    public int ContaId { get; set; }
    public int CategoriaId { get; set; }
}