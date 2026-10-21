namespace ControleFinanceiro.Application.DTOs;

public class AnaliseGastoRequestDto
{
    public string TextoLivre { get; set; } = string.Empty;
}

public class AnaliseGastoResponseDto
{
    public string DescricaoSugerida { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string CategoriaSugerida { get; set; } = string.Empty;
    public int TipoTransacao { get; set; } // 1 = Receita, 2 = Despesa
    public double Confianca { get; set; }  // Ex: 0.95 (95%)
}