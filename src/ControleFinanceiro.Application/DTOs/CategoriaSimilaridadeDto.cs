namespace ControleFinanceiro.Application.DTOs;

/// <summary>
/// DTO utilizado para retornar a categoria encontrada 
/// via similaridade vetorial e seu score.
/// </summary>
public record CategoriaSimilaridadeDto(
    int CategoriaId,
    string NomeCategoria,
    float PontuacaoSimilaridade
);