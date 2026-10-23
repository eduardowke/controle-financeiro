namespace ControleFinanceiro.Application.DTOs;

/// <summary>
/// DTO de resposta detalhada com o resultado da categorização efetuada pela IA.
/// </summary>
public record ResultadoCategorizacaoDto(
    int? CategoriaId,
    string NomeCategoria,
    float ConfiancaIA,
    string OrigemAnalise // Ex: "BuscaVetorial" ou "FallbackLLM"
);