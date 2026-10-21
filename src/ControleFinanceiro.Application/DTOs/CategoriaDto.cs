namespace ControleFinanceiro.Application.DTOs;

public class CriarCategoriaDto
{
    public string Nome { get; set; } = string.Empty;
}

public class CategoriaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}